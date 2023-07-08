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
  Expr = replace(Expr, "^2" => "^2.0")
  Expr = replace(Expr, "^" => " ** ")
end;

println(reduce(string(dist)))
println(reduce(string(dist_dx)))
println(reduce(string(dist_dy)))