module line_mod
  use Vec2D_mod
  implicit none
  type, public :: line_t
    real(8) :: a, b, c
    type(Vec2D_t) :: P1, P2
  contains
    procedure, pass(this) :: new => line_new
    procedure, pass(this) :: distanceTo => line_distanceTo
  end type
contains
  subroutine line_new(this, P1, P2)
    class(line_t), intent(inout) :: this
    type(Vec2D_t), intent(in) :: P1, P2
    this%P1 = P1
    this%P2 = P2
    this%a = P1%y - P2%y
    this%b = P2%x - P1%x
    this%c = - this%a * P2%x - this%b * P2%y
  end subroutine line_new
  real(8) function line_distanceTo(this, P) result(dist)
    class(line_t), intent(inout) :: this
    type(Vec2D_t), intent(in) :: P
    type(Vec2D_t) :: P_line
    P_line = Vec2D_t((this%b*( this%b*P%X - this%a*P%Y) - this%a*this%c) / (this%a*this%a + this%b*this%b), &
                     (this%a*(-this%b*P%X + this%a*P%Y) - this%b*this%c) / (this%a*this%a + this%b*this%b))
    if (this%P1%x > P%X .and. this%P2%x > P%X) then
      dist = 1e6_8
    else if (this%P1%x < P%X .and. this%P2%x < P%X) then
      dist = 1e6_8
    else if (this%P1%y > P%Y .and. this%P2%y > P%Y) then
      dist = 1e6_8
    else if (this%P1%y < P%Y .and. this%P2%y < P%Y) then
      dist = 1e6_8
    else
      dist = P%DistanceTo(P_line)
    end if
  end function line_distanceTo
end module line_mod
