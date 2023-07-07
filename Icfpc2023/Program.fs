open System
open System.IO
open System.Threading.Tasks

open Icfpc2023

let solutionDirectory =
    let mutable directory = Environment.CurrentDirectory
    while not(isNull directory) && not <| File.Exists(Path.Combine(directory, "Icfpc2023.sln")) do
        directory <- Path.GetDirectoryName directory
    if isNull directory then
        failwith $"Cannot find root solution dir starting from path \"{Environment.CurrentDirectory}\"."
    directory

let runSynchronously(t: Task<'a>) =
    t.GetAwaiter().GetResult()

[<EntryPoint>]
let main(args: string[]): int =
    match args with
    | [|"download"; numStr|] ->
        let num = int numStr
        let problemsDir = Path.Combine(solutionDirectory, "problems")
        Directory.CreateDirectory problemsDir |> ignore
        for i in 1..num do
            let content = runSynchronously <| HttpApi.DownloadProblem i
            let filePath = Path.Combine(problemsDir, $"{string i}.json")
            File.WriteAllText(filePath, content)
    | _ -> printfn "Command unrecognized."

    0
