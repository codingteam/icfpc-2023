namespace Icfpc2023.Visualizer.Views

open Avalonia
open Avalonia.Controls
open Avalonia.Media
open Avalonia.Media.Immutable
open Avalonia.Rendering.SceneGraph
open Avalonia.Skia
open Icfpc2023.Visualizer.ViewModels
open SkiaSharp

type FieldView() =
    inherit Control()

    member this.ViewModel = this.DataContext :?> FieldViewModel

    override this.Render(context) =
        context.DrawEllipse(Brushes.Aqua, ImmutablePen(Brushes.DarkGoldenrod), Point(50, 50), 30, 30)
        ()
