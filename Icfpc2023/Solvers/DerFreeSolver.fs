module Icfpc2023.DerFreeSolver

open Accord.Math.Optimization

#nowarn "25"
#nowarn "49"

let private pointsToArray (points: PointD[]) =
    points
    |> Seq.map (fun p -> [| p.X; p.Y |])
    |> Seq.collect id
    |> Seq.toArray

let private arrayToPoints (array: double[]) =
    array
    |> Seq.chunkBySize 2
    |> Seq.map (fun [| x; y |] -> PointD(x, y))
    |> Seq.toArray

let private MusicianDeadZoneRadius = 10.0

let Solve(problem: Problem): Solution =
    printfn $"∇: Computing initial solution..."
    let initialSolution = FoxtranSolver.FoxtranSolveV1(problem)
    let initialScore = Scoring.CalculateScore problem initialSolution
    printfn $"∇: Initial score: {initialScore}"

    let initialGuess = initialSolution.Placements |> pointsToArray

    let objective = fun point ->
        Scoring.CalculateScore problem { Placements = point |> arrayToPoints }

    let method =
        NelderMead(
            numberOfVariables = initialGuess.Length,
            ``function`` = objective)

    let lowerBounds = method.LowerBounds
    for i = 0 to initialSolution.Placements.Length - 1 do
        lowerBounds[i*2] <- problem.StageBottomLeft.X + MusicianDeadZoneRadius
        lowerBounds[i*2 + 1] <- problem.StageBottomLeft.Y + MusicianDeadZoneRadius

    let upperBounds = method.UpperBounds
    for i = 0 to initialSolution.Placements.Length - 1 do
        upperBounds[i*2] <- problem.StageBottomLeft.X + problem.StageWidth - MusicianDeadZoneRadius
        upperBounds[i*2 + 1] <- problem.StageBottomLeft.Y + problem.StageHeight - MusicianDeadZoneRadius

    let success = method.Maximize(initialGuess)
    printfn $"∇: Converged? {success}, status {method.Status}"

    let solution = method.Solution
    let score = Scoring.CalculateScore problem { Placements = solution |> arrayToPoints }
    printfn $"∇: Current score: {score}"

    { Placements = solution |> arrayToPoints }
