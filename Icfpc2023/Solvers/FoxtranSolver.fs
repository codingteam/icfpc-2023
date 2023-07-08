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
        match result.TryGetValue instrument with
        | false, _ -> result[instrument] <- [m]
        | true, existing ->
            result[instrument] <- m :: existing

    result

let private FindMaxValue grid =
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

let private GridCoordToPhysicalCoord (problem: Problem) (struct(x, y)) =
    let (PointD(originX, originY)) = problem.StageBottomLeft
    PointD(10.0 + originX + double x * GridCellSize, 10.0 + originY + double y * GridCellSize)

#nowarn "25"
let private PlaceMusicianAndDestroyPosition problem
                                            position
                                            instrument
                                            (placements: PointD array)
                                            (musicianIndicesPerInstrument: Dictionary<int, list<int>>)
                                            (gridsPerInstrument: Score[,][]) =
    let musician :: rest = musicianIndicesPerInstrument[instrument]

    // Set to target array:
    if placements[musician] <> PointD(0.0, 0.0) then failwith "Second time placement in same musician slot!"
    placements[musician] <- GridCoordToPhysicalCoord problem position

    // Clean up existing data on musician:
    match rest with
    | [] -> musicianIndicesPerInstrument.Remove instrument |> ignore
    | _ -> musicianIndicesPerInstrument[instrument] <- rest

    // Clean up existing data on grid cell:
    let struct(x, y) = position
    for grid in gridsPerInstrument do
        grid[x, y] <- Double.MinValue

let FoxtranSolveV1(problem: Problem): Solution =
    let instrumentCount = problem.Attendees[0].Tastes.Length
    let gridWidth, gridHeight =
        1 + (int <| floor ((problem.StageWidth - 20.0) / GridCellSize)),
        1 + (int <| floor ((problem.StageHeight - 20.0) / GridCellSize))
    let createGrid() = Array2D.zeroCreate gridWidth gridHeight

    let gridsPerInstrument = Array.init instrumentCount (fun _ -> createGrid())

    for cellX in 0 .. gridWidth - 1 do
      for cellY in 0 .. gridHeight - 1  do
        for instrument in 0 .. instrumentCount-1 do
          let cellPosition = GridCoordToPhysicalCoord problem (cellX, cellY)
          let solution = MakeSolutionWithSingleMusician instrument cellPosition
          let score = Scoring.CalculateNoBlockingScore problem solution
          gridsPerInstrument[instrument][cellX, cellY] <- score

    let musicianIndicesPerInstrument = GetMusicianIndicesPerInstrument problem
    let placements = Array.zeroCreate problem.Musicians.Length
    while musicianIndicesPerInstrument.Count > 0 do
        let instrument = ChooseBestInstrument musicianIndicesPerInstrument.Keys gridsPerInstrument
        let position = ChooseBestPosition gridsPerInstrument[instrument]
        PlaceMusicianAndDestroyPosition problem
                                        position
                                        instrument
                                        placements
                                        musicianIndicesPerInstrument
                                        gridsPerInstrument

    { Placements = placements }
