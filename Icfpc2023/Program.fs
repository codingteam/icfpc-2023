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

let solvers = Map [
    "dummy", DummySolver.Solve
]

let readProblem (problemId: int) =
    let problemFile = Path.Combine(problemsDir, $"{problemId}.json")
    JsonDefs.ReadProblemFromFile problemFile

let writeSolutionMetadata (problemId: int) metadata =
    let solutionMetadataFile = Path.Combine(solutionsDir, $"{problemId}.meta.json")
    let solutionMetadataText = JsonDefs.WriteSolutionMetadataToJson metadata
    File.WriteAllText(solutionMetadataFile, solutionMetadataText)

let readSolutionMetadata (problemId: int) =
    let solutionMetadataFile = Path.Combine(solutionsDir, $"{problemId}.meta.json")
    JsonDefs.ReadSolutionMetadataFromFile solutionMetadataFile

let readSolution (problemId: int) =
    let solutionFile = Path.Combine(solutionsDir, $"{problemId}.json")
    let solutionMetadata = readSolutionMetadata problemId
    let solution = JsonDefs.ReadSolutionFromFile solutionFile
    solution, solutionMetadata

let writeSolution (problemId: int) solution solutionMetadata =
    let solutionFile = Path.Combine(solutionsDir, $"{problemId}.json")
    let solutionText = JsonDefs.WriteSolutionToJson solution
    File.WriteAllText(solutionFile, solutionText)
    writeSolutionMetadata problemId solutionMetadata

let solve (problemId: int) (solverName: Solver) =
    let problem = readProblem problemId
    let solver = solvers[solverName]
    let solution = solver problem
    let score = Scoring.CalculateScore problem solution
    let solutionMetadata = {
        Score = score
        Solver = solverName
    }
    writeSolution problemId solution solutionMetadata

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
        solve problemId solverName

    | [| "solve"; "all"; solverName |] ->
        let problems = Directory.GetFiles(problemsDir, "*.json")
        for problem in problems do
            let problemId = Path.GetFileNameWithoutExtension problem |> int
            printfn $"Solving problem \"{problemId}\"…"
            solve problemId solverName

    | [| "solveBest"; Parse(problemId); solverName |] ->
        let problem = readProblem problemId

        let _, oldSolutionMetadata = readSolution problemId
        let oldScore = oldSolutionMetadata.Score
        let oldSolver = oldSolutionMetadata.Solver

        let solver = solvers[solverName]
        let newSolution = solver problem
        let newScore = Scoring.CalculateScore problem newSolution

        let diff = newScore - oldScore
        let diff_percent = 100.0 * (diff / oldScore)
        printfn $"Score for problem {problemId}: {oldScore} ({oldSolver}) -> {newScore} ({solverName}) (%+f{diff} / %+.0f{diff_percent}%%)"
        if newScore > oldScore then
            printfn $"Writing solution..."
            let solutionMetadata = {
                Score = newScore
                Solver = solverName
            }
            writeSolution problemId newSolution solutionMetadata
        else
            printfn "Do nothing!"

    | [| "score"; Parse(problemId) |] ->
        let _, solutionMetadata = readSolution problemId
        printfn $"Score: {string solutionMetadata.Score}"

    | [| "upload"; "all" |] ->
        let token = readToken()
        let solutions = Directory.GetFiles(solutionsDir, "*.json")
        for solution in solutions do
            let filename = Path.GetFileName solution
            if not(filename.EndsWith(".score.json")) then
                printfn $"Uploading solution \"{filename}\"…"
                let problemNumber = Path.GetFileNameWithoutExtension solution |> int
                let submission = { ProblemId = problemNumber; Contents = File.ReadAllText(solution) }
                runSynchronouslyV <| Upload(submission, token)

    | [| "lambdaScore" |] ->
        printfn "Nothing."

    | _ -> printfn "Command unrecognized."

    0
