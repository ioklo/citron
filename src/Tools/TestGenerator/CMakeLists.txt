
add_executable(TestGenerator)

find_package(Boost REQUIRED COMPONENTS algorithm)
find_package(fmt CONFIG REQUIRED)

target_link_libraries(TestGenerator PRIVATE
        Common
        Boost::algorithm
        fmt::fmt
)

include(CMakeSources.g.txt)