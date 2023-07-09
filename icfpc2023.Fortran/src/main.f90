program main
  use domain
  use guess_mod
  use reduce_mod
  implicit none
  type(room_t) :: room, vroom
  character(len=1024) :: filename
  real(8) :: e1, e2
  call get_command_argument(1, filename)
  call room%load(filename)
  call room%print()
  e1 = room%score()
  call guess_v2(room)
  e2 = room%score()
  print *, "Old: ", e1
  print *, "New: ", e2
  print "(A,F15.2,A,F8.2,A)", "Improvement: ", e2-e1, "; ", (e2-e1)/abs(e1)*100, "%"
  if (e2 > e1) then
    call room%dump(trim(filename)//".new", "FoxtranForV2")
  end if
end program main
