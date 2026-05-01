cmake_minimum_required(VERSION 3.31)

project(Citron)

set(CMAKE_CXX_STANDARD 23)

# 프로젝트 전체에 쓰일 option용 pseudo library
add_library(Common INTERFACE)

target_compile_options(Common INTERFACE $<$<CXX_COMPILER_ID:Clang>:-Wno-unqualified-std-cast-call>)
target_compile_options(Common INTERFACE $<$<CXX_COMPILER_ID:AppleClang>:-Wno-unqualified-std-cast-call>)
target_compile_options(Common INTERFACE $<$<CXX_COMPILER_ID:MSVC>:/utf-8>)
target_compile_options(Common INTERFACE $<$<CXX_COMPILER_ID:MSVC>:/Zc:__cplusplus>)

add_subdirectory(Infra)
add_subdirectory(Syntax)
add_subdirectory(Logging)
add_subdirectory(ESymbol)
add_subdirectory(RSymbol)
add_subdirectory(NSymbol)
add_subdirectory(MIR)
add_subdirectory(QIR)
add_subdirectory(TextAnalysis)
add_subdirectory(SmTranslator)
add_subdirectory(MqTranslator)
add_subdirectory(QlTranslator)
add_subdirectory(QEvaluator)

add_subdirectory(Builder)
add_subdirectory(Tools)
add_subdirectory(RuntimeLibrary)
add_subdirectory(TestConsole)

include(CTest)
enable_testing()

add_subdirectory(TextAnalysis.Tests)
add_subdirectory(Builder.Tests)
add_subdirectory(QEvaluator.Tests)
add_subdirectory(EvalTests)
