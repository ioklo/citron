# CMakeList.txt : CMake project for Infra, include source and define
# project specific logic here.

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

# Syntaxes.g.h Syntaxes.g.cpp 를 만드는 방법
add_custom_command(
  OUTPUT
    ${CMAKE_CURRENT_SOURCE_DIR}/../../../include/Syntax/Syntaxes.g.h
    ${CMAKE_CURRENT_SOURCE_DIR}/../../Syntax/Syntaxes.g.cpp
  COMMAND CodeGenerator ${CMAKE_CURRENT_SOURCE_DIR}/../..
  COMMAND ${CMAKE_COMMAND} -E touch CodeGeneration.lastest
  DEPENDS CodeGenerator
  COMMENT "Running generator to produce Syntaxes.g.h, Syntaxes.g.cpp ..."
  BYPRODUCTS CodeGeneration.lastest
  VERBATIM
)

# Syntaxes.g.h와 infra.g.h 파일을 생성하는 타겟 추가
# Syntaxes.g.h Syntaxes.g.cpp <- CodeGeneration
add_custom_target(CodeGeneration
  DEPENDS
    ${CMAKE_CURRENT_SOURCE_DIR}/../../../include/Syntax/Syntaxes.g.h
    ${CMAKE_CURRENT_SOURCE_DIR}/../../Syntax/Syntaxes.g.cpp
)
