cmake_minimum_required(VERSION 3.19)

set(CMAKE_CXX_STANDARD 17)

enable_testing()

project(Citron)

if (MSVC)
    # warning level 4
    add_compile_options(/utf-8 /Zc:__cplusplus)
else()

endif()

add_subdirectory(src)