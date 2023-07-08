program main
  use domain
  use guess_mod
  implicit none
  type(room_t) :: room
  character(len=1024) :: filename
  filename = "22.ini"
  call room%load(filename)
  call room%print()
  print *, room%score()
  call guess_v1(room)
end program main
