# CMakeList.txt : CMake project for Infra, include source and define
# project specific logic here.

# Add source to this project's executable.
add_executable(TestGenerator 
  TestGenerator.cpp
)

find_package(fmt CONFIG REQUIRED)
find_package(Boost REQUIRED)

target_link_libraries(TestGenerator fmt::fmt)
