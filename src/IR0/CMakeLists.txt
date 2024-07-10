# CMakeList.txt : CMake project for Infra, include source and define
# project specific logic here.
#

# Add source to this project's executable.
add_library(IR0
  RArgument.h
  RExp.h
  RLoc.h
  RProgram.h
  RStmt.h

  main.cpp
)

# TODO: Add tests and install targets if needed.
target_include_directories(IR0 PUBLIC ../../include)

target_compile_definitions(IR0 
  PRIVATE 
    IR0_EXPORT
  PUBLIC
    $<$<STREQUAL:$<TARGET_PROPERTY:Infra,TYPE>,SHARED_LIBRARY>:INFRA_IMPORT>
)

target_link_libraries(IR0 PUBLIC Infra)

#get_property(IS_IN_TRY_COMPILE GLOBAL PROPERTY IN_TRY_COMPILE)
#if(NOT IS_IN_TRY_COMPILE)
#  target_compile_options(Symbol PRIVATE "$<$<CXX_COMPILER_ID:Clang>:-fmodule-file=std.pcm>")
#endif()



