module Tests

open System.IO

open Icfpc2023
open Icfpc2023.IterativeScoring
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
let ``IterativeScoring state is invalid if some musician is too close to the edge of the stage``() =
    let problem =
        {
            RoomWidth = 100.0
            RoomHeight = 100.0
            StageWidth = 10.0
            StageHeight = 10.0
            StageBottomLeft = PointD(0.0, 0.0)
            Musicians = [| 0 |]
            Attendees =
                [|
                    {
                        X = 50.0
                        Y = 50.0
                        Tastes = [| 100.0 |]
                    }
                |]
            Pillars = [||]
        }

    let state = State.Create(problem, [| PointD(1.0, 1.0) |])
    Assert.False(state.IsValid)

[<Fact>]
let ``IterativeScoring state is valid if all musicians are far enough from the edge of the stage``() =
    let problem =
        {
            RoomWidth = 100.0
            RoomHeight = 100.0
            StageWidth = 30.0
            StageHeight = 50.0
            StageBottomLeft = PointD(0.0, 0.0)
            Musicians = [| 0; 1; 2 |]
            Attendees =
                [|
                    {
                        X = 90.0
                        Y = 90.0
                        Tastes = [| 100.0; 0.0; -100.0 |]
                    }
                |]
            Pillars = [||]
        }

    let state = State.Create(problem, [| PointD(10.0, 10.0); PointD(20.0, 10.0); PointD(20.0, 40.0) |])
    Assert.True(state.IsValid)

[<Fact>]
let ``IterativeScoring state is invalid if two musicians are closer than 10 to each other``() =
    let problem =
        {
            RoomWidth = 1_000.0
            RoomHeight = 100.0
            StageWidth = 50.0
            StageHeight = 50.0
            StageBottomLeft = PointD(0.0, 0.0)
            Musicians = [| 0; 1 |]
            Attendees =
                [|
                    {
                        X = 90.0
                        Y = 90.0
                        Tastes = [| 100.0; -1_000.0 |]
                    }
                |]
            Pillars = [||]
        }

    let state = State.Create(problem, [| PointD(10.0, 10.0); PointD(11.0, 10.0) |])
    Assert.False(state.IsValid)

[<Fact>]
let ``IterativeScoring state is valid if all musicians are 10 or farther from each other``() =
    let problem =
        {
            RoomWidth = 1_000.0
            RoomHeight = 100.0
            StageWidth = 60.0
            StageHeight = 80.0
            StageBottomLeft = PointD(0.0, 0.0)
            Musicians = [| 0; 0; 0 |]
            Attendees =
                [|
                    {
                        X = 90.0
                        Y = 90.0
                        Tastes = [| 100.0 |]
                    }
                |]
            Pillars = [||]
        }

    let state = State.Create(problem, [| PointD(10.0, 10.0); PointD(20.0, 10.0); PointD(50.0, 70.0) |])
    Assert.True(state.IsValid)

[<Fact>]
let ``IteraticeScoring State.Create throws exception when the number of placements is less than the number of musicians``() =
    let problem =
        {
            RoomWidth = 1_000.0
            RoomHeight = 100.0
            StageWidth = 60.0
            StageHeight = 80.0
            StageBottomLeft = PointD(0.0, 0.0)
            Musicians = [| 0; 0; 1 |]
            Attendees =
                [|
                    {
                        X = 90.0
                        Y = 90.0
                        Tastes = [| 100.0; 200.0 |]
                    }
                |]
            Pillars = [||]
        }

    let too_few_placements = [| PointD(0.0, 0.0) |]
    Assert.Throws<System.Exception>(fun () -> State.Create(problem, too_few_placements) :> obj)
    |> ignore

[<Fact>]
let ``IteraticeScoring State.Create throws exception when the number of placements is more than the number of musicians``() =
    let problem =
        {
            RoomWidth = 1_000.0
            RoomHeight = 100.0
            StageWidth = 60.0
            StageHeight = 80.0
            StageBottomLeft = PointD(0.0, 0.0)
            Musicians = [| 0; 0; 0 |]
            Attendees =
                [|
                    {
                        X = 90.0
                        Y = 90.0
                        Tastes = [| 900.0 |]
                    }
                |]
            Pillars = [||]
        }

    let too_many_placements = [| PointD(0.0, 0.0); PointD(10.0, 10.0); PointD(20.0, 20.0); PointD(30.0, 30.0) |]
    Assert.Throws<System.Exception>(fun () -> State.Create(problem, too_many_placements) :> obj)
    |> ignore

[<Fact>]
let ``IterativeScoring yields the expected score for the sample problem from the spec`` () =
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
    let placements =
        [|
            PointD(590.0, 10.0)
            PointD(1_100.0, 100.0)
            PointD(1_100.0, 150.0)
        |]

    let state = State.Create(problem, placements)
    Assert.Equal(5343.0, state.Score)

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
            Volumes = [| 1.0; 1.0; 1.0 |]
        }

    Assert.Equal(5343.0, Scoring.CalculateScore problem solution)

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

[<Fact>]
let ``CalculateScore for problem 1``(): unit =
    let problem = JsonDefs.ReadProblemFromFile(Path.Combine(DirectoryLookup.problemsDir, "1.json"))
    let solution = JsonDefs.ReadSolutionFromFile(Path.Combine(DirectoryLookup.solutionsDir, "1.json"))
    let score = Scoring.CalculateScore problem solution
    let meta = JsonDefs.ReadSolutionMetadataFromFile(Path.Combine(DirectoryLookup.solutionsDir, "1.meta.json"))
    Assert.Equal(meta.Score, score)

[<Fact>]
let ``CalculateScore for problem 30``(): unit =
    let problem = JsonDefs.ReadProblemFromFile(Path.Combine(DirectoryLookup.problemsDir, "30.json"))
    let solution = JsonDefs.ReadSolutionFromFile(Path.Combine(DirectoryLookup.solutionsDir, "30.json"))
    let score = Scoring.CalculateScore problem solution
    let meta = JsonDefs.ReadSolutionMetadataFromFile(Path.Combine(DirectoryLookup.solutionsDir, "30.meta.json"))
    Assert.Equal(meta.Score, score)
