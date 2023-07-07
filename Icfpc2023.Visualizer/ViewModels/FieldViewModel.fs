namespace Icfpc2023.Visualizer.ViewModels

open Icfpc2023

type FieldViewModel(path: string, problem: Problem) =
    member val Path = path
    member val Problem = problem

    member val Scale = 0.1 with get, set
