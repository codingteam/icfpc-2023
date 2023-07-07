#!/usr/bin/env python3

import math

OVERLAP_DISTANCE = 5

# distance between A and M
def distance_point(A, M):
  return math.sqrt((A.x - M.x)**2 + (A.y - M.y)**2)

# square distance between A and M
def distance2_point(A, M):
  return (A.x - M.x)**2 + (A.y - M.y)**2

# compute parameters of line a*x + b*y + c = 0 which pass through A and M points
def line_parameter(A, M):
  a = A.y - M.y
  b = M.x - A.x
  c = -a*M.x - b*M.y
  return (a, b, c)

# compute distance between line (a*x + b*y + c = 0) and point
# base points (B1, B2) are using to determine if point is between them, otherwise we can set distance to infinity (1000 is enough)
def distance_point_line(line, point, B1, B2):
  a, b, c = line
  x0, y0 = point
  x_line = (b*( b*x0 - a*y0) - a*c) / (a*a + b*b)
  y_line = (a*(-b*x0 + a*y0) - b*c) / (a*a + b*b)
  if (B1.x <= x_line <= B2.x) and (B1.y <= y_line <= B2.y):
    return 1000.0
  if (B1.x => x_line => B2.x) and (B1.y => y_line => B2.y):
    return 1000.0
  return distance_point(Point(x0, y0), Point(x_line, y_line))

# lambda factor (jt-th Musician between i-th Attendee and j-th Musician)
# when overlapping is, return 0
# when no overlapping, return 1
# the idea is to replace step function with continuous function with some parameter for easier gradient-based optimization algorithms
# lambda changes behavior of gate
def lambda_factor(labda, Ai, Mj, Mjt):
  line_Ai_Mj = line_parameter(Ai, Mj) # (a, b, c) typle
  point_Mjt = (Mjt.x, Mjt.y) # (x0, y0) typle
  return 2. / math.pi * math.atan(labda*(distance_point_line(line_Ai_Mj, Mjt) - OVERLAP_DISTANCE)) + 1

# lambda score between A_i and M_j
#
# A - Attendee, i-th
# M - Musician, j-th
# T - taste matrix
# lambda - parameter; exact solution when -> infty
def lambda_score_AiMj(A, M, i, j, T, labda):
  res = T[i,j] // distance2_point(A[i], M[j])
  for jt in range(0, len(M)):
    if jt != j:
      res *= lambda_factor(labda, A[i], M[j], M[jt])
  return res

# lambda score between M_i and M_j
#
# M - Musician, i-th
# M - Musician, j-th
# lambda - parameter; exact solution when -> infty
def lambda_score_MiMj(M, i, j, labda):
  return (2. / math.pi * math.atan(labda * (distance_point(M[i], M[j]) - MUSICAL_MIN_DISTANCE)) + 1) * 100

# to test
class Point:
  def __init__(self):
    self.x = 0
    self.y = 0
  def __init__(self, x, y):
    self.x = x
    self.y = y

p1 = Point(2,3)
p2 = Point(2,4)
a,b,c = line_parameter(p1, p2)
print(a,b,c)
print(a*p1.x + b*p1.y + c)
print(a*p2.x + b*p2.y + c)

p1 = Point(3,2)
p2 = Point(4,2)
a,b,c = line_parameter(p1, p2)
print(a,b,c)
print(a*p1.x + b*p1.y + c)
print(a*p2.x + b*p2.y + c)

p1 = Point(3,3)
p2 = Point(4,4)
a,b,c = line_parameter(p1, p2)
print(a,b,c)
print(a*p1.x + b*p1.y + c)
print(a*p2.x + b*p2.y + c)

p1 = Point(3,3)
p2 = Point(6,0)
a,b,c = line_parameter(p1, p2)
print(a,b,c)
print(a*p1.x + b*p1.y + c)
print(a*p2.x + b*p2.y + c)

