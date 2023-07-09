module Icfpc2023.LambdaSolver

open Accord.Math.Optimization
open System
open System.Linq

#nowarn "25"
#nowarn "49"

let OVERLAP_DISTANCE = 5.0
let MUSICAL_MIN_DISTANCE = 10.0

// #################################################
//
// helpers
//
// #################################################

let attendeesPositions (problem: Problem) =
    Array.init problem.Attendees.Length (fun i ->
        let a = problem.Attendees[i]
        PointD(a.X, a.Y)
    )

let tasteMatrix (problem: Problem) =
    Array2D.init problem.Attendees.Length problem.Musicians.Length (fun i j ->
        problem.Attendees[i].Tastes[problem.Musicians[j]]
    )

// #################################################
//
// energy Ai-Mj interaction
//
// #################################################

// compute parameters of line a*x + b*y + c = 0 which pass through A and M points
let line_parameter(A: PointD, M: PointD) =
  let a = A.Y - M.Y
  let b = M.X - A.X
  let c = -a*M.X - b*M.Y
  a, b, c

// compute distance between line (a*x + b*y + c = 0) and point P
// base points (B1, B2) are using to determine if point is between them, otherwise we can set distance to infinity (1000 is enough)
let distance_point_line(line: double*double*double, P: PointD, B1: PointD, B2: PointD) =
  let a, b, c = line
  let x_line = (b*( b*P.X - a*P.Y) - a*c) / (a*a + b*b)
  let y_line = (a*(-b*P.X + a*P.Y) - b*c) / (a*a + b*b)
  if B1.X > x_line && B2.X > x_line then
    1000.0
  else if B1.X < x_line && B2.X < x_line then
    1000.0
  else if B1.Y > y_line && B2.Y > y_line then
    1000.0
  else if B1.Y < y_line && B2.Y < y_line then
    1000.0
  else P.DistanceTo(PointD(x_line, y_line))

// lambda factor (jt-th Musician between i-th Attendee and j-th Musician)
// when overlapping is, return 0
// when no overlapping, return 1
// the idea is to replace step function with continuous function with some parameter for easier gradient-based optimization algorithms
// lambda changes behavior of gate
let lambda_factor(lambda: double, Ai: PointD, Mj: PointD, Mjt: PointD) =
  let line_Ai_Mj = line_parameter(Ai, Mj) // (a, b, c) typle
  let distance = distance_point_line(line_Ai_Mj, Mjt, Ai, Mj)
  let tmp = (2. / Math.PI * atan(lambda*(distance - OVERLAP_DISTANCE)) + 1.0) / 2.
  tmp

// lambda score between A_i and M_j
//
// A - Attendee, i-th
// M - Musician, j-th
// T - taste matrix
// lambda - parameter; exact solution when -> infty
let lambda_score_AiMj(A: PointD[], M: PointD[], i, j, T: double[,], lambda: double) =
  let mutable res = T[i,j] / A[i].SquaredDistanceTo(M[j])
  for jt in Enumerable.Range(0, M.Length) do
    if jt <> j then
      res <- res * lambda_factor(lambda, A[i], M[j], M[jt])
  res * 1e6

// #################################################
//
// derivative of Ai-Mj interaction over Mj
//
// #################################################

// compute derivative of parameters of line a*x + b*y + c = 0 over point M which pass through A and M points
let line_parameter_deriv(A: PointD, M: PointD) =
  let a = A.Y - M.Y
  let b = M.X - A.X
  let da = PointD(0, -1)
  let db = PointD(1, 0)
  let dc = PointD(-a, -b)
  da, db, dc

// compute derivative of distance between line (a*x + b*y + c = 0) and point P over line
// base points (B1, B2) are using to determine if point is between them, otherwise we can set distance to infinity (1000 is enough)
let distance_point_line_deriv_line(line: double*double*double, linederiv: PointD*PointD*PointD, P: PointD, B1: PointD, B2: PointD) =
  let a, b, c = line
  let da, db, dc = linederiv
  let x_line = (b*( b*P.X - a*P.Y) - a*c) / (a*a + b*b)
  let y_line = (a*(-b*P.X + a*P.Y) - b*c) / (a*a + b*b)
  if B1.X > x_line && B2.X > x_line then
    PointD(0, 0)
  else if B1.X < x_line && B2.X < x_line then
    PointD(0, 0)
  else if B1.Y > y_line && B2.Y > y_line then
    PointD(0, 0)
  else if B1.Y < y_line && B2.Y < y_line then
    PointD(0, 0)
  else
    PointD(((-P.X + ((P.X*b - P.Y*a)*b - a*c)/(a*a + b*b))*(2.0*((P.X*b - P.Y*a)*b - a*c)*(-2.0*a*da.X - 2.0*b*db.X)/(a*a + b*b) ** 2.0 + 2.0*((P.X*b - P.Y*a)*db.X + (P.X*db.X - P.Y*da.X)*b - a*dc.X - c*da.X)/(a*a + b*b))/2.0 + (-P.Y + ((-P.X*b + P.Y*a)*a - b*c)/(a*a + b*b))*(2.0*((-P.X*b + P.Y*a)*a - b*c)*(-2.0*a*da.X - 2.0*b*db.X)/(a*a + b*b) ** 2.0 + 2.0*((-P.X*b + P.Y*a)*da.X + (-P.X*db.X + P.Y*da.X)*a - b*dc.X - c*db.X)/(a*a + b*b))/2.0)/sqrt((-P.X + ((P.X*b - P.Y*a)*b - a*c)/(a*a + b*b)) ** 2.0 + (-P.Y + ((-P.X*b + P.Y*a)*a - b*c)/(a*a + b*b)) ** 2.0),
           ((-P.X + ((P.X*b - P.Y*a)*b - a*c)/(a*a + b*b))*(2.0*((P.X*b - P.Y*a)*b - a*c)*(-2.0*a*da.Y - 2.0*b*db.Y)/(a*a + b*b) ** 2.0 + 2.0*((P.X*b - P.Y*a)*db.Y + (P.X*db.Y - P.Y*da.Y)*b - a*dc.Y - c*da.Y)/(a*a + b*b))/2.0 + (-P.Y + ((-P.X*b + P.Y*a)*a - b*c)/(a*a + b*b))*(2.0*((-P.X*b + P.Y*a)*a - b*c)*(-2.0*a*da.Y - 2.0*b*db.Y)/(a*a + b*b) ** 2.0 + 2.0*((-P.X*b + P.Y*a)*da.Y + (-P.X*db.Y + P.Y*da.Y)*a - b*dc.Y - c*db.Y)/(a*a + b*b))/2.0)/sqrt((-P.X + ((P.X*b - P.Y*a)*b - a*c)/(a*a + b*b)) ** 2.0 + (-P.Y + ((-P.X*b + P.Y*a)*a - b*c)/(a*a + b*b)) ** 2.0))

// derivative of lambda factor over Mj (jt-th Musician between i-th Attendee and j-th Musician)
// when overlapping is, return 0
// when no overlapping, return 1
// the idea is to replace step function with continuous function with some parameter for easier gradient-based optimization algorithms
// lambda changes behavior of gate
let lambda_factor_deriv_Mj(lambda: double, Ai: PointD, Mj: PointD, Mjt: PointD) =
(*
  let line_Ai_Mj = line_parameter(Ai, Mj) // (a, b, c) typle
  let line_Ai_Mj_der = line_parameter_deriv(Ai, Mj) // (da, db, dc) typle
  let distance = distance_point_line(line_Ai_Mj, Mjt, Ai, Mj)
  let line_der = distance_point_line_deriv_line(line_Ai_Mj, line_Ai_Mj_der, Mjt, Ai, Mj)
*)
  let line_Ai_Mj    = line_parameter(Ai, Mj) // (a, b, c) typle
  let line_Ai_Mj_dx = line_parameter(Ai, Mj + PointD(1e-6, 0)) // (a, b, c) typle
  let line_Ai_Mj_dy = line_parameter(Ai, Mj + PointD(0, 1e-6)) // (a, b, c) typle
  let distance    = distance_point_line(line_Ai_Mj,    Mjt, Ai, Mj)
  let distance_dx = distance_point_line(line_Ai_Mj_dx, Mjt, Ai, Mj + PointD(1e-6, 0))
  let distance_dy = distance_point_line(line_Ai_Mj_dy, Mjt, Ai, Mj + PointD(0, 1e-6))
  let line_der = PointD(distance_dx - distance, distance_dy- distance) / 1e-6
//  printfn $"{line_der.X} {line_der.Y}"
  line_der * 2. / Math.PI * lambda / (lambda * lambda * (distance - OVERLAP_DISTANCE)*(distance - OVERLAP_DISTANCE) + 1.0)

// lambda score derivative between A_i and M_j over M_j
//
// A - Attendee, i-th
// M - Musician, j-th
// T - taste matrix
// lambda - parameter; exact solution when -> infty
let lambda_derivative_AiMj_Mj(A: PointD[], M: PointD[], i, j, T: double[,], lambda: double) =
  let mutable res1 = PointD(-2.0 * T[i,j] * (A[i].X - M[j].X) / (A[i].SquaredDistanceTo(M[j])) ** 2.0,
                            -2.0 * T[i,j] * (A[i].Y - M[j].Y) / (A[i].SquaredDistanceTo(M[j])) ** 2.0)
  let mutable res2 = PointD(0, 0)
  for jt in Enumerable.Range(0, M.Length) do
    if jt <> j then
      res1 <- res1 * lambda_factor(lambda, A[i], M[j], M[jt])
  for jtt in Enumerable.Range(0, M.Length) do
    if jtt <> j then
      let mutable Pt = lambda_factor_deriv_Mj(lambda, A[i], M[j], M[jtt])
      for jt in Enumerable.Range(0, M.Length) do
        if jt <> j || jt <> jtt then
          Pt <- Pt * lambda_factor(lambda, A[i], M[j], M[jt])
      res2 <- res2 + Pt
// FIXME: NaN
  res2 <- res2 * (T[i,j] / A[i].SquaredDistanceTo(M[j]))
  res1 + res2

// #################################################
//
// derivative of Ai-Mj interaction over Mjt
//
// #################################################

// compute derivative distance between line (a*x + b*y + c = 0) and point P over point P
// base points (B1, B2) are using to determine if point is between them, otherwise we can set distance to infinity (1000 is enough)
let distance_point_line_deriv_point(line: double*double*double, P: PointD, B1: PointD, B2: PointD) =
  let a, b, c = line
  let x_line = (b*( b*P.X - a*P.Y) - a*c) / (a*a + b*b)
  let y_line = (a*(-b*P.X + a*P.Y) - b*c) / (a*a + b*b)
  if B1.X > x_line && B2.X > x_line then
    PointD(0, 0)
  else if B1.X < x_line && B2.X < x_line then
    PointD(0, 0)
  else if B1.Y > y_line && B2.Y > y_line then
    PointD(0, 0)
  else if B1.Y < y_line && B2.Y < y_line then
    PointD(0, 0)
  else
    PointD(((-2.0 + 2.0*b*b/(a*a + b*b))*(-P.X + ((P.X*b - P.Y*a)*b - a*c)/(a*a + b*b))/2.0 - (-P.Y + ((-P.X*b + P.Y*a)*a - b*c)/(a*a + b*b))*a*b/(a*a + b*b))/sqrt((-P.X + ((P.X*b - P.Y*a)*b - a*c)/(a*a + b*b)) ** 2.0 + (-P.Y + ((-P.X*b + P.Y*a)*a - b*c)/(a*a + b*b)) ** 2.0),
           ((-2.0 + 2.0*a*a/(a*a + b*b))*(-P.Y + ((-P.X*b + P.Y*a)*a - b*c)/(a*a + b*b))/2.0 - (-P.X + ((P.X*b - P.Y*a)*b - a*c)/(a*a + b*b))*a*b/(a*a + b*b))/sqrt((-P.X + ((P.X*b - P.Y*a)*b - a*c)/(a*a + b*b)) ** 2.0 + (-P.Y + ((-P.X*b + P.Y*a)*a - b*c)/(a*a + b*b)) ** 2.0))

// derivative of lambda factor over Mjt (jt-th Musician between i-th Attendee and j-th Musician)
// when overlapping is, return 0
// when no overlapping, return 1
// the idea is to replace step function with continuous function with some parameter for easier gradient-based optimization algorithms
// lambda changes behavior of gate
let lambda_factor_deriv_Mjt(lambda: double, Ai: PointD, Mj: PointD, Mjt: PointD) =
  let line_Ai_Mj = line_parameter(Ai, Mj) // (a, b, c) typle
  let distance = distance_point_line(line_Ai_Mj, Mjt, Ai, Mj)
  let point_der = distance_point_line_deriv_point(line_Ai_Mj, Mjt, Ai, Mj)
  point_der * 2. / Math.PI * lambda / (lambda * lambda * (distance - OVERLAP_DISTANCE)*(distance - OVERLAP_DISTANCE) + 1.0)

// lambda score derivative between A_i and M_j over M_jt
//
// A - Attendee, i-th
// M - Musician, j-th
// T - taste matrix
// lambda - parameter; exact solution when -> infty
// jt-th element is not j-th element; function returns an array of derivatives
let lambda_derivative_AiMj_Mjt(A: PointD[], M: PointD[], i, j, T: double[,], lambda: double) =
  let first = PointD(T[i,j] / (A[i].SquaredDistanceTo(M[j])),
                     T[i,j] / (A[i].SquaredDistanceTo(M[j])))
  let mutable musicianDeriv = Array.create M.Length first
  for jt in Enumerable.Range(0, M.Length) do
    if jt <> j then
      for e in Enumerable.Range(0, M.Length) do
        if e <> jt then
          musicianDeriv[jt] <- musicianDeriv[jt] * lambda_factor(lambda, A[i], M[j], M[jt])
        else
          // scalar product, derivative is from `lambda_factor_deriv_Mjt` only
          musicianDeriv[jt] <- musicianDeriv[jt] * lambda_factor_deriv_Mjt(lambda, A[i], M[j], M[jt])
  musicianDeriv

// #################################################
//
// energy Mi-Mj interaction
//
// #################################################

// lambda score between Mi and Mj
//
// Mi - Musician, i-th
// Mj - Musician, j-th
// lambda - parameter; exact solution when -> infty
let lambda_score_MiMj(Mi: PointD, Mj: PointD, lambda: double) =
  let distance = Mi.DistanceTo(Mj)
  if distance < MUSICAL_MIN_DISTANCE then
    -(- 2. / Math.PI * atan(lambda * (distance - MUSICAL_MIN_DISTANCE)) + 1.0) / 2. * 1e6 * 1e3
  else
    0.0

// #################################################
//
// derivative of Mi-Mj interaction over Mi
//
// #################################################

// lambda score derivative between Mi and Mj over Mi
//
// Mi - Musician, i-th
// Mj - Musician, j-th
// lambda - parameter; exact solution when -> infty
let lambda_score_MiMj_deriv_Mi(Mi: PointD, Mj: PointD) =
  let distance = Mi.DistanceTo(Mj)
  if distance > MUSICAL_MIN_DISTANCE then
    PointD(0.0, 0.0)
  else
    (Mj - Mi) * (- MUSICAL_MIN_DISTANCE / distance + 1.) * 1e3

// #################################################
//
// derivative of Mi-Mj interaction over Mj
//
// #################################################

// lambda score derivative between Mi and Mj over Mj
//
// Mi - Musician, i-th
// Mj - Musician, j-th
// lambda - parameter; exact solution when -> infty
let lambda_score_MiMj_deriv_Mj(Mi: PointD, Mj: PointD) =
  lambda_score_MiMj_deriv_Mi(Mj, Mi)

// #################################################
//
// energy Mi-borders interaction
//
// #################################################

let lambda_score_Mi_borders(Mi: PointD, problem: Problem) =
  if Mi.X < problem.StageBottomLeft.X + MUSICAL_MIN_DISTANCE then
    - 1e6 * 1e3
  else if Mi.X > problem.StageBottomLeft.X + problem.StageWidth - MUSICAL_MIN_DISTANCE then
    - 1e6 * 1e3
  else if Mi.Y < problem.StageBottomLeft.Y + MUSICAL_MIN_DISTANCE then
    - 1e6 * 1e3
  else if Mi.Y > problem.StageBottomLeft.Y + problem.StageHeight - MUSICAL_MIN_DISTANCE then
    - 1e6 * 1e3
  else
    0.0

// #################################################
//
// derivative of Mi-borders interaction over Mi
//
// #################################################

let lambda_score_Mi_border_deriv(Mi: PointD, problem: Problem) =
  if Mi.X < problem.StageBottomLeft.X + MUSICAL_MIN_DISTANCE then
    PointD(1e3, 0)
  else if Mi.X > problem.StageBottomLeft.X + problem.StageWidth - MUSICAL_MIN_DISTANCE then
    PointD(- 1e3, 0)
  else if Mi.Y < problem.StageBottomLeft.Y + MUSICAL_MIN_DISTANCE then
    PointD(0, 1e3)
  else if Mi.Y > problem.StageBottomLeft.Y + problem.StageHeight - MUSICAL_MIN_DISTANCE then
    PointD(0, - 1e3)
  else
    PointD(0, 0)

// #################################################
//
// Total energy and derivatives
//
// #################################################

let lambda_score(M: PointD[], problem: Problem, lambda: double) =
  let mutable res = 0.0
  let A = attendeesPositions problem
  let T = tasteMatrix problem
  for i in Enumerable.Range(0, A.Length) do
    for j in Enumerable.Range(0, M.Length) do
      res <- res + lambda_score_AiMj(A, M, i, j, T, lambda)
  for i in Enumerable.Range(0, M.Length) do
    for j in Enumerable.Range(0, M.Length) do
      if i <> j then
        res <- res + lambda_score_MiMj(M[i], M[j], lambda)
  for i in Enumerable.Range(0, M.Length) do
    res <- res + lambda_score_Mi_borders(M[i], problem)
  res

let lambda_deriv(M: PointD[], problem: Problem, lambda: double) =
  let mutable musicianDeriv = Array.create M.Length (PointD(0.0, 0.0))
  let A = attendeesPositions problem
  let T = tasteMatrix problem
  for i in Enumerable.Range(0, A.Length) do
    for j in Enumerable.Range(0, M.Length) do
      musicianDeriv[j] <- musicianDeriv[j] + lambda_derivative_AiMj_Mj(A, M, i, j, T, lambda)
      let ders = lambda_derivative_AiMj_Mjt(A, M, i, j, T, lambda)
      for k in Enumerable.Range(0, M.Length) do
        musicianDeriv[k] <- musicianDeriv[k] + ders[k]
  for i in Enumerable.Range(0, M.Length) do
    for j in Enumerable.Range(0, M.Length) do
      if i <> j then
        musicianDeriv[j] <- musicianDeriv[j] + lambda_score_MiMj_deriv_Mj(M[i], M[j]) * 0.5
  for i in Enumerable.Range(0, M.Length) do
    musicianDeriv[i] <- musicianDeriv[i] + lambda_score_Mi_border_deriv(M[i], problem)
  musicianDeriv

// #################################################
//
// Solver
//
// #################################################

let private lambdas = [0.01; 0.5; 1; 5; 10; 50; 100; 200; 500; 1000; 5000; 30000]

let private pointsToArray (points: PointD[]) =
    points
    |> Seq.map (fun p -> [| p.X; p.Y |])
    |> Seq.collect id
    |> Seq.toArray

let private arrayToPoints (array: double[]) =
    array
    |> Seq.chunkBySize 2
    |> Seq.map (fun [| x; y |] -> PointD(x, y))
    |> Seq.toArray

let private MusicianDeadZoneRadius = 10.0

let Solve (initialSolutionOpt: Solution option) (problem: Problem): Solution =
    let initialSolution =
        match initialSolutionOpt with
        | Some solution ->
            let initialScore = Scoring.CalculateScore problem solution
            printfn $"λ: Initial score: {initialScore}"
            solution
        | None ->
            printfn $"λ: Computing initial solution (foxtranV1)..."
            let initialSolution = FoxtranSolver.FoxtranSolveV1(problem)
            let initialScore = Scoring.CalculateScore problem initialSolution
            printfn $"λ: Initial score: {initialScore}"
            initialSolution

    let initialGuess = initialSolution.Placements |> pointsToArray
(*
    let res = lambda_score(arrayToPoints initialGuess, problem, 1000)
    let deriv = lambda_score_MiMj_deriv_Mj(PointD(8, 0), PointD(0, 0))
    printfn $"{deriv.X} {deriv.Y}"
    printfn $"{res}"
    for point in lambda_deriv(arrayToPoints initialGuess, problem, 1000) do
      printfn $"{point.X} {point.Y}"
    failwith "Nothing."
*)

    let mutable solution = initialGuess
    for lambda in lambdas do
        printfn $"λ: Current λ parameter: {lambda}"
        let objective = fun point -> lambda_score(arrayToPoints point, problem, lambda)
        let gradient = fun point -> lambda_deriv(arrayToPoints point, problem, lambda) |> pointsToArray


        let method =
            NelderMead(
                numberOfVariables = initialGuess.Length,
                ``function`` = objective)

        let lowerBounds = method.LowerBounds
        for i = 0 to problem.Musicians.Length - 1 do
            lowerBounds[i*2] <- problem.StageBottomLeft.X + MusicianDeadZoneRadius
            lowerBounds[i*2 + 1] <- problem.StageBottomLeft.Y + MusicianDeadZoneRadius

        let upperBounds = method.UpperBounds
        for i = 0 to problem.Musicians.Length - 1 do
            upperBounds[i*2] <- problem.StageBottomLeft.X + problem.StageWidth - MusicianDeadZoneRadius
            upperBounds[i*2 + 1] <- problem.StageBottomLeft.Y + problem.StageHeight - MusicianDeadZoneRadius

(*
        let method =
            BroydenFletcherGoldfarbShanno(
                numberOfVariables = initialGuess.Length,
                ``function`` = objective,
                gradient = gradient)
*)

        let success = method.Maximize(solution)
        printfn $"λ: Converged? {success}, status {method.Status}"

        solution <- method.Solution
        printfn $"λ: Objective value: {method.Value}"

        let score = Scoring.CalculateScore problem {
            Placements = solution |> arrayToPoints
            Volumes = Solution.defaultVolumes problem.Musicians.Length
        }
        printfn $"λ: Current score: {score}"

    { initialSolution with Placements = solution |> arrayToPoints }
