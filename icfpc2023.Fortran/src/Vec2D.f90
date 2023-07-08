module Vec2D_mod
  implicit none
  private
  type, public :: Vec2D_t
    real(8) :: x = 0._8, y = 0._8
  contains
    procedure, pass(this) :: vec2D_newFloat
    procedure, pass(this) :: add => Vec2D_add
    procedure, pass(this) :: rescale => Vec2D_rescale
    procedure, pass(this) :: rescaleVec => Vec2D_rescaleVec
    procedure, pass(this) :: SquaredDistanceTo => Vec2D_SquaredDistanceTo
    procedure, pass(this) :: DistanceTo => Vec2D_DistanceTo
    generic, public :: new => vec2D_newFloat
    generic, public :: operator(+) => add
    generic, public :: operator(*) => rescale, rescaleVec
  end type
contains
  pure subroutine vec2D_newFloat(this, x, y)
    class(Vec2D_t), intent(inout) :: this
    real(8), intent(in) :: x, y
    this%x = x
    this%y = y
  end subroutine vec2D_newFloat
  pure function Vec2D_add(this, vec) result(res)
    class(Vec2D_t), intent(in) :: this, vec
    type(Vec2D_t) :: res
    res%x = this%x + vec%x
    res%y = this%y + vec%y
  end function Vec2D_add
  pure function Vec2D_rescale(this, scale) result(res)
    class(Vec2D_t), intent(in) :: this
    real(8), intent(in) :: scale
    type(Vec2D_t) :: res
    res%x = this%x * scale
    res%y = this%y * scale
  end function Vec2D_rescale
  pure function Vec2D_rescaleVec(this, scale) result(res)
    class(Vec2D_t), intent(in) :: this, scale
    type(Vec2D_t) :: res
    res%x = this%x * scale%x
    res%y = this%y * scale%y
  end function Vec2D_rescaleVec
  pure real(8) function Vec2D_SquaredDistanceTo(this, point) result(res)
    class(Vec2D_t), intent(in) :: this, point
    res = (this%x - point%x) * (this%x - point%x) + &
          (this%y - point%y) * (this%y - point%y)
  end function Vec2D_SquaredDistanceTo
  pure real(8) function Vec2D_DistanceTo(this, point) result(res)
    class(Vec2D_t), intent(in) :: this, point
    res = sqrt((this%x - point%x) * (this%x - point%x) + &
               (this%y - point%y) * (this%y - point%y))
  end function Vec2D_DistanceTo
end module Vec2D_mod
