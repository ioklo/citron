
add_executable(QEvaluator.Tests)

target_compile_options(QEvaluator.Tests PRIVATE
        -Wno-unqualified-std-cast-call
)

target_include_directories(QEvaluator.Tests
        PRIVATE
          ../../externals/utfcpp/source
        PUBLIC Public)

target_link_libraries(QEvaluator.Tests PRIVATE Infra NSymbol QEvaluator QIR RSymbol
        GTest::gtest_main
        )

target_sources(QEvaluator.Tests
        PRIVATE
        test.cpp

        PUBLIC FILE_SET headers
        TYPE HEADERS
        FILES
)

find_package(GTest CONFIG REQUIRED)

include(GoogleTest)
gtest_discover_tests(QEvaluator.Tests)