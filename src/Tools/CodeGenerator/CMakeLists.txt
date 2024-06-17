# CMakeList.txt : CMake project for Infra, include source and define
# project specific logic here.
#

# Add source to this project's executable.
add_executable(CodeGenerator 
  CodeGenerator.cpp
  Misc.cpp
  Misc.h
  SymbolCodeGenerator.cpp
  SymbolCodeGenerator.h
  SyntaxCodeGenerator.cpp
  SyntaxCodeGenerator.h
)

target_compile_features(CodeGenerator PRIVATE cxx_std_20)

find_package(fmt CONFIG REQUIRED)
target_link_libraries(CodeGenerator fmt::fmt)

# TODO: Add tests and install targets if needed.
target_compile_options(CodeGenerator PRIVATE "$<$<C_COMPILER_ID:MSVC>:/utf-8>")
target_compile_options(CodeGenerator PRIVATE "$<$<CXX_COMPILER_ID:MSVC>:/utf-8>")

add_custom_command(
  TARGET CodeGenerator POST_BUILD
  COMMAND CodeGenerator ${CMAKE_CURRENT_SOURCE_DIR}/../..
)