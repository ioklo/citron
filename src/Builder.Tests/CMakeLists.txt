
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

include(CMakeSources.g.txt)

find_package(GTest CONFIG REQUIRED)

include(GoogleTest)
gtest_discover_tests(Builder.Tests)