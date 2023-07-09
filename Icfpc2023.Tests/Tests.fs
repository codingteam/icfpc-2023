module Tests

open System.IO

open Icfpc2023
open Icfpc2023.Scoring
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

[<Fact>]
let ``Can check if rectangle contains a point``() =
    let rectangle =
        {
            BottomLeft = PointD(0.0, 0.0)
            Width = 100.0
            Height = 500.0
        }

    Assert.False(rectangle.Contains(PointD(100_000.0, 200_000.0)))
    Assert.False(rectangle.Contains(PointD(-1.0, -5.0)))

    // boundary belongs to the rectangle
    Assert.True(rectangle.Contains(PointD(0.0, 0.0)))
    Assert.True(rectangle.Contains(PointD(1.0, 2.0)))

    Assert.False(rectangle.Contains(PointD(5.0, 501.0)))
    Assert.False(rectangle.Contains(PointD(101.0, 30.0)))

[<Fact>]
let ``Scoring yields the expected score for the sample problem from the spec`` () =
    let problem =
        {
            RoomWidth = 2_000.0
            RoomHeight = 5_000.0
            StageWidth = 1_000.0
            StageHeight = 200.0
            StageBottomLeft = PointD(500.0, 0.0)
            Musicians = [| 0; 1; 0 |]
            Attendees =
                [|
                    {
                        X = 100.0
                        Y = 500.0
                        Tastes = [| 1_000.0; -1_000.0 |]
                    }
                    {
                        X = 200.0
                        Y = 1_000.0
                        Tastes = [| 200.0; 200.0 |]
                    }
                    {
                        X = 1_100.0
                        Y = 800.0
                        Tastes = [| 800.0; 1_500.0 |]
                    }
                |]
            Pillars = [||]
        }
    let solution =
        {
            Placements =
                [|
                    PointD(590.0, 10.0)
                    PointD(1_100.0, 100.0)
                    PointD(1_100.0, 150.0)
                |]
            Volumes = [||]
        }

    Assert.Equal(5343.0, CalculateScore problem solution)

[<Fact>]
let ``Stadium detects intersection when blocking musician is right on the line`` () =
    let stadium =
        {
            Center1 = PointD(1100.0, 100.0)
            Center2 = PointD(1100.0, 800.0)
            Radius = 5.0
        }
    let blockingMusician = PointD(1100.0, 150.0)
    Assert.True(stadium.Contains blockingMusician)
