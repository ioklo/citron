cmake_minimum_required(VERSION 3.31)

project(Citron CXX)

set(CMAKE_CXX_STANDARD 23)

add_subdirectory(Infra)
add_subdirectory(Syntax)
add_subdirectory(Logging)
add_subdirectory(Symbol)
add_subdirectory(TextAnalysis)
add_subdirectory(IR0)
add_subdirectory(IR1)
add_subdirectory(SyntaxIR0Translator)
add_subdirectory(IR0IR1Translator)

add_subdirectory(Builder)
add_subdirectory(Tools)

include(CTest)
enable_testing()

add_subdirectory(TextAnalysis.Tests)
add_subdirectory(Builder.Tests)

