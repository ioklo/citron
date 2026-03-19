cmake_minimum_required(VERSION 3.31)

project(CitronPreprocessor)

set(CMAKE_CXX_STANDARD 23)

# 프로젝트 전체에 쓰일 option용 pseudo library
include(../Common.cmake)

add_subdirectory(CMakeSourcesGenerator)
add_subdirectory(CodeGenerator)
add_subdirectory(DocUpdater)
add_subdirectory(TestGenerator)
