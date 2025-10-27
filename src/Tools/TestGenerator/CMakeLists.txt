
add_executable(TestGenerator)

target_compile_options(TestGenerator PRIVATE
        -Wno-unqualified-std-cast-call
)

find_package(Boost REQUIRED COMPONENTS algorithm)
find_package(fmt CONFIG REQUIRED)

target_link_libraries(TestGenerator PRIVATE Boost::algorithm)
target_link_libraries(TestGenerator PRIVATE fmt::fmt)

target_sources(TestGenerator
        PRIVATE
        TestGenerator.cpp
)