module Icfpc2023.LambdaSolver

open Accord.Math.Optimization

let pointsToArray (points: PointD[]) =
    points
    |> Seq.map (fun p -> [| p.X; p.Y |])
    |> Seq.collect id
    |> Seq.toArray

let arrayToPoints (array: double[]) =
    array
    |> Seq.chunkBySize 2
    |> Seq.map (fun [| x; y |] -> PointD(x, y))
    |> Seq.toArray

let Solve(problem: Problem): Solution =
    let initialSolution = DummySolver.RandomDummyV2(problem)
    let initialGuess = initialSolution.Placements |> pointsToArray

    let f = fun point -> LambdaScoring.lambda_score(arrayToPoints point, problem, 0.5)
    let df = fun point -> LambdaScoring.lambda_deriv(arrayToPoints point, problem, 0.5) |> pointsToArray

    let method = BroydenFletcherGoldfarbShanno(initialGuess.Length, f, df)
    method.Maximize(initialGuess) |> ignore
    let solution = method.Solution |> arrayToPoints

    { Placements = solution }
