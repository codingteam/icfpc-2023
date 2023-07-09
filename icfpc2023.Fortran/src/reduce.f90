module reduce_mod
  use domain
  use line_mod
  implicit none
contains
  subroutine reduce_attendees(room, power_factor)
    type(room_t), intent(inout) :: room
    real(8), intent(in) :: power_factor
    type(attendee_t), allocatable :: new_attendees(:)
    logical, allocatable :: used_attendees(:)
    real(8), allocatable :: Daa(:,:), Das(:)
    logical :: was_merged
    integer :: pos(2), i, j, merged
    was_merged = .true.
    do while(was_merged)
      merged = 0
      was_merged = .false.
      allocate(new_attendees(room%N_attendees))
    allocate(used_attendees(room%N_attendees), source = .false.)
      Daa = room%build_AA_invsquareddistance_matrix() ** (power_factor / 2._8)
      Das = build_AS_invdistance_vector(room)
      do while (.true.)
        pos = maxloc(Daa)
        if (Daa(pos(1), pos(2)) < 1e-2 ** (power_factor / 2._8)) exit
        if (Das(pos(1)) < Daa(pos(1), pos(2)) .and. Das(pos(2)) < Daa(pos(1), pos(2)) .and. &
            sound_transparency(room%attendees(pos(1)), room%attendees(pos(2)), room%pillars) > 0.5_8) then
          was_merged = .true.
          used_attendees(pos(1)) = .true.
          used_attendees(pos(2)) = .true.
          Daa(pos(1), :) = -1
          Daa(pos(2), :) = -1
          Daa(:, pos(1)) = -1
          Daa(:, pos(2)) = -1
          merged = merged + 1
          new_attendees(merged)%pos%x = (room%attendees(pos(1))%pos%x + room%attendees(pos(2))%pos%x) / 2._8
          new_attendees(merged)%pos%y = (room%attendees(pos(1))%pos%y + room%attendees(pos(2))%pos%y) / 2._8
          new_attendees(merged)%tastes = room%attendees(pos(1))%tastes + room%attendees(pos(2))%tastes
        else
          Daa(pos(1), pos(2)) = -1
          Daa(pos(2), pos(1)) = -1
        end if
      end do
      do i = 1, size(used_attendees)
        if (.not.used_attendees(i)) then
          merged = merged + 1
          new_attendees(merged) = room%attendees(i)
        end if
      end do
      room%N_attendees = merged
      deallocate(room%attendees)
      room%attendees = new_attendees(1:merged)
      deallocate(new_attendees, used_attendees)
    end do
  end subroutine reduce_attendees
  function build_AS_invdistance_vector(room) result(Das)
    type(room_t), intent(inout) :: room
    real(8), allocatable :: Das(:)
    integer :: i
    real(8) :: dist(4)
    type(Vec2D_t) :: P(4)
    type(line_t) :: l(4)
    P = room%stage_bottom_left
    P(2)%x = P(2)%x + room%stage_width
    P(3)%x = P(3)%x + room%stage_width
    P(3)%y = P(3)%y + room%stage_height
    P(4)%y = P(4)%y + room%stage_height
    call l(1)%new(P(1),P(2))
    call l(2)%new(P(2),P(3))
    call l(3)%new(P(3),P(4))
    call l(4)%new(P(4),P(1))
    allocate(Das(room%N_attendees))
    do concurrent (i = 1:room%N_attendees)
      dist(1) = l(1)%distanceTo(room%attendees(i)%pos)
      dist(2) = l(2)%distanceTo(room%attendees(i)%pos)
      dist(3) = l(3)%distanceTo(room%attendees(i)%pos)
      dist(4) = l(4)%distanceTo(room%attendees(i)%pos)
      Das(i) = 1._8 / minval(dist)
    end do
  end function build_AS_invdistance_vector
end module reduce_mod