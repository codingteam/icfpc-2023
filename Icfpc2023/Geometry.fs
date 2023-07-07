namespace Icfpc2023

[<Struct>]
type PointD =
    | PointD of double * double
    member this.X = match this with PointD(x, _) -> x
    member this.Y = match this with PointD(_, y) -> y


[<Struct>]
type Stadium =
    {
        Center1: PointD
        Center2: PointD
        Radius: double
    }
    member this.Contains(p: PointD): bool =
        let r_squared = this.Radius * this.Radius

        let dx = p.X - this.Center1.X
        let dy = p.Y - this.Center1.Y
        let r1 = dx * dx + dy * dy
        if r1 <= r_squared then true
        else

        let dx = p.X - this.Center2.X
        let dy = p.Y - this.Center2.Y
        let r2 = dx * dx + dy * dy

        if r2 <= r_squared then true
        else

        let (PointD(x0, y0)) = p
        let (PointD(x1, y1)) = this.Center1
        let (PointD(x2, y2)) = this.Center2

        let d = abs((x2 - x1) * (y1 - y0) - (x1 - x0) * (y2 - y1)) / sqrt((x2 - x1) ** 2.0 + (y2 - y1) ** 2)
        d <= this.Radius
