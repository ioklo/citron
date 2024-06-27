# CMakeList.txt : CMake project for Infra, include source and define
# project specific logic here.
#

# Add source to this project's executable.
add_library(Syntax SHARED
  pch.cpp pch.h
  Syntaxes.cpp
  Syntaxes.g.cpp

  ../../include/Syntax/SyntaxConfig.h
  ../../include/Syntax/Tokens.h
  ../../include/Syntax/Syntaxes.g.h
  ../../include/Syntax/Syntax.h
)

# CodeGeneration <- Syntax
add_dependencies(Syntax CodeGeneration)

# TODO: Add tests and install targets if needed.
target_include_directories (Syntax PUBLIC ../../include)
target_compile_definitions(Syntax PRIVATE SYNTAX_EXPORT)
target_link_libraries(Syntax PUBLIC Infra)

target_compile_features(Syntax PRIVATE cxx_std_20)

target_compile_options(Syntax PRIVATE "$<$<C_COMPILER_ID:MSVC>:/utf-8>")
target_compile_options(Syntax PRIVATE "$<$<CXX_COMPILER_ID:MSVC>:/utf-8>")
