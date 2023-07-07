module Icfpc2023.DummySolver

let Solve(problem: Problem): Solution =
    let vacantRadius = 10.0
    let grid = seq {
        for x in 0.0 .. vacantRadius .. problem.StageWidth do
            for y in 0.0 .. vacantRadius .. problem.StageHeight ->
                problem.StageBottomLeft + PointD(x, y)
    }
    {
        Placements = Seq.take problem.Musicians.Length grid |> Seq.toArray
    }
