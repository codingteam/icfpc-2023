program main
  use domain
  use guess_mod
  implicit none
  type(room_t) :: room
  character(len=1024) :: filename
  real(8) :: e1, e2
  filename = "23.ini"
  call room%load(filename)
  call room%print()
  e1 = room%score()
  call guess_v1(room)
  e2 = room%score()
  print *, "Old: ", e1
  print *, "New: ", e2
  print "(A,F12.4,A,F5.2,A)", "Improvement: ", e2-e1, "; ", (e2-e1)/abs(e1)*100, "%"
  if (e2 > e1) then
    call room%dump(trim(filename)//".new")
  end if
end program main
