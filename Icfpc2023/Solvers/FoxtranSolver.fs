module Icfpc2023.FoxtranSolver

let FoxtranSolveV1(problem: Problem): Solution =
    let vacantRadius = 10.0
    // grid over scene; compute effective potential for each musician
    let grid = seq {
      for x in vacantRadius .. vacantRadius .. problem.StageWidth-vacantRadius do
        for y in vacantRadius .. vacantRadius .. problem.StageHeight-vacantRadius do
          for musician in problem.Musicians ->
            // Point -> [scores]
    }
    // for each musician type
    //   find maximum over all musicians
    //     take this point
    //     remove points around selected point (set large negative value to these grid points over all musiians)
    {
        Placements = ... 
    }
