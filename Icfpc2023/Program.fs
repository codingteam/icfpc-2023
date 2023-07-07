open System
open System.IO
open System.Text
open System.Threading.Tasks

open Icfpc2023
open Icfpc2023.HttpApi

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

let readProblem i =
    let problemFile = Path.Combine(problemsDir, $"{i}.json")
    JsonDefs.ReadProblemFromFile problemFile

let readSolution i =
    let solutionFile = Path.Combine(solutionsDir, $"{i}.json")
    JsonDefs.ReadSolutionFromFile solutionFile

let writeSolution i solution =
    let solutionFile = Path.Combine(solutionsDir, $"{i}.json")
    let solutionText = JsonDefs.WriteSolutionToJson solution
    File.WriteAllText(solutionFile, solutionText)

let writeSolutionScore i score =
    let solutionScoreFile = Path.Combine(solutionsDir, $"{i}.score.json")
    let solutionScoreText = JsonDefs.WriteSolutionScoreToJson score
    File.WriteAllText(solutionScoreFile, solutionScoreText)

let getExistingScore i problem =
    let solutionScoreFile = Path.Combine(solutionsDir, $"{i}.score.json")
    try
        JsonDefs.ReadSolutionScoreFromFile solutionScoreFile
    with
    | :? System.IO.FileNotFoundException ->
            let oldSolution = readSolution i
            let score = Scoring.CalculateScore problem oldSolution
            writeSolutionScore i score
            score

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
    | [|"download"; numStr|] ->
        let num = int numStr
        Directory.CreateDirectory problemsDir |> ignore
        let content = runSynchronously <| DownloadProblem num
        let filePath = Path.Combine(problemsDir, $"{string num}.json")
        printfn $"Downloading problem {num} to \"{filePath}\"…"
        File.WriteAllText(filePath, content)

    | [| "solve"; numStr; solverStr |] ->
        let num = int numStr
        let problem = readProblem num
        let solver = solvers[solverStr]
        let solution = solver problem
        writeSolution num solution

    | [| "solveBest"; numStr; solverStr |] ->
        let num = int numStr
        let problem = readProblem num

        let oldSolution = readSolution num
        let oldScore = getExistingScore num problem

        let solver = solvers[solverStr]
        let newSolution = solver problem
        let newScore = Scoring.CalculateScore problem newSolution

        let diff = newScore - oldScore
        let diff_percent = 100.0 * (diff / oldScore)
        printfn $"Score for problem {num}: {oldScore} -> {newScore} (%+f{diff} / %+.0f{diff_percent}%%)"
        if newScore > oldScore then
            printfn $"Writing solution..."
            writeSolution num newSolution
        else
            printfn "Do nothing!"

    | [| "score"; numStr |] ->
        let num = int numStr
        let problem = readProblem num
        let solution = readSolution num
        let score = Scoring.CalculateScore problem solution
        printfn $"Score: {string score}"

    | [|"upload"; "all"|] ->
        let token = readToken()
        let solutions = Directory.GetFiles(solutionsDir, "*.json")
        for solution in solutions do
            printfn $"Uploading solution \"{Path.GetFileName solution}\"…"
            let problemNumber = Path.GetFileNameWithoutExtension solution |> int
            let submission = { ProblemId = problemNumber; Contents = File.ReadAllText(solution) }
            runSynchronouslyV <| Upload(submission, token)

    | [| "lambdaScore" |] ->
        printfn "Nothing."

    | _ -> printfn "Command unrecognized."

    0
