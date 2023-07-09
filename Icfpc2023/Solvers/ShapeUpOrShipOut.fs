module Icfpc2023.ShapeUpOrShipOut

open Icfpc2023.IterativeScoring

let Solve (initialSolutionOpt: Solution option) (problem: Problem): Solution =
    let initialSolution =
        match initialSolutionOpt with
        | Some solution ->
            let initialScore = Scoring.CalculateScore problem solution
            printfn $"💪: Initial score: {initialScore}"
            solution
        | None -> failwith "ShapeUpOrShipOut requires an initial solution"

    let volumes = Array.create problem.Musicians.Length 1.0
    let state = State.Create(problem, initialSolution.Placements, volumes)

    let MinVolume = 0.0
    let MaxVolume = 10.0
    for musicianId in 0 .. problem.Musicians.Length-1 do
        let subscore = state.MusicianAttendeeTotalImpact.MusicianSubscore musicianId
        let newVolume =
            if subscore < 0
            then MinVolume
            else MaxVolume
        volumes.[musicianId] <- newVolume

    let solution = { initialSolution with Volumes = volumes }
    let score = Scoring.CalculateScore problem solution
    printfn $"💪: Current score: {score}"

    solution
