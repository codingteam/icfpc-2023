module Icfpc2023.Converter

open System.Text

let ToIni (problem: Problem) (solution: Solution option): string =
    let result = StringBuilder()
    result.AppendLine $"""[version]
1
[room_height]
{string problem.RoomHeight}
[room_width]
{string problem.RoomWidth}
[stage_height]
{string problem.StageHeight}
[stage_width]
{string problem.StageWidth}
[stage_bottom_left]
{string problem.StageBottomLeft.X} {problem.StageBottomLeft.Y}
[musicians]
{string problem.Musicians.Length}""" |> ignore
    let musicianPositions =
        solution
        |> Option.map(fun s -> Seq.ofArray s.Placements)
        |> Option.defaultValue(Seq.initInfinite (fun _ -> PointD(-1.0, -1.0)))
    for instrument, position in Seq.zip problem.Musicians musicianPositions do
        result.AppendLine $"{string position.X} {string position.Y} {string instrument}" |> ignore
    result.AppendLine $"""[attendees]
{string problem.Attendees.Length}""" |> ignore
    for attendee in problem.Attendees do
        result.Append $"{string attendee.X} {string attendee.Y}" |> ignore
        for taste in attendee.Tastes do
            result.Append $" {string taste}" |> ignore
        result.AppendLine() |> ignore

    result.AppendLine $"""[pillars]
{string problem.Pillars.Length}""" |> ignore
    for pillar in problem.Pillars do
        result.AppendLine $"{string pillar.Center.X} {string pillar.Center.Y} {string pillar.Radius}" |> ignore

    result.ToString().ReplaceLineEndings("\n")
