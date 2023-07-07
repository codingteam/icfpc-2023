module Icfpc2023.Scoring

[<Struct>]
type private Musician = {
    Instrument: int
    Location: PointD
}

let private CalculateAttendeeMusicianScore (attendee: Attendee) (musician: Musician): double =
    let d_squared = (attendee.X - musician.Location.X) ** 2.0 + (attendee.Y - musician.Location.Y) ** 2.0
    ceil(1_000_000.0 * attendee.Tastes[musician.Instrument] / d_squared)

let private AnyOtherMusicianBlocksSound (musicians: Musician[]) (attendee: Attendee) (mIndex: int): bool =
    let musician = PointD(musicians.[mIndex].Location.X, musicians.[mIndex].Location.Y)
    let attendee = PointD(attendee.X, attendee.Y)
    let blockZone = { Center1 = musician; Center2 = attendee; Radius = 5.0 }

    Seq.indexed musicians
    |> Seq.filter(fun (i, _) -> i <> mIndex)
    |> Seq.exists(fun (_, m) -> blockZone.Contains(m.Location))

let private CalculateAttendeeScore (musicians: Musician[]) (attendee: Attendee): double =
    Seq.indexed musicians
    |> Seq.sumBy(fun (i, musician) ->
        if AnyOtherMusicianBlocksSound musicians attendee i then 0.0
        else CalculateAttendeeMusicianScore attendee musician
    )

let CalculateScore(problem: Problem) (solution: Solution): double =
    let musicians =
        problem.Musicians
        |> Seq.zip solution.Placements
        |> Seq.map(fun(p, i) -> { Instrument = i; Location = p })
        |> Seq.toArray
    problem.Attendees |> Array.sumBy(CalculateAttendeeScore musicians)
