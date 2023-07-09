program main
  use domain
  use guess_mod
  use reduce_mod
  implicit none
  type(room_t) :: room, vroom
  character(len=1024) :: filename
  real(8) :: e1, e2, etmp
  call get_command_argument(1, filename)
  call room%load(filename)
  call vroom%load(filename)
  call room%print()
  e1 = room%score()
  call reduce_attendees(vroom, 4._8)
  print *, vroom%N_attendees
  call guess_v1(vroom)
  etmp = vroom%score()
  room%musicians = vroom%musicians
  e2 = room%score()
  print *, "Old: ", e1
  print *, "Tmp: ", etmp
  print *, "New: ", e2
  print "(A,F12.4,A,F5.2,A)", "Improvement: ", e2-e1, "; ", (e2-e1)/abs(e1)*100, "%"
  if (e2 > e1) then
    call room%dump(trim(filename)//".new", "FoxtranForV2")
  end if
end program main
