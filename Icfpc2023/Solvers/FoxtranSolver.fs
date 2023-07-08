module Icfpc2023.FoxtranSolver

open System
open System.Collections.Generic

let private MusicianDeadZoneRadius = 10.0
let private GridCellSize = MusicianDeadZoneRadius

type private PartialSolution(position: PointD, instrument: int) =
    interface IPartialSolution with
        member _.GetPlacedMusicians _ =
            [| position, instrument |]

let private MakeSolutionWithSingleMusician index point =
    PartialSolution(point, index)

let private GetMusicianIndicesPerInstrument problem: Dictionary<int, list<int>> =
    let result = Dictionary<int, list<int>>()
    for m in 0 .. problem.Musicians.Length - 1 do
        let instrument = problem.Musicians[m]
        match result.TryGetValue(m) with
        | false, _ -> result[instrument] <- [m]
        | true, existing ->
            result[instrument] <- m :: existing

    result

let FindMaxValue grid =
    let mutable max = Double.MinValue
    let mutable maxPos = struct(-1, -1)
    for x in 0 .. Array2D.length1 grid - 1 do
        for y in 0 .. Array2D.length2 grid - 1 do
            if grid[x, y] > max then
                 max <- grid[x, y]
                 maxPos <- x, y
    struct(maxPos, max)

let private ChooseBestInstrument availableInstruments (gridsPerInstrument: Score[,][]) =
    availableInstruments
    |> Seq.maxBy(fun i ->
        let struct(_, v) = FindMaxValue gridsPerInstrument[i]
        v
    )

let private ChooseBestPosition grid =
    let struct(pos, _) = FindMaxValue grid
    pos

let private GridCoordToPhysicalCoord(struct(x, y)) =
    PointD(double x * GridCellSize, double y * GridCellSize)

#nowarn "25"
let private PlaceMusicianAndDestroyPosition position
                                            instrument
                                            (placements: PointD array)
                                            (musicianIndicesPerInstrument: Dictionary<int, list<int>>)
                                            (gridsPerInstrument: Score[,][]) =
    let musician :: rest = musicianIndicesPerInstrument[instrument]

    // Set to target array:
    placements[musician] <- GridCoordToPhysicalCoord position

    // Clean up existing data on musician:
    match rest with
    | [] -> musicianIndicesPerInstrument.Remove instrument |> ignore
    | _ -> musicianIndicesPerInstrument[instrument] <- rest

    // Clean up existing data on grid cell:
    let struct(x, y) = position
    gridsPerInstrument[instrument][x, y] <- Double.MinValue

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
          let cellPosition = GridCoordToPhysicalCoord(cellX, cellY)
          let solution = MakeSolutionWithSingleMusician instrument cellPosition
          let score = Scoring.CalculateNoBlockingScore problem solution
          gridsPerInstrument[instrument][cellX, cellY] <- score

    let musicianIndicesPerInstrument = GetMusicianIndicesPerInstrument problem
    let placements = Array.zeroCreate problem.Musicians.Length
    while musicianIndicesPerInstrument.Count > 0 do
        let instrument = ChooseBestInstrument musicianIndicesPerInstrument.Keys gridsPerInstrument
        let position = ChooseBestPosition gridsPerInstrument[instrument]
        PlaceMusicianAndDestroyPosition position instrument placements musicianIndicesPerInstrument gridsPerInstrument

    { Placements = placements }
