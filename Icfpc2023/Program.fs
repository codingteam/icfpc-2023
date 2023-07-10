open System
open System.IO
open System.Text
open System.Threading
open System.Threading.Tasks

open Icfpc2023
open Icfpc2023.DirectoryLookup
open Icfpc2023.HttpApi
open Icfpc2023.Solvers

let inline (|Parse|_|) (str: string) : 'a option =
    let mutable value = Unchecked.defaultof<'a>
    let result = (^a: (static member TryParse: string * byref< ^a> -> bool) str, &value)
    if result then Some value
    else None

let runSynchronously(t: Task<'a>) =
    t.GetAwaiter().GetResult()

let runSynchronouslyV(t: Task) =
    t.GetAwaiter().GetResult()

let readToken() =
    let tokenFile = Path.Combine(solutionDirectory, "token.txt")
    if not <| File.Exists tokenFile then
        failwith $"Please save the access token to file \"{tokenFile}\"."
    File.ReadAllText(tokenFile).Trim()

let nondeterministicSolvers = Map [
    "randomDummyV1", DummySolver.RandomDummyV1
    "randomDummyV2", DummySolver.RandomDummyV2
    "derfree_hor", DerFreeSolver.SolveHorizontal None
]

let experimentalSolvers = Map [
    "lambda", LambdaSolver.Solve None
    "derfree", DerFreeSolver.Solve None
]

let improvementSolvers = Map [
    "lambda", LambdaSolver.Solve
    "derfree", DerFreeSolver.Solve
    "derfree_hor", DerFreeSolver.SolveHorizontal
    "suosi", ShapeUpOrShipOut.Solve
    "swap", SwapSolver.Solve
    "swap_hor", SwapSolver.SolveHorizontal
]

let solvers = Map(Seq.concat [
        seq {
            "dummyV1", DummySolver.SolveV1
            "dummyV2", DummySolver.SolveV2
            "foxtranV1", FoxtranSolver.FoxtranSolveV1
            "foxtranV2", FoxtranSolver.FoxtranSolveV2
        };
        Map.toSeq nondeterministicSolvers
    ])

let inline (.+) (xs: 'T[]) (ys: 'T[]) =
    (xs, ys) ||> Array.map2 (fun x y -> x + y)

let postProcessProblem (problem: Problem) =
    let processedAttendees =
        problem.Attendees
        |> Seq.groupBy (fun attendee -> attendee.X, attendee.Y)
        |> Seq.map (fun (pos, attendees) -> pos, attendees |> Seq.map (fun a -> a.Tastes))
        |> Seq.map (fun (pos, tastesSeq) -> pos, tastesSeq |> Seq.reduce (.+))
        |> Seq.map (fun ((x, y), tastes) -> { X = x; Y = y; Tastes = tastes })
        |> Seq.toArray

    { problem with Attendees = processedAttendees }

let readProblem (problemId: int) =
    let problemFile = Path.Combine(problemsDir, $"{problemId}.json")
    JsonDefs.ReadProblemFromFile problemFile |> postProcessProblem

let tryReadSolution (problemId: int): (Solution * SolutionMetadata) option =
    let solutionFile = Path.Combine(solutionsDir, $"{problemId}.json")
    let solutionMetadataFile = Path.Combine(solutionsDir, $"{problemId}.meta.json")
    try
        let solution = JsonDefs.ReadSolutionFromFile solutionFile
        let solutionMetadata = JsonDefs.ReadSolutionMetadataFromFile solutionMetadataFile
        Some (solution, solutionMetadata)
    with :? FileNotFoundException ->
        None

let writeSolution (problemId: int) solution solutionMetadata =
    let solutionFile = Path.Combine(solutionsDir, $"{problemId}.json")
    let solutionMetadataFile = Path.Combine(solutionsDir, $"{problemId}.meta.json")
    let solutionText = JsonDefs.WriteSolutionToJson solution
    let solutionMetadataText = JsonDefs.WriteSolutionMetadataToJson solutionMetadata
    File.WriteAllText(solutionFile, solutionText)
    File.WriteAllText(solutionMetadataFile, solutionMetadataText)

let solve (problemId: int) (solverName: SolverName) =
    let solveWithSolver (problemId: int) (solverName: SolverName) =
        printfn $"Trying to solve problem {problemId} with solver {solverName}..."
        let problem = readProblem problemId
        let solver =
            solvers
            |> Map.tryFind solverName
            |> Option.orElseWith (fun () -> experimentalSolvers |> Map.tryFind solverName)
            |> Option.get // TODO: unsafe. gsomix
        let solution = solver problem
        let score = Scoring.CalculateScore problem solution
        printfn $"On problem {problemId}, solver {solverName} yielded a score of {score}"
        let solutionMetadata = {
            Score = score
            SolverName = solverName
        }
        solution, solutionMetadata

    match solverName with
    | "best" ->
        solvers
        |> Map.toSeq
        |> Seq.map (fun (solverName, _) -> solveWithSolver problemId solverName)
        |> Seq.maxBy (fun (_, solutionMetadata) -> solutionMetadata.Score)
    | "best-nondeterministic" ->
        nondeterministicSolvers
        |> Map.toSeq
        |> Seq.map (fun (solverName, _) -> solveWithSolver problemId solverName)
        |> Seq.maxBy (fun (_, solutionMetadata) -> solutionMetadata.Score)
    | _ ->
        solveWithSolver problemId solverName

let private StoreReceivedSolution problemId newSolution newSolutionMetadata preserveBest =
    match tryReadSolution problemId with
    | Some (_, oldSolutionMetadata) ->
        let newScore = newSolutionMetadata.Score
        let oldScore = oldSolutionMetadata.Score
        let oldSolver = oldSolutionMetadata.SolverName
        let diff = newScore - oldScore
        let diff_percent = 100.0 * (diff / abs(oldScore))
        printfn $"Score for problem {problemId}: {oldScore} ({oldSolver}) -> {newScore} ({newSolutionMetadata.SolverName}) (%+f{diff} / %+.0f{diff_percent}%%)"

        if not preserveBest then
            printfn $"Writing solution for problem {problemId}..."
            writeSolution problemId newSolution newSolutionMetadata
        elif preserveBest && (newScore > oldScore) then
            printfn $"Writing best solution for problem {problemId}..."
            writeSolution problemId newSolution newSolutionMetadata
        else
            printfn "Do nothing!"

    | None ->
        printfn $"Score for problem {problemId}: {newSolutionMetadata.Score}"
        printfn $"Writing solution for problem {problemId}..."
        writeSolution problemId newSolution newSolutionMetadata

let solveCommand (problemId: int) (solverName: SolverName) (preserveBest: bool): unit =
    printfn $"Solving problem {problemId}..."
    let solution, solutionMetadata = solve problemId solverName
    let newSolver = solutionMetadata.SolverName
    StoreReceivedSolution problemId solution solutionMetadata preserveBest

let improveCommand (problemId: int) (solverName: SolverName) (preserveBest: bool) =
    let improveWithSolver (problemId: int) (solverName: SolverName) (initialSolution: Solution) =
        printfn $"Trying to improve problem {problemId} with solver {solverName}..."
        let problem = readProblem problemId
        let solver = improvementSolvers[solverName]
        let solution = solver (Some initialSolution) problem
        let score = Scoring.CalculateScore problem solution
        printfn $"On problem {problemId}, solver {solverName} yielded a score of {score}"
        let solutionMetadata = {
            Score = score
            SolverName = solverName
        }
        solution, solutionMetadata

    printfn $"Improving problem {problemId}..."
    match tryReadSolution problemId with
    | Some (oldSolution, oldSolutionMetadata) ->
        let newSolution, newSolutionMetadata = improveWithSolver problemId solverName oldSolution
        let newSolver = newSolutionMetadata.SolverName

        let newScore = newSolutionMetadata.Score
        let oldScore = oldSolutionMetadata.Score
        let oldSolver = oldSolutionMetadata.SolverName
        let diff = newScore - oldScore
        let diff_percent = 100.0 * (diff / abs(oldScore))
        printfn $"Score for problem {problemId}: {oldScore} ({oldSolver}) -> {newScore} ({newSolver}) (%+f{diff} / %+.0f{diff_percent}%%)"

        if not preserveBest then
            printfn $"Writing solution for problem {problemId}..."
            writeSolution problemId newSolution newSolutionMetadata
        elif preserveBest && (newScore > oldScore) then
            printfn $"Writing best solution for problem {problemId}..."
            writeSolution problemId newSolution newSolutionMetadata
        else
            printfn $"Do nothing!"

    | None ->
        printfn $"No solution found. There is nothing to improve!"

let solveAllCommand (solverName: SolverName) (preserveBest: bool) =
    let problems = Directory.GetFiles(problemsDir, "*.json")
    for problem in problems do
        let problemId = Path.GetFileNameWithoutExtension problem |> int
        solveCommand problemId solverName preserveBest

let recalculateScoreCommand (problemId: int) =
    let problem = readProblem problemId
    match tryReadSolution problemId with
    | Some(solution, solutionMetadata) ->
        let updatedSolutionMetadata = {
            Score = Scoring.CalculateScore problem solution
            SolverName = solutionMetadata.SolverName
        }
        writeSolution problemId solution updatedSolutionMetadata
    | _ -> printfn $"Problem {problemId} is not solved yet!"

let recalculateScoreAllCommand () =
    let problems = Directory.GetFiles(problemsDir, "*.json")
    let mutable finished = 0
    Parallel.ForEach(problems, fun (problem: string) ->
        let problemId = Path.GetFileNameWithoutExtension problem |> int
        recalculateScoreCommand problemId
        let res = Interlocked.Increment(&finished)
        printfn $"Finished {res} / {problems.Length} problems"
    ) |> ignore

let convertIni problemFile =
    let problem = JsonDefs.ReadProblemFromFile problemFile |> postProcessProblem
    let problemId = int <| Path.GetFileNameWithoutExtension problemFile
    let solution =
        tryReadSolution problemId
        |> Option.map fst

    let ini = Converter.ToIni problem solution
    Directory.CreateDirectory examplesDir |> ignore
    let iniFile = Path.Combine(examplesDir, $"{problemId}.ini")
    File.WriteAllText(iniFile, ini)

[<EntryPoint>]
let main(args: string[]): int =
    Console.OutputEncoding <- Encoding.UTF8

    match args with
    | [| "download"; "all" |] ->
        runSynchronouslyV <| task {
            Directory.CreateDirectory problemsDir |> ignore
            let! count = GetProblemCount()
            for i in 1..count do
                let! content = DownloadProblem i
                let filePath = Path.Combine(problemsDir, $"{string i}.json")
                printfn $"Downloading problem {i} to \"{filePath}\"…"
                File.WriteAllText(filePath, content)
        }

    | [| "download"; Parse(problemId) |] ->
        Directory.CreateDirectory problemsDir |> ignore
        let content = runSynchronously <| DownloadProblem problemId
        let filePath = Path.Combine(problemsDir, $"{string problemId}.json")
        printfn $"Downloading problem {problemId} to \"{filePath}\"…"
        File.WriteAllText(filePath, content)

    | [| "solve"; Parse(problemId); solverName |] ->
        solveCommand problemId solverName false

    | [| "solve"; Parse(problemId); solverName; "--preserve-best" |] ->
        solveCommand problemId solverName true

    | [| "solve"; "all"; solverName |] ->
        solveAllCommand solverName false

    | [| "solve"; "all"; solverName; "--preserve-best" |] ->
        solveAllCommand solverName true

    | [| "improve"; Parse(problemId); solverName |] ->
        improveCommand problemId solverName false

    | [| "improve"; Parse(problemId); solverName; "--preserve-best" |] ->
        improveCommand problemId solverName true

    | [| "find"; Parse(problemId); solverName |] ->
        while true do
            solveCommand problemId solverName true

    | [| "score"; Parse(problemId) |] ->
        match tryReadSolution problemId with
        | Some(_, solutionMetadata) -> printfn $"Score: {string solutionMetadata.Score}"
        | _ -> printfn $"Problem {problemId} is not solved yet!"

    | [| "recalculate-score"; Parse(problemId) |] ->
        recalculateScoreCommand problemId

    | [| "recalculate-score"; "all" |] ->
        recalculateScoreAllCommand()

    | [| "upload"; Parse(problemId) |] ->
        let token = readToken()
        let filename = Path.Combine(solutionsDir, $"{problemId}.json")
        printfn $"Uploading solution \"{filename}\"…"
        let submission = { ProblemId = problemId; Contents = File.ReadAllText(filename) }
        runSynchronouslyV <| Upload(submission, token)

    | [| "upload"; "all" |] ->
        let token = readToken()
        let solutions = Directory.GetFiles(solutionsDir, "*.json")
        for solution in solutions do
            let filename = Path.GetFileName solution
            if not(filename.EndsWith(".meta.json")) then
                printfn $"Uploading solution \"{filename}\"…"
                let problemNumber = Path.GetFileNameWithoutExtension solution |> int
                let submission = { ProblemId = problemNumber; Contents = File.ReadAllText(solution) }
                runSynchronouslyV <| Upload(submission, token)

    | [| "convertini"; "all" |] ->
        for problemFile in Directory.GetFiles problemsDir do
            convertIni problemFile

    | [| "convertini"; problemId |] ->
        let problemFile = Path.Combine(problemsDir, $"{problemId}.json")
        convertIni problemFile

    | [| "lambdaScore" |] ->
        printfn "Nothing."

    | [| "vulpes"; "all" |] ->
        for item in Directory.GetFiles(examplesDir, "*.ini.new") do
            let problemId = Path.GetFileNameWithoutExtension item |> Path.GetFileNameWithoutExtension |> int
            printfn $"Reading problem {problemId}…"
            let problem = readProblem problemId
            let solution = VulpesImporter.ImportIni problemId
            printfn $"Scoring problem {problemId}…"
            let metadata = { Score = Scoring.CalculateScore problem solution; SolverName = "vulpes" }
            printfn $"Trying to store solution for {problemId}…"
            StoreReceivedSolution problemId solution metadata true

    | [| "volumeUp" |] ->
        let solutions = Directory.GetFiles(solutionsDir, "*.json")
        for solution in solutions do
            if not(solution.EndsWith(".meta.json")) then
                let problemId = Path.GetFileNameWithoutExtension solution |> int
                let solutionFile = Path.Combine(solutionsDir, $"{problemId}.json")
                let solution = JsonDefs.ReadSolutionLegacyFromFile solutionFile
                let solutionText = JsonDefs.WriteSolutionToJson solution
                File.WriteAllText(solutionFile, solutionText)

    | _ -> printfn "Command unrecognized."

    0
