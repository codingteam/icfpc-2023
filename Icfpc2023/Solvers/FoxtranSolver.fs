module Icfpc2023.FoxtranSolver

let private MusicianDeadZoneRadius = 10.0
let private GridCellSize = MusicianDeadZoneRadius

type private PartialSolution(position: PointD, instrument: int) =
    interface IPartialSolution with
        member _.GetPlacedMusicians(musicians: int[]) =
            [| position, instrument |]

let private MakeSolutionWithSingleMusician index point =
    PartialSolution(point, index)

let FoxtranSolveV1(problem: Problem): Solution =
    let instrumentCount = problem.Attendees[0].Tastes.Length
    // TODO: Should we exclude the edge cells completely?
    let gridWidth, gridHeight =
        int <| ceil (float problem.StageWidth / GridCellSize),
        int <| ceil (float problem.StageHeight / GridCellSize)
    let createGrid() = Array2D.zeroCreate gridWidth gridHeight

    let gridsPerInstrument = Array.init instrumentCount (fun _ -> createGrid())

    for cellX in 0 .. gridWidth - 1 do
      for cellY in 0 .. gridHeight - 1  do
        for instrument in 0 .. instrumentCount-1 do
          // TODO: Place the musician in the middle of the cell?
          let cellPosition = PointD(x * GridCellSize, y * GridCellSize)
          let solution = MakeSolutionWithSingleMusician instrument cellPosition
          let score = Scoring.CalculateNoBlockingScore problem solution
          gridsPerInstrument[instrument][cellX, cellY] <- score

    // for each musician type
    //   find maximum over all musicians
    //     take this point
    //     remove points around selected point (set large negative value to these grid points over all musiians)
    {
        Placements = ...
    }
