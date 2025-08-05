
add_executable(TextAnalysis.Tests)

target_compile_options(TextAnalysis.Tests PRIVATE
        -Wno-unqualified-std-cast-call
)

target_include_directories(TextAnalysis.Tests
        PRIVATE
          ../../externals/utfcpp/source
        PUBLIC Public)

target_link_libraries(TextAnalysis.Tests PRIVATE Infra Syntax TextAnalysis
        GTest::gtest_main
        )

target_sources(TextAnalysis.Tests
        PRIVATE
        CommandLexerTests.cpp
        ExpParserTests.g.cpp
        LexerTests.cpp
        ScriptParserTests.g.cpp
        StmtParserTests.g.cpp
        TestMisc.cpp
        TypeExpParserTests.g.cpp

        PUBLIC FILE_SET headers
        TYPE HEADERS
        FILES
        TestMisc.h
)

find_package(GTest CONFIG REQUIRED)

include(GoogleTest)
gtest_discover_tests(TextAnalysis.Tests)