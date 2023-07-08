import Pkg
Pkg.UPDATED_REGISTRY_THIS_SESSION[] = true
Pkg.add("SymPy")
Pkg.add("SpecialFunctions")

using SymPy
using SpecialFunctions

@vars x y x0 y0
@symfuns a b c
a = a(x, y);
b = b(x, y);
c = c(x, y);

x_line = (b*( b*x0 - a*y0) - a*c) / (a*a + b*b);
y_line = (a*(-b*x0 + a*y0) - b*c) / (a*a + b*b);

dist = ((x_line-x0)^2+(y_line-y0)^2)^(1 // 2);

dist_dx = diff(dist, x);
dist_dy = diff(dist, y);

dist_dx0 = diff(dist, x0);
dist_dy0 = diff(dist, y0);

function reduce(Expr::String)
  Expr = replace(Expr, "(x, y)" => "")
  Expr = replace(Expr, "Derivative(a, x)" => "da.X")
  Expr = replace(Expr, "Derivative(a, y)" => "da.Y")
  Expr = replace(Expr, "Derivative(b, x)" => "db.X")
  Expr = replace(Expr, "Derivative(b, y)" => "db.Y")
  Expr = replace(Expr, "Derivative(c, x)" => "dc.X")
  Expr = replace(Expr, "Derivative(c, y)" => "dc.Y")
  Expr = replace(Expr, "a^2" => "a*a")
  Expr = replace(Expr, "b^2" => "b*b")
  Expr = replace(Expr, "c^2" => "c*c")
  Expr = replace(Expr, "2" => "2.0")
  Expr = replace(Expr, "^" => " ** ")
  Expr = replace(Expr, "x0" => "P.X")
  Expr = replace(Expr, "y0" => "P.Y")
end;

println("##########")
println("Dist:")
println(reduce(string(dist)))
println()
println("##########")
println("dDist/dLine:")
println(reduce(string(dist_dx)))
println(reduce(string(dist_dy)))
println()
println("##########")
println("dDist/dPoint:")
println(reduce(string(dist_dx0)))
println(reduce(string(dist_dy0)))
println()