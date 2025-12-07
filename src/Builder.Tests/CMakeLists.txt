
add_executable(Builder.Tests)

target_link_libraries(Builder.Tests PRIVATE
        Common
        Infra
        Syntax
        TextAnalysis
        SyntaxIR0Translator
        Builder
        GTest::gtest_main
)
target_sources(Builder.Tests
        PRIVATE
        test.cpp
)


find_package(GTest CONFIG REQUIRED)

include(GoogleTest)
gtest_discover_tests(Builder.Tests)