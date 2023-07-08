program main
  use domain
  implicit none
  type(room_t) :: room
  character(len=1024) :: filename
  filename = "test.txt"
  call room%load(filename)
  call room%print()
end program main
