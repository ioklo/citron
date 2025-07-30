
add_executable(TestGenerator)

target_compile_options(TestGenerator PRIVATE
        -Wno-unqualified-std-cast-call
)

find_package(Boost REQUIRED)
find_package(fmt REQUIRED)

target_include_directories(TestGenerator PRIVATE ${Boost_INCLUDE_DIRS})

target_link_libraries(TestGenerator PRIVATE fmt::fmt)

target_sources(TestGenerator
        PRIVATE
        TestGenerator.cpp
)