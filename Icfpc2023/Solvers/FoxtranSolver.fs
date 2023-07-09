module Icfpc2023.FoxtranSolver

open System
open System.Collections.Generic
open System.Linq
open System.Threading

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

let private FindMaxValue(grid, shadowMatrix: double[,] option) =
    let mutable max = Double.MinValue
    let mutable maxPos = struct(-1, -1)
    for x in 0 .. Array2D.length1 grid - 1 do
        for y in 0 .. Array2D.length2 grid - 1 do
            let value = grid[x, y]
            let value =
                match shadowMatrix with
                | None -> value
                | Some sm -> value * sm[x, y]

            if value > max then
                 max <- value
                 maxPos <- x, y
    struct(maxPos, max)

let private ChooseBestInstrument availableInstruments (gridsPerInstrument: Score[,][]) shadowMatrix =
    availableInstruments
    |> Seq.maxBy(fun i ->
        let struct(_, v) = FindMaxValue(gridsPerInstrument[i], shadowMatrix)
        v
    )

let private ChooseBestPosition(grid, shadowMatrix) =
    let struct(pos, _) = FindMaxValue(grid, shadowMatrix)
    pos

let private GridCoordToPhysicalCoord (problem: Problem) (struct(x, y)) =
    let (PointD(originX, originY)) = problem.StageBottomLeft
    PointD(10.0 + originX + double x * GridCellSize, 10.0 + originY + double y * GridCellSize)

let rec private ApplyShadow (shadowMatrix: double[,], struct(x, y), fuel: int): unit =
    if fuel > 0 then
        let w = Array2D.length1 shadowMatrix
        let h = Array2D.length2 shadowMatrix

        let shadowedCells =
            seq {
                if x < w / 2 then
                    struct(x + 1, y)
                else if x > w / 2 then
                    struct(x - 1, y)
                if y < h / 2 then
                    struct(x, y + 1)
                else if y > h / h then
                    struct(x, y - 1)
                if x < w / 2 && y < h / 2 then
                    struct(x + 1, y + 1)
                else if x > w / 2 && y > h / 2 then
                    struct(x - 1, y - 1)
                if x < w / 2 && y > h / 2 then
                    struct(x + 1, y - 1)
                else if x > w / 2 && y < h / 2 then
                    struct(x - 1, y + 1)
            } |> Seq.filter(fun struct(x, y) -> x >= 0 && x < w && y >= 0 && y < h)

        for sx, sy in shadowedCells do
            shadowMatrix[sx, sy] <- shadowMatrix[sx, sy] * 0.1
            ApplyShadow(shadowMatrix, struct(sx, sy), fuel - 1)

#nowarn "25"
let private PlaceMusicianAndDestroyPosition problem
                                            position
                                            instrument
                                            (placements: PointD array)
                                            (volumes: double array)
                                            (shadowMatrix: double[,] option)
                                            (musicianIndicesPerInstrument: Dictionary<int, list<int>>)
                                            (gridsPerInstrument: Score[,][]) =
    let musician :: rest = musicianIndicesPerInstrument[instrument]

    // Set to target array:
    if placements[musician] <> PointD(0.0, 0.0) then failwith "Second time placement in same musician slot!"
    placements[musician] <- GridCoordToPhysicalCoord problem position

    // Calculate volume:
    if volumes[musician] <> 0.0 then failwith "Second time placement in same musician volume slot!"
    let currentValue =
        let struct(x, y) = position
        gridsPerInstrument[instrument][x, y]
    volumes[musician] <- if currentValue >= 0 then 10.0 else 0.0

    // Clean up existing data on musician:
    match rest with
    | [] -> musicianIndicesPerInstrument.Remove instrument |> ignore
    | _ -> musicianIndicesPerInstrument[instrument] <- rest

    // Clean up existing data on grid cell:
    let struct(x, y) = position
    for grid in gridsPerInstrument do
        grid[x, y] <- Double.MinValue
// FIXME: remove grid point for grids where shift lower MusicianDeadZoneRadius
//        for cellX in 0 .. grid.length1 - 1 do
//            for cellY in 0 .. grid.length2 - 1 do
//                let cellPosition = GridCoordToPhysicalCoord problem (cellX, cellY)
//                if cellPosition.SquaredDistanceTo(position) < MusicianDeadZoneRadius * MusicianDeadZoneRadius then
//                    grid[cellX, cellY] <- Double.MinValue


    match shadowMatrix with
    | None -> ()
    | Some shadowMatrix ->
        let fuel = 1
        ApplyShadow(shadowMatrix, position, fuel)

let FoxtranSolveV1(problem: Problem): Solution =
    let instrumentCount = problem.Attendees[0].Tastes.Length
    let gridWidth, gridHeight =
        1 + (int <| floor ((problem.StageWidth - 20.0) / GridCellSize)),
        1 + (int <| floor ((problem.StageHeight - 20.0) / GridCellSize))
    let createGrid() = Array2D.zeroCreate gridWidth gridHeight

    let mutable finishedInstruments = 0
    let gridsPerInstrument =
        ParallelEnumerable.Range(0, instrumentCount)
            .Select(fun instrument ->
                let grid = createGrid()
                for cellX in 0 .. gridWidth - 1 do
                    for cellY in 0 .. gridHeight - 1  do
                        let cellPosition = GridCoordToPhysicalCoord problem (cellX, cellY)
                        let solution = MakeSolutionWithSingleMusician instrument cellPosition
                        let score = Scoring.CalculateNoBlockingScore problem solution
                        grid[cellX, cellY] <- score
                let finished = Interlocked.Increment(&finishedInstruments)
                if finished % 25 = 0 then
                    printfn $"  Prepared {finished}/{instrumentCount} instruments…"
                grid
            )
            .ToArray()

    printfn $"Prepared {finishedInstruments}/{instrumentCount} instruments."

    let musicianIndicesPerInstrument = GetMusicianIndicesPerInstrument problem
    let placements = Array.zeroCreate problem.Musicians.Length
    let volumes = Array.zeroCreate problem.Musicians.Length
    while musicianIndicesPerInstrument.Count > 0 do
        let instrument = ChooseBestInstrument musicianIndicesPerInstrument.Keys gridsPerInstrument None
        let position = ChooseBestPosition(gridsPerInstrument[instrument], None)
        PlaceMusicianAndDestroyPosition problem
                                        position
                                        instrument
                                        placements
                                        volumes
                                        None
                                        musicianIndicesPerInstrument
                                        gridsPerInstrument

    {
        Placements = placements
        Volumes = volumes
    }

let FoxtranSolveV2(problem: Problem): Solution =
    let instrumentCount = problem.Attendees[0].Tastes.Length
    let gridWidth, gridHeight =
        1 + (int <| floor ((problem.StageWidth - 20.0) / GridCellSize)),
        1 + (int <| floor ((problem.StageHeight - 20.0) / GridCellSize))
    let createGrid() = Array2D.zeroCreate gridWidth gridHeight

    let mutable finishedInstruments = 0
    let gridsPerInstrument =
        ParallelEnumerable.Range(0, instrumentCount)
            .Select(fun instrument ->
                let grid = createGrid()
                for cellX in 0 .. gridWidth - 1 do
                    for cellY in 0 .. gridHeight - 1  do
                        let cellPosition = GridCoordToPhysicalCoord problem (cellX, cellY)
                        let solution = MakeSolutionWithSingleMusician instrument cellPosition
                        let score = Scoring.CalculateNoBlockingScore problem solution
                        grid[cellX, cellY] <- score
                let finished = Interlocked.Increment(&finishedInstruments)
                if finished % 25 = 0 then
                    printfn $"  Prepared {finished}/{instrumentCount} instruments…"
                grid
            )
            .ToArray()

    printfn $"Prepared {finishedInstruments}/{instrumentCount} instruments."
    let shadowMatrix = Array2D.create gridWidth gridHeight 1.0

    let musicianIndicesPerInstrument = GetMusicianIndicesPerInstrument problem
    let placements = Array.zeroCreate problem.Musicians.Length
    let volumes = Array.zeroCreate problem.Musicians.Length
    while musicianIndicesPerInstrument.Count > 0 do
        let instrument = ChooseBestInstrument musicianIndicesPerInstrument.Keys gridsPerInstrument (Some shadowMatrix)
        let position = ChooseBestPosition(gridsPerInstrument[instrument], Some shadowMatrix)
        PlaceMusicianAndDestroyPosition problem
                                        position
                                        instrument
                                        placements
                                        volumes
                                        (Some shadowMatrix)
                                        musicianIndicesPerInstrument
                                        gridsPerInstrument

    {
        Placements = placements
        Volumes = volumes
    }
