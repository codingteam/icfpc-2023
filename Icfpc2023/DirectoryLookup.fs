module Icfpc2023.DirectoryLookup

open System
open System.IO

let solutionDirectory =
    let mutable directory = Environment.CurrentDirectory
    while not(isNull directory) && not <| File.Exists(Path.Combine(directory, "Icfpc2023.sln")) do
        directory <- Path.GetDirectoryName directory
    if isNull directory then
        failwith $"Cannot find root solution dir starting from path \"{Environment.CurrentDirectory}\"."
    directory

let problemsDir = Path.Combine(solutionDirectory, "problems")
let solutionsDir = Path.Combine(solutionDirectory, "solutions")
let examplesDir = Path.Combine(solutionDirectory, "examples")
