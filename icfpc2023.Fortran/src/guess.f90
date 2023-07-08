module guess_mod
  use domain
  implicit none
contains
  subroutine guess_v1(room)
    type(room_t), intent(inout) :: room
    integer :: t, i, j, instr
    real(8), allocatable :: Pot(:,:,:)
    integer(8), allocatable :: order(:)
    type(musician_t) :: musician
    integer(8) :: max_pos(3)
    allocate(Pot(room%N_instruments, int(room%stage_width, 8) - 20 + 1, int(room%stage_height, 8) - 20 + 1))
    allocate(order(room%N_musicians))
    do i = 1, room%N_musicians
      room%musicians(i)%pos%x = -2000
      order(i) = room%musicians(i)%instrument
    end do
    do t = 1, room%N_musicians
      do i = 1, int(room%stage_height, 8) - 20 + 1
        do j = 1, int(room%stage_width, 8) - 20 + 1
          musician%pos%x = j + 9
          musician%pos%y = i + 9
          musician%pos = musician%pos + room%stage_bottom_left
          do instr = 1, room%N_instruments
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
    end do
    do t = 1, room%N_musicians
      print *, room%musicians(t)%pos%x, room%musicians(t)%pos%y
    end do
  end subroutine guess_v1
end module guess_mod