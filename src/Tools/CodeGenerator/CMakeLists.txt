
add_executable(CodeGenerator)

target_compile_options(CodeGenerator PRIVATE
        -Wno-unqualified-std-cast-call
)

find_package(fmt REQUIRED)

target_sources(CodeGenerator
        PRIVATE
        CodeGenerator.cpp
        Misc.cpp
        SyntaxCodeGenerator.cpp

        PUBLIC FILE_SET cxx_modules
        TYPE CXX_MODULES
        FILES
        Misc.ixx
        SyntaxCodeGenerator.ixx
)

add_custom_command(
  OUTPUT
    ${CMAKE_CURRENT_SOURCE_DIR}/../Syntax/Syntaxes.g.cpp
    ${CMAKE_CURRENT_SOURCE_DIR}/../Syntax/Syntaxes.g.ixx
  COMMAND
    CodeGenerator
  ARGS
    ../..
  DEPENDS CodeGenerator
  WORKING_DIRECTORY
    ${CMAKE_CURRENT_SOURCE_DIR}

  COMMENT "Generating Syntaxes.g.cpp and Syntaxes.g.ixx"

  VERBATIM
)

target_link_libraries(CodeGenerator PRIVATE fmt::fmt)