program main
  use domain
  implicit none
  type(room_t) :: room
  character(len=1024) :: filename
  filename = "18.ini"
  call room%load(filename)
  call room%print()
  print *, room%score()
end program main
