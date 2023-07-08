namespace Icfpc2023.Visualizer.Views

open Avalonia
open Avalonia.Controls
open Avalonia.Media
open Avalonia.Threading
open Icfpc2023
open Icfpc2023.Visualizer.ViewModels

type FieldView() =
    inherit Control()

    let RoomOutline = Pen(Brushes.Black)
    let StageFill = Brushes.LightBlue
    let StageOutline = Pen(Brushes.DarkGray)
    let AttendeeDisplayRadius = 1.0
    let AttendeeFill = Brushes.Red
    let PillarDisplayRadius = 1.0
    let PillarFill = Brushes.DarkGreen
    let MusicianDisplayRadius = 1.0
    let MusicianFill = Brushes.Blue

    member this.ViewModel = this.DataContext :?> FieldViewModel

    override this.Render(context) =
        this.DrawRoom(context)
        this.DrawStage(context)
        this.ViewModel.Problem.Attendees |> Seq.iter(this.DrawAttendee context)
        this.ViewModel.Solution
        |> Option.toArray
        |> Seq.collect (fun s -> s.Placements)
        |> Seq.iter(this.DrawMusician context)
        this.ViewModel.Problem.Pillars |> Seq.iter(this.DrawPillar context)

    override this.OnDataContextChanged _ =
        Dispatcher.UIThread.InvokeAsync(fun () -> this.InvalidateVisual()) |> ignore

    member private this.TransformDistance d =
        d * this.ViewModel.Scale

    member private this.TransformPoint(x, y) =
        Point(this.TransformDistance x, this.Bounds.Height - this.TransformDistance y)

    member private this.TransformPoint(p: Point) =
        this.TransformPoint(p.X, p.Y)

    member private this.TransformRect(rect: Rect) =
        let topLeft = this.TransformPoint(rect.BottomLeft)
        let bottomRight = this.TransformPoint(rect.TopRight)
        Rect(topLeft, bottomRight)

    member private this.DrawRoom(context: DrawingContext) =
        let problem = this.ViewModel.Problem
        let rect = Rect(0, 0, problem.RoomWidth, problem.RoomHeight)
        context.DrawRectangle(RoomOutline, this.TransformRect(rect))

    member private this.DrawStage(context: DrawingContext) =
        let problem = this.ViewModel.Problem
        let bottomLeft = problem.StageBottomLeft
        let bottomLeft = Point(bottomLeft.X, bottomLeft.Y)
        let rect = Rect(bottomLeft, Size(problem.StageWidth, problem.StageHeight))
        context.DrawRectangle(StageFill, StageOutline, this.TransformRect(rect))

    member private this.DrawAttendee(context: DrawingContext) (attendee: Attendee) =
        let radius = AttendeeDisplayRadius
        context.DrawEllipse(AttendeeFill, null, this.TransformPoint(attendee.X, attendee.Y), radius, radius)

    member private this.DrawMusician(context: DrawingContext) (musicianPos: PointD) =
        let radius = MusicianDisplayRadius
        context.DrawEllipse(MusicianFill, null, this.TransformPoint(musicianPos.X, musicianPos.Y), radius, radius)

    member private this.DrawPillar(context: DrawingContext) (pillar: Pillar) =
        let radius = this.TransformDistance pillar.Radius
        context.DrawEllipse(PillarFill, null, this.TransformPoint(pillar.Center.X, pillar.Center.Y), radius, radius)
