
add_executable(TestGenerator)

find_package(Boost REQUIRED COMPONENTS algorithm)
find_package(fmt CONFIG REQUIRED)

target_link_libraries(TestGenerator PRIVATE
        Common
        Boost::algorithm
        fmt::fmt
)

target_sources(TestGenerator
        PRIVATE
        TestGenerator.cpp
)
