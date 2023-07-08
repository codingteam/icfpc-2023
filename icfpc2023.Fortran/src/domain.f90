module domain
  use Vec2D_mod
  implicit none
  private

  type, public :: attendee_t
    type(Vec2D_t) :: pos
    real(8), allocatable :: tastes(:)
  end type

  type, public :: musician_t
    type(Vec2D_t) :: pos
    integer :: instrument
  end type

  type, public :: pillar_t
    type(Vec2D_t) :: pos
    real(8) :: radius
  end type

  type, public :: room_t
    real(8) :: room_height, room_width
    real(8) :: stage_height, stage_width
    type(Vec2D_t) :: stage_bottom_left
    type(musician_t), allocatable :: musicians(:)
    type(attendee_t), allocatable :: attendees(:)
    type(pillar_t), allocatable :: pillars(:)
  end type

contains
end module domain
