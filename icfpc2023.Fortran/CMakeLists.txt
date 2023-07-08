cmake_minimum_required(VERSION 3.22)

project(ICFPC2023
  VERSION 0.0.0
  LANGUAGES C CXX Fortran)

set(CMAKE_Fortran_STANDARD 2003)

set(PN ${PROJECT_NAME})

include(CheckIPOSupported)
check_ipo_supported(RESULT IPO_AVAILABLE)

set(SOURCES)
add_subdirectory(src)

add_executable(${PN} ${SOURCES})
if(IPO_AVAILABLE)
  set_target_properties(${PN} PROPERTIES INTERPROCEDURAL_OPTIMIZATION TRUE)
endif()
