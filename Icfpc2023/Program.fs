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
        let problemFile = Path.Combine(problemsDir, $"{num}.json")
        let problem = JsonDefs.ReadProblemFromFile problemFile
        let solver = solvers[solverStr]
        let solution = solver problem
        let solutionFile = Path.Combine(solutionsDir, $"{num}.json")
        let solutionText = JsonDefs.WriteSolutionToJson solution
        File.WriteAllText(solutionFile, solutionText)

    | [| "score"; numStr |] ->
        let num = int numStr
        let problemFile = Path.Combine(problemsDir, $"{num}.json")
        let solutionFile = Path.Combine(solutionsDir, $"{num}.json")
        let problem = JsonDefs.ReadProblemFromFile problemFile
        let solution = JsonDefs.ReadSolutionFromFile solutionFile
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
