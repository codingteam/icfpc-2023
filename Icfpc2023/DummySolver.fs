module Icfpc2023.DummySolver

let Solve(problem: Problem): Solution =
    let vacantRadius = 10.0
    let xStep = vacantRadius
    let yStep = vacantRadius * sqrt 3.0
    let oddRows = seq {
        for y in vacantRadius .. 2.0*yStep .. problem.StageHeight-vacantRadius do
            for x in vacantRadius .. xStep .. problem.StageWidth-vacantRadius ->
                problem.StageBottomLeft + PointD(x, y)
    }
    let evenRows = seq {
        for y in vacantRadius+yStep .. 2.0*yStep .. problem.StageHeight-vacantRadius do
            for x in vacantRadius*1.5 .. xStep .. problem.StageWidth-vacantRadius ->
                problem.StageBottomLeft + PointD(x, y)
    }
    let grid = Seq.concat [oddRows; evenRows]
    {
        Placements = Seq.take problem.Musicians.Length grid |> Seq.toArray
    }
