# CMakeList.txt : CMake project for Infra, include source and define
# project specific logic here.

# Add source to this project's executable.
add_executable(SyntaxPrinter 
  pch.h pch.cpp 
  SyntaxPrinter.cpp
)

target_compile_features(SyntaxPrinter PRIVATE cxx_std_20)

target_link_libraries(SyntaxPrinter PRIVATE Infra Syntax TextAnalysis)

target_compile_options(SyntaxPrinter PRIVATE "$<$<C_COMPILER_ID:MSVC>:/utf-8>")
target_compile_options(SyntaxPrinter PRIVATE "$<$<CXX_COMPILER_ID:MSVC>:/utf-8>")

target_include_directories(SyntaxPrinter PRIVATE ../../include)