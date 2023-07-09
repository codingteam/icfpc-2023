namespace Icfpc2023

open System

[<Struct>]
type PointD =
    | PointD of double * double
    member this.X = match this with PointD(x, _) -> x
    member this.Y = match this with PointD(_, y) -> y
    member this.SquaredDistanceTo(p: PointD): double =
        let (PointD(x1, y1)) = this
        let (PointD(x2, y2)) = p
        (x1 - x2) ** 2.0 + (y1 - y2) ** 2.0
    member this.DistanceTo(p: PointD): double =
        sqrt <| this.SquaredDistanceTo p
    static member (+) (a: PointD, b: PointD) = PointD(a.X + b.X, a.Y + b.Y)
    static member (-) (a: PointD, b: PointD) = PointD(a.X - b.X, a.Y - b.Y)
    static member (*) (p: PointD, k: double) = PointD(p.X * k, p.Y * k)
    static member (/) (p: PointD, k: double) = PointD(p.X / k, p.Y / k)
    static member (*) (p1: PointD, p2: PointD) = PointD(p1.X * p2.X, p1.Y * p2.Y)
    static member (~-) (a: PointD) = PointD(-a.X, -a.Y)

[<Struct>]
type Line =
    {
        End1: PointD
        End2: PointD
    }
    member this.DistanceTo(p: PointD): double =
        let (PointD(x0, y0)) = p
        let (PointD(x1, y1)) = this.End1
        let (PointD(x2, y2)) = this.End2

        abs((x2 - x1) * (y1 - y0) - (x1 - x0) * (y2 - y1)) / sqrt((x2 - x1) ** 2.0 + (y2 - y1) ** 2)

#nowarn "3391"

[<Struct>]
type Rectangle =
    {
        BottomLeft: PointD
        Height: double
        Width: double
    }
    member private this.UpperRight: PointD =
        this.BottomLeft + PointD(this.Width, this.Height)

    member this.Contains(p: PointD): bool =
        let (PointD(x, y)) = p
        let (PointD(x0, y0)) = this.BottomLeft
        let (PointD(x1, y1)) = this.UpperRight
        (x >= x0 && x <= x1) && (y >= y0 && y <= y1)

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
        if r1 < r_squared then true
        else

        let dx = p.X - this.Center2.X
        let dy = p.Y - this.Center2.Y
        let r2 = dx * dx + dy * dy

        if r2 < r_squared then true
        else

        let (PointD(x0, y0)) = p
        let (PointD(x1, y1)) = this.Center1
        let (PointD(x2, y2)) = this.Center2

        let line = { End1 = this.Center1; End2 = this.Center2 }
        if line.DistanceTo(p) >= this.Radius then false
        else

        // Check that the point lies in the neighbourhood of the segment between the two centres. Two cases here, depending on whether the segment is vertical or not

        let closest =
            if x1 = x2
            then PointD(x1, y0)
            else // the segment is not vertical

                // Calculate slope and intercept of line between the two centres:
                // y = mx + b
                let m = (y2 - y1) / (x2 - x1)
                let b = y1 - m*x1

                // The point on the line that's closest to p
                // ax + by + c = 0    ===   mx - y + b = 0
                let closest_x = (x0 + m*y0 - m*b) / (m ** 2.0 + 1.0)
                let closest_y = (m*(x0 + m*y0) + b) / (m ** 2.0 + 1.0)
                PointD(closest_x, closest_y)

        // Now check if the closest point is between the two centres, i.e. it
        // lies on the segment between them. By construction, it lies on the
        // line, so it's sufficient to check that the distance to either end
        // doesn't exceed the length of the segment
        let segment_length = this.Center1.DistanceTo(this.Center2)
        closest.DistanceTo(this.Center1) < segment_length && closest.DistanceTo(this.Center2) < segment_length

    static member NormalizeVector(v: PointD): PointD =
        let (PointD(x, y)) = v
        let length = sqrt(x ** 2.0 + y ** 2.0)
        PointD(x / length, y / length)

    static member RotateVector (radians: double) (v: PointD): PointD =
        let (PointD(x, y)) = v
        let cos = cos radians
        let sin = sin radians
        PointD(x * cos - y * sin, x * sin + y * cos)

    // https://stackoverflow.com/a/18292964/2684760
    static member LineIntersectsRect(struct(a, b), rect: Rectangle): bool =
        let (PointD(x1, y1)) = a
        let (PointD(x2, y2)) = b

        let minX = rect.BottomLeft.X
        let maxX = rect.BottomLeft.X + rect.Width
        let minY = rect.BottomLeft.Y
        let maxY = rect.BottomLeft.Y + rect.Height

        let k = (y2 - y1) / (x2 - x1)
        let y = k * (minX - x1) + y1
        if y > minY && y < maxY then true
        else
            let y = k * (maxX - x1) + y1
            if y > minY && y < maxY then true
            else
                let x = (minY - y1) / k + x1
                if x > minX && x < maxX then true
                else
                    let x = (maxY - y1) / k + x1
                    if x > minX && x < maxX then true
                    else false

    member this.RectangularPartIntersectsWith(rect: Rectangle): bool =
        let radiusVector = Stadium.NormalizeVector(this.Center2 - this.Center1) * this.Radius
        let line1 = struct (
            this.Center1 + Stadium.RotateVector(Math.PI / 2.0) radiusVector,
            this.Center2 + Stadium.RotateVector(Math.PI / 2.0) radiusVector
        )
        let line2 = struct (
            this.Center1 - Stadium.RotateVector(Math.PI / 2.0) radiusVector,
            this.Center2 - Stadium.RotateVector(Math.PI / 2.0) radiusVector
        )
        Stadium.LineIntersectsRect(line1, rect) || Stadium.LineIntersectsRect(line2, rect)
