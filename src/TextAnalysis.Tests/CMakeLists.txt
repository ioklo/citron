find_package(GTest REQUIRED)

add_executable(TextAnalysis.Tests
    pch.h pch.cpp

    LexerTests.cpp
    CommandLexerTests.cpp

    TestMisc.h TestMisc.cpp

    TypeExpParserTests.g.cpp
    ExpParserTests.g.cpp
    StmtParserTests.g.cpp
    ScriptParserTests.g.cpp
)

target_link_libraries(TextAnalysis.Tests PRIVATE Infra Syntax TextAnalysis GTest::gtest GTest::gtest_main)

gtest_discover_tests(TextAnalysis.Tests
    # set a working directory so your project root so that you can find test data via paths relative to the project root
    WORKING_DIRECTORY ${PROJECT_DIR}
    PROPERTIES VS_DEBUGGER_WORKING_DIRECTORY "${PROJECT_DIR}"
)