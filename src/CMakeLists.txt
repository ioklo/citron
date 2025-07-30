cmake_minimum_required(VERSION 3.31)

project(Citron CXX)

set(CMAKE_CXX_STANDARD 23)

add_subdirectory(Infra)
add_subdirectory(Syntax)
add_subdirectory(Logging)
add_subdirectory(Symbol)
add_subdirectory(TextAnalysis)
add_subdirectory(Tools)

# for tests
include(FetchContent)
FetchContent_Declare(
        googletest
        URL https://github.com/google/googletest/archive/03597a01ee50ed33e9dfd640b249b4be3799d395.zip
)
# For Windows: Prevent overriding the parent project's compiler/linker settings
set(gtest_force_shared_crt ON CACHE BOOL "" FORCE)
FetchContent_MakeAvailable(googletest)

add_subdirectory(TextAnalysis.Tests)

