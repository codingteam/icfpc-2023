namespace Icfpc2023.Visualizer.Views

open Avalonia
open Avalonia.Controls
open Avalonia.Input
open Avalonia.Media
open Avalonia.Threading
open Icfpc2023
open Icfpc2023.Visualizer.ViewModels

type FieldView() =
    inherit Control()

    let ControlBackgroundFill = Brushes.AliceBlue
    let RoomOutline = Pen(Brushes.Black)
    let StageFill = Brushes.LightBlue
    let StageOutline = Pen(Brushes.DarkGray)
    let AttendeeDisplayRadius = 1.0
    let AttendeeFill = Brushes.Red
    let PillarFill = Brushes.DarkGreen
    let MusicianDisplayRadius = 1.0
    let MusicianFill = Brushes.Blue
    let MusicianSelectedFill = Brushes.Yellow
    let SelectionPen = Pen(Brushes.Brown)

    member this.ViewModel = this.DataContext :?> FieldViewModel

    member private this.DistanceFromModel d =
        d * this.ViewModel.Scale

    member private this.DistanceToModel d =
        d / this.ViewModel.Scale

    member private this.PointFromModel(x, y) =
        Point(this.DistanceFromModel x, this.Bounds.Height - this.DistanceFromModel y)

    member private this.PointFromModel(p: Point) =
        this.PointFromModel(p.X, p.Y)

    member private this.PointFromModel(p: PointD) =
        this.PointFromModel(p.X, p.Y)

    member private this.RectFromModel(rect: Rect) =
        let topLeft = this.PointFromModel(rect.BottomLeft)
        let bottomRight = this.PointFromModel(rect.TopRight)
        Rect(topLeft, bottomRight)

    member private this.PointToModel(x, y) =
        PointD(this.DistanceToModel x, this.DistanceToModel(this.Bounds.Height - y))

    member private this.PointToModel(p: Point) =
        this.PointToModel(p.X, p.Y)

    override this.OnPointerPressed(e) =
        printfn "Pointer pressed"
        if e.GetCurrentPoint(this).Properties.IsLeftButtonPressed then
            this.ViewModel.StartSelection(this.PointToModel(e.GetPosition this))

    override this.OnPointerReleased(e) =
        printfn "Pointer released"
        if e.InitialPressMouseButton = MouseButton.Left then
            this.ViewModel.FinishSelection(this.PointToModel(e.GetPosition this))
        else if e.InitialPressMouseButton = MouseButton.Right then
            this.ViewModel.MoveSelectedTo(this.PointToModel(e.GetPosition this))
            printfn $"Selected musicians: {this.ViewModel.SelectedMusicians.Count}"
        this.InvalidateVisual()

    override this.OnPointerMoved(e) =
        if this.ViewModel.SelectionActive then
            this.ViewModel.ExtendSelection(this.PointToModel(e.GetPosition this))
            this.InvalidateVisual()

    override this.Render(context) =
        this.DrawBackground(context)
        this.DrawRoom(context)
        this.DrawStage(context)
        this.ViewModel.Problem.Attendees |> Seq.iter(this.DrawAttendee context)
        this.ViewModel.Solution
        |> Option.toArray
        |> Seq.collect (fun s -> s.Placements)
        |> Seq.zip this.ViewModel.Problem.Musicians
        |> Seq.map MusicianModel
        |> Seq.iter(this.DrawMusician context)
        this.ViewModel.Problem.Pillars |> Seq.iter(this.DrawPillar context)
        this.DrawSelection(context)

    override this.OnDataContextChanged _ =
        Dispatcher.UIThread.InvokeAsync(fun () -> this.InvalidateVisual()) |> ignore

    member private this.DrawBackground(context: DrawingContext) =
        context.DrawRectangle(ControlBackgroundFill, null, this.Bounds)

    member private this.DrawRoom(context: DrawingContext) =
        let problem = this.ViewModel.Problem
        let rect = Rect(0, 0, problem.RoomWidth, problem.RoomHeight)
        context.DrawRectangle(RoomOutline, this.RectFromModel(rect))

    member private this.DrawStage(context: DrawingContext) =
        let problem = this.ViewModel.Problem
        let bottomLeft = problem.StageBottomLeft
        let bottomLeft = Point(bottomLeft.X, bottomLeft.Y)
        let rect = Rect(bottomLeft, Size(problem.StageWidth, problem.StageHeight))
        context.DrawRectangle(StageFill, StageOutline, this.RectFromModel(rect))

    member private this.DrawAttendee(context: DrawingContext) (attendee: Attendee) =
        let radius = AttendeeDisplayRadius
        context.DrawEllipse(AttendeeFill, null, this.PointFromModel(attendee.X, attendee.Y), radius, radius)

    member private this.DrawMusician(context: DrawingContext) (musician: MusicianModel) =
        let radius = MusicianDisplayRadius
        let fill =
            if this.ViewModel.SelectedMusicians.Contains(musician)
            then MusicianSelectedFill
            else MusicianFill
        let center = this.PointFromModel(musician.Position.X, musician.Position.Y)
        context.DrawEllipse(fill, null, center, radius, radius)

    member private this.DrawPillar(context: DrawingContext) (pillar: Pillar) =
        let radius = this.DistanceFromModel pillar.Radius
        context.DrawEllipse(PillarFill, null, this.PointFromModel(pillar.Center.X, pillar.Center.Y), radius, radius)

    member private this.DrawSelection(context: DrawingContext) =
        if this.ViewModel.SelectionActive then
            printfn $"Selection rect: {this.ViewModel.SelectionStart} -> {this.ViewModel.SelectionEnd}"
            let selectionDisplayRect =
                Rect(
                    this.PointFromModel(this.ViewModel.SelectionStart),
                    this.PointFromModel(this.ViewModel.SelectionEnd)
                )
            printfn $"Selection display rect: {selectionDisplayRect} ({selectionDisplayRect.Width} x {selectionDisplayRect.Height})"
            context.DrawRectangle(SelectionPen, selectionDisplayRect)
