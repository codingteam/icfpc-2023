namespace Icfpc2023.Visualizer.ViewModels

open Icfpc2023

type FieldViewModel(problem: Problem) =
    member val Problem = problem

    member val Scale = 0.1 with get, set
