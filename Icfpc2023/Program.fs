open System
open System.IO
open System.Text
open System.Threading.Tasks

open Icfpc2023.HttpApi

let solutionDirectory =
    let mutable directory = Environment.CurrentDirectory
    while not(isNull directory) && not <| File.Exists(Path.Combine(directory, "Icfpc2023.sln")) do
        directory <- Path.GetDirectoryName directory
    if isNull directory then
        failwith $"Cannot find root solution dir starting from path \"{Environment.CurrentDirectory}\"."
    directory

let runSynchronously(t: Task<'a>) =
    t.GetAwaiter().GetResult()

let runSynchronouslyV(t: Task) =
    t.GetAwaiter().GetResult()

let readToken() =
    let tokenFile = Path.Combine(solutionDirectory, "token.txt")
    if not <| File.Exists tokenFile then
        failwith $"Please save the access token to file \"{tokenFile}\"."
    File.ReadAllText(tokenFile).Trim()

[<EntryPoint>]
let main(args: string[]): int =
    Console.OutputEncoding <- Encoding.UTF8

    match args with
    | [|"download"; numStr|] ->
        let num = int numStr
        let problemsDir = Path.Combine(solutionDirectory, "problems")
        Directory.CreateDirectory problemsDir |> ignore
        for i in 1..num do
            let content = runSynchronously <| DownloadProblem i
            let filePath = Path.Combine(problemsDir, $"{string i}.json")
            printfn $"Downloading problem {i} to \"{filePath}\"…"
            File.WriteAllText(filePath, content)
    | [|"upload"; "all"|] ->
        let token = readToken()
        let solutionsDir = Path.Combine(solutionDirectory, "solutions")
        let solutions = Directory.GetFiles(solutionsDir, "*.json")
        for solution in solutions do
            printfn $"Uploading solution \"{Path.GetFileName solution}\"…"
            let problemNumber = Path.GetFileNameWithoutExtension solution |> int
            let submission = { ProblemId = problemNumber; Contents = File.ReadAllText(solution) }
            runSynchronouslyV <| Upload(submission, token)
    | _ -> printfn "Command unrecognized."

    0
