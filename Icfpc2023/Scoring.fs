module Icfpc2023.Scoring

type ClosenessFactor = double

[<Struct>]
type private Musician = {
    Instrument: int
    Location: PointD
    Volume: double
}

let private CalculateAttendeeMusicianScore(attendee: Attendee, musician: Musician, closeness: ClosenessFactor): Score =
    let d_squared = (attendee.X - musician.Location.X) ** 2.0 + (attendee.Y - musician.Location.Y) ** 2.0
    musician.Volume * ceil(closeness * 1_000_000.0 * attendee.Tastes[musician.Instrument] / d_squared)

let private AnyOtherMusicianBlocksSound (musicians: Musician[]) (attendee: Attendee) (mIndex: int): bool =
    let musician = PointD(musicians[mIndex].Location.X, musicians[mIndex].Location.Y)
    let attendee = PointD(attendee.X, attendee.Y)
    let blockZone = { Center1 = musician; Center2 = attendee; Radius = 5.0 }

    Seq.indexed musicians
    |> Seq.filter(fun (i, _) -> i <> mIndex)
    |> Seq.exists(fun (_, m) -> blockZone.Contains(m.Location))

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

let private CalculateAttendeeScore (pillars: Pillar[]) (musicians: Musician[]) (closenessFactors: ClosenessFactor[]) (attendee: Attendee): Score =
    Seq.indexed musicians
    |> Seq.sumBy(fun (i, musician) ->
        if AnyOtherMusicianBlocksSound musicians attendee i then 0.0
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
        problem.Attendees |> Array.sumBy(CalculateAttendeeScore problem.Pillars musicians closeness_factors)

let CalculateNoBlockingScore(problem: Problem) (solution: IPartialSolution): Score =
    let musicians =
        solution.GetPlacedMusicians problem.Musicians
        |> Seq.map(fun(p, v, i) -> { Instrument = i; Location = p; Volume = v })
        |> Seq.toArray
    problem.Attendees |> Array.sumBy(CalculateAttendeeNoBlockingScore musicians)
