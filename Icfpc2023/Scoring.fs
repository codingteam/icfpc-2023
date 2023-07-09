module Icfpc2023.Scoring

type ClosenessFactor = double

[<Struct>]
type private Musician = {
    Instrument: int
    Location: PointD
    Volume: double
}

type SectoredIndex(problem: Problem) =
    let sectorSize = 50.0
    let sectors =
        let w = int <| ceil(problem.StageWidth / sectorSize)
        let h = int <| ceil(problem.StageHeight / sectorSize)
        Array2D.init w h (fun _ _ -> ResizeArray<PointD>())

    let getSectorSquares(stadium: Stadium) =
        let stadium' =
            { stadium with
                Center1 = stadium.Center1 - problem.StageBottomLeft
                Center2 = stadium.Center2 - problem.StageBottomLeft
            }

        let w = Array2D.length1 sectors
        let h = Array2D.length2 sectors
        seq {
            for x in 0..w - 1 do
                for y in 0..h - 1 do
                    let square = {
                        BottomLeft = PointD(float x * sectorSize, float y * sectorSize)
                        Width = sectorSize
                        Height = sectorSize
                    }
                    if stadium'.RectangularPartIntersectsWith square then
                        yield struct(x, y)
        }

    member _.Add(point: PointD): unit =
        let sector = sectors[int ((point.X - problem.StageBottomLeft.X) / sectorSize), int ((point.Y- problem.StageBottomLeft.Y) / sectorSize)]
        sector.Add point

    member _.GetPointsIn(s: Stadium): PointD seq =
        let squares = getSectorSquares s |> Seq.toArray

        squares
        |> Seq.collect(fun (struct(x, y)) -> sectors[x, y])
        |> Seq.filter(s.Contains)

let private CalculateAttendeeMusicianScore(attendee: Attendee, musician: Musician, closeness: ClosenessFactor): Score =
    let d_squared = (attendee.X - musician.Location.X) ** 2.0 + (attendee.Y - musician.Location.Y) ** 2.0
    musician.Volume * ceil(closeness * 1_000_000.0 * attendee.Tastes[musician.Instrument] / d_squared)

let private AnyOtherMusicianBlocksSound(index: SectoredIndex,
                                        attendee: Attendee,
                                        musician: Musician): bool =
    let musician = musician.Location
    let attendee = PointD(attendee.X, attendee.Y)
    let blockZone = { Center1 = musician; Center2 = attendee; Radius = 5.0 }
    let candidates = index.GetPointsIn blockZone |> Seq.toArray // TODO REMOVE TOARRAY

    let blocking =
        candidates
        |> Seq.filter(fun p -> p <> musician)
        |> Seq.toArray

    blocking.Length > 0

let private AnyPillarBlocksSound (pillars: Pillar[]) (musician: Musician) (attendee: Attendee): bool =
    let musician = PointD(musician.Location.X, musician.Location.Y)
    let attendee = PointD(attendee.X, attendee.Y)
    Array.toSeq pillars
    |> Seq.exists(fun (p) ->
            let blockZone = {
                Center1 = musician
                Center2 = attendee
                Radius = p.Radius
            }
            blockZone.Contains(p.Center))

let private CalculateAttendeeScore(pillars: Pillar[],
                                   musicians: Musician[],
                                   index: SectoredIndex,
                                   closenessFactors: ClosenessFactor[],
                                   attendee: Attendee): Score =
    Seq.indexed musicians
    |> Seq.sumBy(fun (i, musician) ->
        if AnyOtherMusicianBlocksSound(index, attendee, musicians[i]) then 0.0
        else if AnyPillarBlocksSound pillars musicians.[i] attendee then 0.0
        else CalculateAttendeeMusicianScore(attendee, musician, closenessFactors.[i])
    )

let private CalculateAttendeeNoBlockingScore (musicians: Musician[]) (attendee: Attendee): Score =
    musicians
    |> Array.sumBy(fun musician ->
        // FIXME: closeness factor is 1.0 here, i.e. we ignore closeness
        CalculateAttendeeMusicianScore(attendee, musician, 1.0)
    )

let private CalculateMusicianNoBlockingScore (attendees: Attendee[]) (musician: Musician): Score =
    attendees
    |> Array.sumBy(fun attendee ->
        // FIXME: closeness factor is 1.0 here, i.e. we ignore closeness
        CalculateAttendeeMusicianScore(attendee, musician, 1.0)
    )

let private CalculateClosenessFactors (musicians: Musician[]): ClosenessFactor[] =
    let coeffs: double[] = Array.zeroCreate (musicians.Length * musicians.Length)
    for i in 0 .. musicians.Length-1 do
        for j in 0 .. i-1 do
            if musicians.[i].Instrument = musicians.[j].Instrument
            then
                let distance = musicians.[i].Location.DistanceTo(musicians.[j].Location)
                let reciprocal = 1.0 / distance

                coeffs.[i*musicians.Length + j] <- reciprocal
                coeffs.[j*musicians.Length + i] <- reciprocal
    seq {
        for i in 0 .. musicians.Length-1 ->
            1.0 + (coeffs[i*musicians.Length .. (i+1)*musicians.Length-1] |> Seq.sum)
    } |> Seq.toArray

let CalculateScore(problem: Problem) (solution: Solution): Score =
    let BadScore = -1000000000.0
    let SafeDistanceToEdge = 10.0

    let area_for_musicians: Rectangle =
        {
            BottomLeft = problem.StageBottomLeft + PointD(SafeDistanceToEdge, SafeDistanceToEdge)
            Width = problem.StageWidth - SafeDistanceToEdge
            Height = problem.StageHeight - SafeDistanceToEdge
        }
    let musicians_are_safely_on_stage =
        solution.Placements |> Seq.forall (fun (p) -> area_for_musicians.Contains(p))
    if not musicians_are_safely_on_stage
    then BadScore
    else

    let musicians =
        Seq.zip3 problem.Musicians solution.Placements solution.Volumes
        |> Seq.map(fun(i, p, v) -> { Instrument = i; Location = p; Volume = v })
        |> Seq.toArray
    let closeness_factors =
        if problem.Pillars.Length > 0
        then CalculateClosenessFactors musicians
        else [| for i in 0 .. musicians.Length-1 -> 1.0 |]

    let tooClose =
        Seq.allPairs musicians musicians
        |> Seq.filter (fun (m1, m2) -> m1 <> m2)
        |> Seq.exists (fun (m1, m2) -> m1.Location.DistanceTo(m2.Location) < 10.0)

    if tooClose
    then BadScore
    else
        let index = SectoredIndex problem
        musicians |> Seq.iter(fun m -> index.Add m.Location)

        problem.Attendees
        |> Array.sumBy(fun a -> CalculateAttendeeScore(problem.Pillars, musicians, index, closeness_factors, a))

let CalculateNoBlockingScore(problem: Problem) (solution: IPartialSolution): Score =
    let musicians =
        solution.GetPlacedMusicians problem.Musicians
        |> Seq.map(fun(p, v, i) -> { Instrument = i; Location = p; Volume = v })
        |> Seq.toArray
    problem.Attendees |> Array.sumBy(CalculateAttendeeNoBlockingScore musicians)
