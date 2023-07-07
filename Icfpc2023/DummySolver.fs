module Icfpc2023.DummySolver

let Solve(problem: Problem): Solution =
    let vacantRadius = 10.0
    let grid = seq {
        for x in vacantRadius .. vacantRadius .. problem.StageWidth-vacantRadius do
            for y in vacantRadius .. vacantRadius .. problem.StageHeight-vacantRadius ->
                problem.StageBottomLeft + PointD(x, y)
    }
    {
        Placements = Seq.take problem.Musicians.Length grid |> Seq.toArray
    }
