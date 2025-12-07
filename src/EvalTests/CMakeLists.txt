
add_executable(EvalTests)

target_include_directories(EvalTests
        PRIVATE
          ../../externals/utfcpp/source
        PUBLIC Public)

target_link_libraries(EvalTests PRIVATE
        Common
        Infra
        Logging
        Syntax
        ESymbol
        RSymbol
        NSymbol
        MIR
        QIR
        TextAnalysis
        SyntaxIR0Translator
        IR0IR1Translator
        QEvaluator
        GTest::gtest_main
)
target_sources(EvalTests
        PRIVATE
        EvalTests.g.cpp

        PUBLIC FILE_SET headers
        TYPE HEADERS
        FILES
)

find_package(GTest CONFIG REQUIRED)

include(GoogleTest)
gtest_discover_tests(EvalTests)