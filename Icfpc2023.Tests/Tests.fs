module Tests

open System.IO

open Icfpc2023
open Xunit

[<Fact>]
let ``Test problem reader`` () =
    let solDir = DirectoryLookup.solutionDirectory
    let problemDef = Path.Combine(solDir, "problems", "1.json")
    let p = JsonDefs.ReadProblemFromFile problemDef
    Assert.NotNull p

[<Fact>]
let ``FoxtranV1 solver test``(): unit =
    let problem = JsonDefs.ReadProblemFromFile(Path.Combine(DirectoryLookup.problemsDir, "22.json"))
    let solution = FoxtranSolver.FoxtranSolveV1 problem
    Assert.NotNull solution
