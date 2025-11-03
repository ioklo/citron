cmake_minimum_required(VERSION 3.31)

project(Citron CXX)

set(CMAKE_CXX_STANDARD 23)

add_subdirectory(Infra)
add_subdirectory(Syntax)
add_subdirectory(Logging)
add_subdirectory(ESymbol)
add_subdirectory(RSymbol)
add_subdirectory(NSymbol)
add_subdirectory(MIR)
add_subdirectory(QIR)
add_subdirectory(TextAnalysis)
add_subdirectory(SyntaxIR0Translator)
add_subdirectory(IR0IR1Translator)
add_subdirectory(QEvaluator)

add_subdirectory(Builder)
add_subdirectory(Tools)

include(CTest)
enable_testing()

add_subdirectory(TextAnalysis.Tests)
add_subdirectory(Builder.Tests)
add_subdirectory(QEvaluator.Tests)

