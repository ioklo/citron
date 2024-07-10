# CMakeList.txt : CMake project for Infra, include source and define
# project specific logic here.

# Add source to this project's executable.
add_executable(SyntaxPrinter 
  pch.h pch.cpp 
  SyntaxPrinter.cpp
)

target_include_directories(SyntaxPrinter PRIVATE ../../include)

target_compile_definitions(SyntaxPrinter
  PUBLIC
    $<$<STREQUAL:$<TARGET_PROPERTY:Infra,TYPE>,SHARED_LIBRARY>:INFRA_IMPORT>
    $<$<STREQUAL:$<TARGET_PROPERTY:Syntax,TYPE>,SHARED_LIBRARY>:SYNTAX_IMPORT>
    $<$<STREQUAL:$<TARGET_PROPERTY:TextAnalysis,TYPE>,SHARED_LIBRARY>:TEXTANALYSIS_IMPORT>
)
target_link_libraries(SyntaxPrinter PRIVATE Infra Syntax TextAnalysis)
