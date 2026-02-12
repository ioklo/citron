
add_executable(TestGenerator)

find_package(Boost REQUIRED COMPONENTS algorithm)

target_link_libraries(TestGenerator PRIVATE
        Common
        Boost::algorithm
)

include(CMakeSources.g.txt)