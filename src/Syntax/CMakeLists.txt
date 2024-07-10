# CMakeList.txt : CMake project for Infra, include source and define
# project specific logic here.
#

# Add source to this project's executable.
add_library(Syntax
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
target_include_directories (Syntax PUBLIC ../../include PRIVATE ${CMAKE_CURRENT_BINARY_DIR})

target_compile_definitions(Syntax 
  PRIVATE 
    SYNTAX_EXPORT
  PUBLIC 
    $<$<STREQUAL:$<TARGET_PROPERTY:Infra,TYPE>,SHARED_LIBRARY>:INFRA_IMPORT>
)
target_link_libraries(Syntax PUBLIC Infra)

