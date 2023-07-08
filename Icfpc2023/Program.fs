open System
open System.IO
open System.Text
open System.Threading.Tasks

open Icfpc2023
open Icfpc2023.HttpApi

let inline (|Parse|_|) (str: string) : 'a option =
    let mutable value = Unchecked.defaultof<'a>
    let result = (^a: (static member TryParse: string * byref< ^a> -> bool) str, &value)
    if result then Some value
    else None

let solutionDirectory =
    let mutable directory = Environment.CurrentDirectory
    while not(isNull directory) && not <| File.Exists(Path.Combine(directory, "Icfpc2023.sln")) do
        directory <- Path.GetDirectoryName directory
    if isNull directory then
        failwith $"Cannot find root solution dir starting from path \"{Environment.CurrentDirectory}\"."
    directory

let problemsDir = Path.Combine(solutionDirectory, "problems")
let solutionsDir = Path.Combine(solutionDirectory, "solutions")

let runSynchronously(t: Task<'a>) =
    t.GetAwaiter().GetResult()

let runSynchronouslyV(t: Task) =
    t.GetAwaiter().GetResult()

let readToken() =
    let tokenFile = Path.Combine(solutionDirectory, "token.txt")
    if not <| File.Exists tokenFile then
        failwith $"Please save the access token to file \"{tokenFile}\"."
    File.ReadAllText(tokenFile).Trim()

let nondeterministic_solvers = Map [
    "randomDummyV1", DummySolver.RandomDummyV1
    "randomDummyV2", DummySolver.RandomDummyV2
]

let solvers = Map(Seq.concat [
        seq {
            "dummyV1", DummySolver.SolveV1
            "dummyV2", DummySolver.SolveV2
            // "lambda", LambdaSolver.Solve
            "foxtranV1", FoxtranSolver.FoxtranSolveV1
        };
        Map.toSeq nondeterministic_solvers
    ])

let readProblem (problemId: int) =
    let problemFile = Path.Combine(problemsDir, $"{problemId}.json")
    JsonDefs.ReadProblemFromFile problemFile

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
        printf $"Trying to solve problem {problemId} with solver {solverName}... "
        let problem = readProblem problemId
        let solver = solvers[solverName]
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
        nondeterministic_solvers
        |> Map.toSeq
        |> Seq.map (fun (solverName, _) -> solveWithSolver problemId solverName)
        |> Seq.maxBy (fun (_, solutionMetadata) -> solutionMetadata.Score)
    | _ ->
        solveWithSolver problemId solverName

let solveCommand (problemId: int) (solverName: SolverName) (preserveBest: bool) =
    printfn $"Solving problem {problemId}..."
    let solution, solutionMetadata = solve problemId solverName
    let newSolver = solutionMetadata.SolverName

    match tryReadSolution problemId with
    | Some (_, oldSolutionMetadata) ->
        let newScore = solutionMetadata.Score
        let oldScore = oldSolutionMetadata.Score
        let oldSolver = oldSolutionMetadata.SolverName
        let diff = newScore - oldScore
        let diff_percent = 100.0 * (diff / oldScore)
        printfn $"Score for problem {problemId}: {oldScore} ({oldSolver}) -> {newScore} ({newSolver}) (%+f{diff} / %+.0f{diff_percent}%%)"

        if not preserveBest then
            printfn $"Writing solution for problem {problemId}..."
            writeSolution problemId solution solutionMetadata
        elif preserveBest && (newScore > oldScore) then
            printfn $"Writing best solution for problem {problemId}..."
            writeSolution problemId solution solutionMetadata
        else
            printfn $"Do nothing!"

    | None ->
        printfn $"Score for problem {problemId}: {solutionMetadata.Score}"
        printfn $"Writing solution for problem {problemId}..."
        writeSolution problemId solution solutionMetadata

let solveAllCommand (solverName: SolverName) (preserveBest: bool) =
    let problems = Directory.GetFiles(problemsDir, "*.json")
    for problem in problems do
        let problemId = Path.GetFileNameWithoutExtension problem |> int
        solveCommand problemId solverName preserveBest

let convertIni problemFile =
    let problem = JsonDefs.ReadProblemFromFile problemFile
    let problemId = int <| Path.GetFileNameWithoutExtension problemFile
    let solution =
        tryReadSolution problemId
        |> Option.map fst

    let ini = Converter.ToIni problem solution
    let examplesDir = Path.Combine(solutionDirectory, "examples")
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

    | [| "score"; Parse(problemId) |] ->
        match tryReadSolution problemId with
        | Some(_, solutionMetadata) -> printfn $"Score: {string solutionMetadata.Score}"
        | _ -> printfn $"Problem {problemId} is not solved yet!"

    | [| "recalculate-score"; Parse(problemId) |] ->
        let problem = readProblem problemId
        match tryReadSolution problemId with
        | Some(solution, solutionMetadata) ->
            let updatedSolutionMetadata = {
                Score = Scoring.CalculateScore problem solution
                SolverName = solutionMetadata.SolverName
            }
            writeSolution problemId solution updatedSolutionMetadata
        | _ -> printfn $"Problem {problemId} is not solved yet!"

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

    | _ -> printfn "Command unrecognized."

    0
