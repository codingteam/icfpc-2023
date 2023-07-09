module grid_mod
  use Vec2D_mod
  implicit none
  type grid_t
    type(Vec2D_t), allocatable :: pos(:)
    real(8), allocatable :: value(:)
    logical, allocatable :: skip(:)
  contains
    procedure, pass(this) :: alloc
    procedure, pass(this) :: merge
    procedure, pass(this) :: drop
    procedure, pass(this) :: dealloc
    procedure, pass(this) :: exclude
    procedure, pass(grid) :: generate_grid
    procedure, pass(grid) :: generate_radial_grid
  end type
contains
  subroutine alloc(this, N)
    class(grid_t), intent(inout) :: this
    integer(8), intent(in) :: N
    if (allocated(this%pos)) deallocate(this%pos)
    if (allocated(this%value)) deallocate(this%value)
    if (allocated(this%skip)) deallocate(this%skip)
    allocate(this%pos(N))
    allocate(this%value(N), source = 0._8)
    allocate(this%skip(N), source = .false.)
  end subroutine alloc
  subroutine merge(this, grid)
    class(grid_t), intent(inout) :: this
    type(grid_t), intent(in) :: grid
    type(grid_t) :: tmp
    integer(8) :: N_points, i, m
    tmp = this
    N_points = count(.not.tmp%skip) + count(.not.grid%skip)
    call this%alloc(N_points)
    m = 0
    do i = 1, size(tmp%skip)
      if (.not.tmp%skip(i)) then
        m = m + 1
        this%pos(m) = tmp%pos(i)
        this%value(m) = tmp%value(i)
      end if
    end do
    do i = 1, size(grid%skip)
      if (.not.grid%skip(i)) then
        m = m + 1
        this%pos(m) = grid%pos(i)
        this%value(m) = grid%value(i)
      end if
    end do
  end subroutine merge
  subroutine drop(this)
    class(grid_t), intent(inout) :: this
    type(grid_t) :: tmp
    integer(8) :: N_points, i, m
    tmp = this
    N_points = count(.not.tmp%skip)
    call this%alloc(N_points)
    m = 0
    do i = 1, size(tmp%skip)
      if (.not.tmp%skip(i)) then
        m = m + 1
        this%pos(m) = tmp%pos(i)
        this%value(m) = tmp%value(i)
      end if
    end do
  end subroutine drop
  subroutine dealloc(this)
    class(grid_t), intent(inout) :: this
    if (allocated(this%pos)) deallocate(this%pos)
    if (allocated(this%value)) deallocate(this%value)
    if (allocated(this%skip)) deallocate(this%skip)
  end subroutine dealloc
  subroutine exclude(this, point, value)
    class(grid_t), intent(inout) :: this
    type(Vec2D_t), intent(in) :: point
    real(8), intent(in) :: value
    integer :: i
    do i = 1, size(this%skip)
      if (point%SquaredDistanceTo(this%pos(i)) < 100._8) then
        this%value(i) = value
        this%skip(i) = .true.
      end if
    end do
  end subroutine exclude
  subroutine generate_grid(grid, minx, maxx, miny, maxy, value)
    class(grid_t), intent(inout) :: grid
    real(8), intent(in) :: minx, maxx, miny, maxy
    real(8), intent(in) :: value
    integer(8) :: Nx, Ny, N
    integer(8) :: i, j, m
    real(8) :: x, y
    call grid%dealloc
    Nx = 1 + (maxx - minx) / value
    Ny = 1 + (maxy - miny) / value
    N = Nx * Ny
    call grid%alloc(N)
    m = 1
    do i = 1, Nx
      do j = 1, Ny
        x = minx + (i - 1) * value
        y = miny + (j - 1) * value
        grid%pos(m)%x = x
        grid%pos(m)%y = y
        m = m + 1
      end do
    end do
  end subroutine generate_grid
  subroutine generate_radial_grid(grid, minx, maxx, miny, maxy, center, Nrad)
    real(8), parameter :: radial(11) = 10._8 + (/ 1e-7_8, 0.46_8, 0.97_8, 1.55_8, 2.22_8, 3.01_8, 3.98_8, 5.23_8, 6.99_8, 10.0_8, 15.0_8 /)
    real(8), parameter :: PI2 = 4.0_8*atan(1.0_8)*2._8
    class(grid_t), intent(inout) :: grid
    real(8), intent(in) :: minx, maxx, miny, maxy
    type(Vec2D_t), intent(in) :: center
    integer(8), intent(in) :: Nrad
    integer(8) :: irad, iang, m
    real(8) :: x, y
    call grid%dealloc
    m = 0
    do irad = 1, size(radial)
      do iang = 1, Nrad
        x = radial(irad) * sin(PI2*((iang-1)/dble(Nrad)+(irad-1)/dble(2*Nrad))) + center%x
        y = radial(irad) * cos(PI2*((iang-1)/dble(Nrad)+(irad-1)/dble(2*Nrad))) + center%y
        if (minx > x .or. maxx < x) cycle
        if (miny > y .or. maxy < y) cycle
        m = m + 1
      end do
    end do
    call grid%alloc(m)
    m = 1
    do irad = 1, size(radial)
      do iang = 1, Nrad
        x = radial(irad) * sin(PI2*((iang-1)/dble(Nrad)+(irad-1)/dble(2*Nrad))) + center%x
        y = radial(irad) * cos(PI2*((iang-1)/dble(Nrad)+(irad-1)/dble(2*Nrad))) + center%y
        if (minx > x .or. maxx < x) cycle
        if (miny > y .or. maxy < y) cycle
        grid%pos(m)%x = x
        grid%pos(m)%y = y
        m = m + 1
      end do
    end do
  end subroutine generate_radial_grid
end module grid_mod
