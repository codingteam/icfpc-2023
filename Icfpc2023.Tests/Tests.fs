module Tests

open System.IO

open Icfpc2023
open Xunit

[<Fact>]
let ``Test problem reader`` () =
    let solDir = Program.solutionDirectory
    let problemDef = Path.Combine(solDir, "problems", "1.json")
    let p = JsonDefs.ReadProblemFromFile problemDef
    Assert.NotNull p
