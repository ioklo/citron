
add_executable(Builder.Tests)

target_compile_options(Builder.Tests PRIVATE
        -Wno-unqualified-std-cast-call
)

target_link_libraries(Builder.Tests PRIVATE Infra Syntax TextAnalysis SyntaxIR0Translator Builder
        GTest::gtest_main
        )

target_sources(Builder.Tests
        PRIVATE
        test.cpp
)


find_package(GTest CONFIG REQUIRED)

include(GoogleTest)
gtest_discover_tests(Builder.Tests)