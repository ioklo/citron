
add_executable(TextAnalysis.Tests)

target_include_directories(TextAnalysis.Tests
        PRIVATE
          ../../externals/utfcpp/source
        PUBLIC Public)

target_link_libraries(TextAnalysis.Tests PRIVATE
        Common
        Infra
        Syntax
        TextAnalysis
        GTest::gtest_main
)

include(CMakeSources.g.txt)

find_package(GTest CONFIG REQUIRED)

include(GoogleTest)
gtest_discover_tests(TextAnalysis.Tests)