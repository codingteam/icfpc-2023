module guess_mod
  use domain
  implicit none
contains
  subroutine guess_v1(room)
    type(room_t), intent(inout) :: room
    integer :: t, i, j, instr
    real(8), allocatable :: Pot(:,:,:)
    integer(8), allocatable :: order(:), order_check(:)
    type(musician_t) :: musician
    integer(8) :: max_pos(3), ord_pos(1)
    allocate(Pot(room%N_instruments, int(room%stage_width, 8) - 20 + 1, int(room%stage_height, 8) - 20 + 1))
    allocate(order(room%N_musicians), order_check(room%N_musicians))
    do i = 1, room%N_musicians
      room%musicians(i)%pos%x = -2000
      order(i) = room%musicians(i)%instrument
      order_check(i) = order(i)
    end do
    do t = 1, room%N_musicians
      do i = 1, int(room%stage_height, 8) - 20 + 1
        do j = 1, int(room%stage_width, 8) - 20 + 1
          musician%pos%x = j + 9
          musician%pos%y = i + 9
          musician%pos = musician%pos + room%stage_bottom_left
          do instr = 1, room%N_instruments
            ord_pos = findloc(order_check, instr)
            if (ord_pos(1) == 0) then
              Pot(instr, j, i) = -1e9_8
              cycle
            end if
            musician%instrument = instr
            room%musicians(t) = musician
            Pot(instr, j, i) = room%score()
          end do
        end do
      end do
      max_pos = maxloc(Pot)
      room%musicians(t)%instrument = max_pos(1)
      room%musicians(t)%pos%x = max_pos(2) + 9
      room%musicians(t)%pos%y = max_pos(3) + 9
      order_check(findloc(order_check, max_pos(1))) = -1
    end do
    call restore_order(room, order)
  end subroutine guess_v1
  subroutine restore_order(room, order)
    type(room_t), intent(inout) :: room
    integer(8) :: order(:)
    type(musician_t) :: musician
    integer :: i, j
    do i = 1, room%N_musicians
      if (room%musicians(i)%instrument == order(i)) cycle
      do j = i + 1, room%N_musicians
        if (room%musicians(j)%instrument == order(i)) then
          musician = room%musicians(i)
          room%musicians(i) = room%musicians(j)
          room%musicians(j) = musician
          exit
        end if
      end do
    end do
  end subroutine restore_order
end module guess_mod
