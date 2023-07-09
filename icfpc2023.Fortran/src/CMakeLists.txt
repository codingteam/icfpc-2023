set(dir "${CMAKE_CURRENT_SOURCE_DIR}")

list(APPEND SOURCES
  ${dir}/main.f90
  ${dir}/Vec2D.f90
  ${dir}/domain.f90
  ${dir}/line.f90
  ${dir}/guess.f90
  ${dir}/reduce.f90
)

set(SOURCES ${SOURCES} PARENT_SCOPE)
