namespace Icfpc2023.Visualizer.ViewModels

open Icfpc2023

type FieldViewModel(problemId: int, problem: Problem, solution: Solution) =
    member val ProblemId = problemId
    member val Problem = problem
    member val Solution = solution

    member val Scale = 0.1 with get, set
