module Icfpc2023.Scoring

[<Struct>]
type private Musician = {
    Instrument: int
    Location: PointD
}

let private CalculateAttendeeMusicianScore(attendee: Attendee, musician: Musician): Score =
    let d_squared = (attendee.X - musician.Location.X) ** 2.0 + (attendee.Y - musician.Location.Y) ** 2.0
    ceil(1_000_000.0 * attendee.Tastes[musician.Instrument] / d_squared)

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
    let line = { End1 = musician; End2 = attendee }
    Array.toSeq pillars
    |> Seq.exists(fun (p) -> line.DistanceTo(p.Center) < p.Radius)

let private CalculateAttendeeScore (pillars: Pillar[]) (musicians: Musician[]) (attendee: Attendee): Score =
    Seq.indexed musicians
    |> Seq.sumBy(fun (i, musician) ->
        if AnyOtherMusicianBlocksSound musicians attendee i then 0.0
        else if AnyPillarBlocksSound pillars musicians.[i] attendee then 0.0
        else CalculateAttendeeMusicianScore(attendee, musician)
    )

let private CalculateAttendeeNoBlockingScore (musicians: Musician[]) (attendee: Attendee): Score =
    musicians
    |> Array.sumBy(fun musician ->
        CalculateAttendeeMusicianScore(attendee, musician)
    )

let private CalculateMusicianNoBlockingScore (attendees: Attendee[]) (musician: Musician): Score =
    attendees
    |> Array.sumBy(fun attendee ->
        CalculateAttendeeMusicianScore(attendee, musician)
    )

let CalculateScore(problem: Problem) (solution: Solution): Score =
    let musicians =
        problem.Musicians
        |> Seq.zip solution.Placements
        |> Seq.map(fun(p, i) -> { Instrument = i; Location = p })
        |> Seq.toArray
    problem.Attendees |> Array.sumBy(CalculateAttendeeScore problem.Pillars musicians)

let CalculateNoBlockingScore(problem: Problem) (solution: IPartialSolution): Score =
    let musicians =
        solution.GetPlacedMusicians problem.Musicians
        |> Seq.map(fun(p, i) -> { Instrument = i; Location = p })
        |> Seq.toArray
    problem.Attendees |> Array.sumBy(CalculateAttendeeNoBlockingScore musicians)
