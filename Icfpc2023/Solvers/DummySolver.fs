module Icfpc2023.DummySolver

let private random = System.Random()

let private shuffle<'T>(a: 'T[]) =
    for i in 0 .. a.Length - 1 do
        let j = random.Next(i, a.Length)
        let ai = a.[i]
        a.[i] <- a.[j]
        a.[j] <- ai

let private rectangularGrid(width: double, height: double, step: double, offset: PointD): seq<PointD> =
    seq {
        for x in step .. step .. width-step do
            for y in step .. step .. height-step ->
                offset + PointD(x, y)
    }

let private hexagonalGrid(width: double, height: double, step: double, offset: PointD): seq<PointD> =
    let xStep = step
    let yStep = step * sqrt 3.0
    let oddRows = seq {
        for y in step .. 2.0*yStep .. height-step do
            for x in step .. xStep .. width-step ->
                offset + PointD(x, y)
    }
    let evenRows = seq {
        for y in step+yStep .. 2.0*yStep .. height-step do
            for x in step*1.5 .. xStep .. width-step ->
                offset + PointD(x, y)
    }
    Seq.concat [oddRows; evenRows]

let SolveV1(problem: Problem): Solution =
    let vacantRadius = 10.0
    let grid = rectangularGrid(problem.StageWidth, problem.StageHeight, vacantRadius, problem.StageBottomLeft)
    {
        Placements = Seq.take problem.Musicians.Length grid |> Seq.toArray
        Volumes = Solution.defaultVolumes problem.Musicians.Length
    }

// Puts musicians on a hexagonal grid, achieving the optimal packing:
// https://en.wikipedia.org/wiki/Circle_packing
let SolveV2(problem: Problem): Solution =
    let vacantRadius = 10.0
    let grid = hexagonalGrid(problem.StageWidth, problem.StageHeight, vacantRadius, problem.StageBottomLeft)
    {
        Placements = Seq.take problem.Musicians.Length grid |> Seq.toArray
        Volumes = Solution.defaultVolumes problem.Musicians.Length
    }

/// Like SolveV1, but fills the rectangular grid randomly rather than
/// progressively.
let RandomDummyV1(problem: Problem): Solution =
    let vacantRadius = 10.0
    let grid =
        rectangularGrid(problem.StageWidth, problem.StageHeight, vacantRadius, problem.StageBottomLeft)
        |> Seq.toArray
    shuffle grid
    let placements =
        Array.toSeq grid
        |> Seq.take problem.Musicians.Length
        |> Seq.toArray
    {
        Placements = placements
        Volumes = Solution.defaultVolumes problem.Musicians.Length
    }

/// Like SolveV2, but fills the hexagonal grid randomly rather than
/// progressively.
let RandomDummyV2(problem: Problem): Solution =
    let vacantRadius = 10.0
    let grid =
        hexagonalGrid(problem.StageWidth, problem.StageHeight, vacantRadius, problem.StageBottomLeft)
        |> Seq.toArray
    shuffle grid
    let placements =
        Array.toSeq grid
        |> Seq.take problem.Musicians.Length
        |> Seq.toArray
    {
        Placements = placements
        Volumes = Solution.defaultVolumes problem.Musicians.Length
    }
