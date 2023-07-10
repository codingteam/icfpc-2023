module Icfpc2023.SwapSolver

let private Random = System.Random()
let private NumOfIterations = 1000
let private NumOfIterationsHorizontal = 10000

#nowarn "25"

let private genRandomPair (minValue: int) (maxValue: int) =
    let initial = Seq.initInfinite (fun _ -> Random.Next (minValue, maxValue))
    let [| a; b |] =
        initial
        |> Seq.distinct
        |> Seq.take(2)
        |> Seq.toArray

    struct (a, b)

let private swapMusicians (numOfMusicians: int) (placements: PointD array) =
    let newPlacements = Array.copy placements
    let struct (a, b) = genRandomPair 0 numOfMusicians
    let tmp = newPlacements[a]
    newPlacements[a] <- newPlacements[b]
    newPlacements[b] <- tmp
    newPlacements

let Solve (initialSolutionOpt: Solution option) (problem: Problem): Solution =
    let initialSolution =
        match initialSolutionOpt with
        | Some solution ->
            let initialScore = Scoring.CalculateScore problem solution
            printfn $"ξ: Initial score: {initialScore}"
            solution
        | None ->
            printfn $"ξ: Computing initial solution (foxtranV1)..."
            let initialSolution = FoxtranSolver.FoxtranSolveV1(problem)
            let initialScore = Scoring.CalculateScore problem initialSolution
            printfn $"ξ: Initial score: {initialScore}"
            initialSolution

    let mutable currentSolution = initialSolution
    let mutable currentScore = Scoring.CalculateScore problem currentSolution

    printfn "ξ: The Game begins..."
    for i = 0 to NumOfIterations - 1 do
        if i % 100 = 0 then
            printfn $"ξ: Iteration {i}, swap musicians..."

        let placements = currentSolution.Placements
        let newPlacements = swapMusicians problem.Musicians.Length placements
        let newSolution = { currentSolution with Placements = newPlacements }
        let newScore = Scoring.CalculateScore problem newSolution

        if newScore > currentScore then
            printfn $"ξ: Score {currentScore} -> {newScore}"
            currentSolution <- newSolution
            currentScore <- newScore

    currentSolution

let SolveHorizontal (initialSolutionOpt: Solution option) (problem: Problem): Solution =
    let initialSolution =
        match initialSolutionOpt with
        | Some solution ->
            let initialScore = Scoring.CalculateScore problem solution
            printfn $"ξ: Initial score: {initialScore}"
            solution
        | None ->
            printfn $"ξ: Computing initial solution (foxtranV1)..."
            let initialSolution = FoxtranSolver.FoxtranSolveV1(problem)
            let initialScore = Scoring.CalculateScore problem initialSolution
            printfn $"ξ: Initial score: {initialScore}"
            initialSolution

    let mutable currentSolution = initialSolution
    let mutable currentScore = Scoring.CalculateScore problem currentSolution

    printfn "ξ: The Game begins..."
    for i = 0 to NumOfIterations - 1 do
        if i % 100 = 0 then
            printfn $"ξ: Iteration {i}, swap musicians..."

        let placements = currentSolution.Placements
        let newPlacements = swapMusicians problem.Musicians.Length placements
        let newSolution = { currentSolution with Placements = newPlacements }
        let newScore = Scoring.CalculateScore problem newSolution

        if newScore > currentScore then
            printfn $"ξ: Score {currentScore} -> {newScore}"
            currentSolution <- newSolution
            currentScore <- newScore

            printfn $"ξ: Optimizing solution..."
            let optimizedSolution = DerFreeSolver.SolveHorizontal (Some currentSolution) (problem: Problem)
            let optimizedScore = Scoring.CalculateScore problem optimizedSolution
            if optimizedScore > currentScore then
                printfn $"ξ: Score {currentScore} -> {newScore}"
                currentSolution <- optimizedSolution
                currentScore <- optimizedScore

    currentSolution
