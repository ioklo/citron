cmake_minimum_required(VERSION 3.31)

project(Citron CXX)

set(CMAKE_CXX_STANDARD 23)

add_subdirectory(Infra)
add_subdirectory(Logging)
add_subdirectory(Syntax)
add_subdirectory(Symbol)

add_subdirectory(Tools)


