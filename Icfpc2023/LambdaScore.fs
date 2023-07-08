module Icfpc2023.LambdaScore

open System
open System.Linq

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
let distance_point_line(line, P: PointD, B1: PointD, B2: PointD) =
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
  2. / Math.PI * atan(lambda*(distance_point_line(line_Ai_Mj, Mjt, Ai, Mj) - OVERLAP_DISTANCE)) + 1.0

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
  res

// #################################################
//
// derivative of Ai-Mj interaction over Mj
//
// #################################################

// compute derivative of parameters of line a*x + b*y + c = 0 over point M which pass through A and M points
let line_parameter_deriv(A: PointD, M: PointD) =
  let a = PointD(0, -1)
  let b = PointD(1, 0)
  let c = PointD(-a, -b)
  a, b, c

// compute derivative of distance between line (a*x + b*y + c = 0) and point P over line
// base points (B1, B2) are using to determine if point is between them, otherwise we can set distance to infinity (1000 is enough)
let distance_point_line_deriv_line(line, linederiv, P: PointD, B1: PointD, B2: PointD) =
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
    PointD(((-x0 + ((x0*b - y0*a)*b - a*c)/(a*a + b*b))*(2*((x0*b - y0*a)*b - a*c)*(-2*a*da.X - 2*b*db.X)/(a*a + b*b) ** 2.0 + 2*((x0*b - y0*a)*db.X + (x0*db.X - y0*da.X)*b - a*dc.X - c*da.X)/(a*a + b*b))/2 + (-y0 + ((-x0*b + y0*a)*a - b*c)/(a*a + b*b))*(2*((-x0*b + y0*a)*a - b*c)*(-2*a*da.X - 2*b*db.X)/(a*a + b*b) ** 2.0 + 2*((-x0*b + y0*a)*da.X + (-x0*db.X + y0*da.X)*a - b*dc.X - c*db.X)/(a*a + b*b))/2)/sqrt((-x0 + ((x0*b - y0*a)*b - a*c)/(a*a + b*b)) ** 2.0 + (-y0 + ((-x0*b + y0*a)*a - b*c)/(a*a + b*b)) ** 2.0),
           ((-x0 + ((x0*b - y0*a)*b - a*c)/(a*a + b*b))*(2*((x0*b - y0*a)*b - a*c)*(-2*a*da.Y - 2*b*db.Y)/(a*a + b*b) ** 2.0 + 2*((x0*b - y0*a)*db.Y + (x0*db.Y - y0*da.Y)*b - a*dc.Y - c*da.Y)/(a*a + b*b))/2 + (-y0 + ((-x0*b + y0*a)*a - b*c)/(a*a + b*b))*(2*((-x0*b + y0*a)*a - b*c)*(-2*a*da.Y - 2*b*db.Y)/(a*a + b*b) ** 2.0 + 2*((-x0*b + y0*a)*da.Y + (-x0*db.Y + y0*da.Y)*a - b*dc.Y - c*db.Y)/(a*a + b*b))/2)/sqrt((-x0 + ((x0*b - y0*a)*b - a*c)/(a*a + b*b)) ** 2.0 + (-y0 + ((-x0*b + y0*a)*a - b*c)/(a*a + b*b)) ** 2.0))

// lambda score derivative between A_i and M_j over M_j
//
// A - Attendee, i-th
// M - Musician, j-th
// T - taste matrix
// lambda - parameter; exact solution when -> infty
// NOTE: derivatives over lambda_factor are not implemented yet
let lambda_derivative_AiMj_Mj(A: PointD[], M: PointD[], i, j, T: double[,], lambda: double) =
  let mutable res1 = PointD(-2.0 * T[i,j] * (A[i].X - M[j].X) / (A[i].SquaredDistanceTo(M[j])) ** 2.0,
                            -2.0 * T[i,j] * (A[i].Y - M[j].Y) / (A[i].SquaredDistanceTo(M[j])) ** 2.0)
  let mutable res2 = PointD(0, 0)
  for jt in Enumerable.Range(0, M.Length) do
    if jt <> j then
      res1 <- res1 * lambda_factor(lambda, A[i], M[j], M[jt])
  for jtt in Enumerable.Range(0, M.Length) do
    if jtt <> j then
      let mutable tmpX = lambda_factor(lambda, A[i], M[j], M[jtt]) // deriv over X
      let mutable tmpY = lambda_factor(lambda, A[i], M[j], M[jtt]) // deriv over Y
      for jt in Enumerable.Range(0, M.Length) do
        if jt <> j || jt <> jtt then
          tmpX <- tmpX * lambda_factor(lambda, A[i], M[j], M[jtt])
          tmpY <- tmpY * lambda_factor(lambda, A[i], M[j], M[jtt])
      res2 <- res2 + PointD(tmpX, tmpY)
  res2 <- res2 * (T[i,j] / A[i].SquaredDistanceTo(M[j]))
  res1 + res2

// #################################################
//
// derivative of Ai-Mj interaction over Mjt
//
// #################################################

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
  (2. / Math.PI * atan(lambda * (Mi.DistanceTo(Mj) - MUSICAL_MIN_DISTANCE)) + 1.0) * 100.0

// #################################################
//
// derivative of Mi-Mj interaction over Mi
//
// #################################################

// #################################################
//
// derivative of Mi-Mj interaction over Mj
//
// #################################################

// #################################################
//
// energy Mi-borders interaction
//
// #################################################

// #################################################
//
// derivative of Mi-borders interaction over Mi
//
// #################################################

