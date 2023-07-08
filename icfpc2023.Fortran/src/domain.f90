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
    integer(8) :: instrument
  end type

  type, public :: pillar_t
    type(Vec2D_t) :: pos
    real(8) :: radius
  end type

  type, public :: room_t
    real(8) :: room_height = 0, room_width = 0
    real(8) :: stage_height = 0, stage_width = 0
    type(Vec2D_t) :: stage_bottom_left
    integer :: version = 0
    integer :: N_musicians = 0
    integer :: N_attendees = 0
    integer :: N_pillars = 0
    type(musician_t), allocatable :: musicians(:)
    type(attendee_t), allocatable :: attendees(:)
    type(pillar_t), allocatable :: pillars(:)
  contains
    procedure, pass(this) :: load => room_load
    procedure, pass(this) :: print => room_print
    procedure, pass(this) :: score => room_score
    procedure, pass(this) :: build_taste_matrix => room_build_taste_matrix
  end type

contains
  subroutine room_load(this, filename)
    class(room_t), intent(inout) :: this
    character(len=*), intent(in) :: filename
    character(len=100) :: line
    integer :: LU
    integer :: input_version
    integer :: i, status, max_instrument = 0
    ! Open the file
    open(newunit=LU, file=filename, status='old', action='read', iostat=status)

    if (status /= 0) then
      write(*, *) 'Filename does not exist: ' // trim(filename)
      stop
    end if

    ! Read the file line by line until the end-of-file is reached
    do
      read(LU, '(A)', iostat=status) line

      if (status /= 0) then
        exit
      else if (index(line, "[version]") /= 0) then
        read(LU, *) this%version
      else if (index(line, "[room_height]") /= 0) then
        read(LU, *) this%room_height
      else if (index(line, "[room_width]") /= 0) then
        read(LU, *) this%room_width
      else if (index(line, "[stage_height]") /= 0) then
        read(LU, *) this%stage_height
      else if (index(line, "[stage_width]") /= 0) then
        read(LU, *) this%stage_width
      else if (index(line, "[stage_bottom_left]") /= 0) then
        read(LU, *) this%stage_bottom_left
      else if (index(line, "[musicians]") /= 0) then
        read(LU, *) this%N_musicians
        allocate(this%musicians(this%N_musicians))
        do i = 1, this%N_musicians
          read(LU, *) this%musicians(i)%pos%x, this%musicians(i)%pos%y, this%musicians(i)%instrument
        end do
        do i = 1, this%N_musicians
          if (this%musicians(i)%instrument > max_instrument) then
            max_instrument = this%musicians(i)%instrument
          end if
        end do
      else if (index(line, "[attendees]") /= 0) then
        read(LU, *) this%N_attendees
        allocate(this%attendees(this%N_attendees))
        do i = 1, this%N_attendees
          allocate(this%attendees(i)%tastes(0:max_instrument))
          read(LU, *) this%attendees(i)%pos%x, this%attendees(i)%pos%y, this%attendees(i)%tastes(:)
        end do
      else if (index(line, "[pillars]") /= 0) then
        read(LU, *) this%N_pillars
        allocate(this%pillars(this%N_pillars))
        do i = 1, this%N_pillars
          read(LU, *) this%pillars(i)%pos%x, this%pillars(i)%pos%y, this%pillars(i)%radius
        end do
      end if
    end do

    ! Close the file
    close(LU)
  end subroutine room_load

  subroutine room_print(this)
    class(room_t), intent(in) :: this
    print *, "Version:           ", this%version
    print *, "Room:              ", this%room_height, this%room_width
    print *, "Stage pos:         ", this%stage_bottom_left
    print *, "Stage:             ", this%stage_height, this%stage_width
    print *, "Parameters(M/N/P): ", this%N_musicians, this%N_attendees, this%N_pillars
  end subroutine room_print

  function room_build_taste_matrix(this) result(Tma)
    class(room_t), intent(in) :: this
    real(8), allocatable :: Tma(:,:) ! Taste matrix
    integer :: i, j
    allocate(Tma(this%N_musicians, this%N_attendees))
    forall ( i = 1:this%N_attendees, j = 1:this%N_musicians)
      Tma(j, i) = this%attendees(i)%tastes(this%musicians(j)%instrument)
    end forall
  end function room_build_taste_matrix

  real(8) function room_score(this) result(energy)
    class(room_t), intent(in) :: this
    integer :: i, j
    real(8), allocatable :: Tma(:,:)
    Tma = this%build_taste_matrix()
    energy = 0.0
    do i = 1, this%N_attendees
      energy = energy
    end do
  end function room_score
end module domain
