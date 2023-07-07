module Icfpc2023.Scoring

[<Struct>]
type private Musician = {
    Instrument: int
    Location: PointD
}

let private CalculateAttendeeMusicianScore (attendee: Attendee) (musician: Musician): double =
    // TODO: Determine if musician is blocked by anyone else
    let d_squared = (attendee.X - musician.Location.X) ** 2.0 + (attendee.Y - musician.Location.Y) ** 2.0
    ceil(1_000_000.0 * attendee.Tastes[musician.Instrument] / d_squared)

let private CalculateAttendeeScore (musicians: Musician[]) (attendee: Attendee): double =
    musicians
    |> Array.sumBy(CalculateAttendeeMusicianScore attendee)

let CalculateScore(problem: Problem) (solution: Solution): double =
    let musicians =
        problem.Musicians
        |> Seq.zip solution.Placements
        |> Seq.map(fun(p, i) -> { Instrument = i; Location = p })
        |> Seq.toArray
    problem.Attendees |> Array.sumBy(CalculateAttendeeScore musicians)
