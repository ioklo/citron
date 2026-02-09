
add_executable(QEvaluator.Tests)

target_include_directories(QEvaluator.Tests
        PRIVATE
          ../../externals/utfcpp/source
        PUBLIC Public)

target_link_libraries(QEvaluator.Tests PRIVATE
        Common
        Infra
        NSymbol
        QEvaluator
        QIR
        RSymbol
        GTest::gtest_main
)

include(CMakeSources.g.txt)

find_package(GTest CONFIG REQUIRED)

include(GoogleTest)
gtest_discover_tests(QEvaluator.Tests)