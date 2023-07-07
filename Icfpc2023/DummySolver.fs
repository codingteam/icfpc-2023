module Icfpc2023.DummySolver

let Solve(problem: Problem): Solution =
    {
        Placements = problem.Musicians |> Array.map(fun _ -> PointD(0.0, 0.0))
    }
