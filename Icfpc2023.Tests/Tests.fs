module Tests

open System.IO

open Icfpc2023
open Icfpc2023.IterativeScoring
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
            Musicians = [| 1 |]
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
            Musicians = [| 1; 2; 3 |]
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
            Musicians = [| 1; 2 |]
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
            Musicians = [| 1; 1; 1 |]
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
            Musicians = [| 1; 1; 1 |]
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
            Musicians = [| 1; 1; 1 |]
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

    let too_many_placements = [| PointD(0.0, 0.0); PointD(10.0, 10.0); PointD(20.0, 20.0); PointD(30.0, 30.0) |]
    Assert.Throws<System.Exception>(fun () -> State.Create(problem, too_many_placements) :> obj)
    |> ignore
