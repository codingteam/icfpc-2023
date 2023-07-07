module Icfpc2023.LambdaScore

open System
open System.Linq

let OVERLAP_DISTANCE = 5.0
let MUSICAL_MIN_DISTANCE = 10.0

let distance_point(A: PointD, M) =
  A.DistanceTo(M)

// square distance between A and M
let distance2_point(A: PointD, M) =
  A.SquaredDistanceTo(M)

// compute parameters of line a*x + b*y + c = 0 which pass through A and M points
let line_parameter(A: PointD, M: PointD) =
  let a = A.Y - M.Y
  let b = M.X - A.X
  let c = -a*M.X - b*M.Y
  a, b, c

// compute distance between line (a*x + b*y + c = 0) and point
// base points (B1, B2) are using to determine if point is between them, otherwise we can set distance to infinity (1000 is enough)
let distance_point_line(line, point, B1: PointD, B2: PointD) =
  let a, b, c = line
  let (PointD(x0, y0)) = point
  let x_line = (b*( b*x0 - a*y0) - a*c) / (a*a + b*b)
  let y_line = (a*(-b*x0 + a*y0) - b*c) / (a*a + b*b)
  if B1.X > x_line && B2.X > x_line then
    1000.0
  else if B1.X < x_line && B2.X < x_line then
    1000.0
  else if B1.Y > y_line && B2.Y > y_line then
    1000.0
  else if B1.Y < y_line && B2.Y < y_line then
    1000.0
  else distance_point(PointD(x0, y0), PointD(x_line, y_line))

// lambda factor (jt-th Musician between i-th Attendee and j-th Musician)
// when overlapping is, return 0
// when no overlapping, return 1
// the idea is to replace step function with continuous function with some parameter for easier gradient-based optimization algorithms
// lambda changes behavior of gate
let lambda_factor(labda, Ai, Mj: PointD, Mjt: PointD) =
  let line_Ai_Mj = line_parameter(Ai, Mj) // (a, b, c) typle
  let point_Mjt = (Mjt.X, Mjt.Y) // (x0, y0) typle
  2. / Math.PI * atan(labda*(distance_point_line(line_Ai_Mj, Mjt, Ai, Mj) - OVERLAP_DISTANCE)) + 1.0

// lambda score between A_i and M_j
//
// A - Attendee, i-th
// M - Musician, j-th
// T - taste matrix
// lambda - parameter; exact solution when -> infty
let lambda_score_AiMj(A: PointD[], M: PointD[], i, j, T: double[,], labda) =
  let mutable res = T[i,j] // distance2_point(A[i], M[j])
  for jt in Enumerable.Range(0, M.Length) do
    if jt <> j then
      res <- res * lambda_factor(labda, A[i], M[j], M[jt])
  res

// lambda score between M_i and M_j
//
// M - Musician, i-th
// M - Musician, j-th
// lambda - parameter; exact solution when -> infty
let lambda_score_MiMj(M: PointD[], i, j, labda) =
  (2. / Math.PI * atan(labda * (distance_point(M[i], M[j]) - MUSICAL_MIN_DISTANCE)) + 1.0) * 100.0

let DoTest() =
    let p1 = PointD(2,3)
    let p2 = PointD(2,4)
    let a,b,c = line_parameter(p1, p2)
    printfn "%A" (a,b,c)
    printfn "%A" (a*p1.X + b*p1.Y + c)
    printfn "%A" (a*p2.X + b*p2.Y + c)

    let p1 = PointD(3,2)
    let p2 = PointD(4,2)
    let a,b,c = line_parameter(p1, p2)
    printfn "%A" (a,b,c)
    printfn "%A" (a*p1.X + b*p1.Y + c)
    printfn "%A" (a*p2.X + b*p2.Y + c)

    let p1 = PointD(3,3)
    let p2 = PointD(4,4)
    let a,b,c = line_parameter(p1, p2)
    printfn "%A" (a,b,c)
    printfn "%A" (a*p1.X + b*p1.Y + c)
    printfn "%A" (a*p2.X + b*p2.Y + c)

    let p1 = PointD(3,3)
    let p2 = PointD(6,0)
    let a,b,c = line_parameter(p1, p2)
    printfn "%A" (a,b,c)
    printfn "%A" (a*p1.X + b*p1.Y + c)
    printfn "%A" (a*p2.X + b*p2.Y + c)

