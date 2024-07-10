find_package(GTest CONFIG REQUIRED)

enable_testing()

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

target_compile_definitions(TextAnalysis.Tests 
  PUBLIC 
    $<$<STREQUAL:$<TARGET_PROPERTY:Infra,TYPE>,SHARED_LIBRARY>:INFRA_IMPORT>
    $<$<STREQUAL:$<TARGET_PROPERTY:Syntax,TYPE>,SHARED_LIBRARY>:SYNTAX_IMPORT>
    $<$<STREQUAL:$<TARGET_PROPERTY:TextAnalysis,TYPE>,SHARED_LIBRARY>:TEXTANALYSIS_IMPORT>
)

target_link_libraries(TextAnalysis.Tests PRIVATE Infra Syntax TextAnalysis GTest::gtest GTest::gtest_main)

include(GoogleTest)
gtest_discover_tests(TextAnalysis.Tests)
    # set a working directory so your project root so that you can find test data via paths relative to the project root
    # WORKING_DIRECTORY ${PROJECT_DIR}
    # PROPERTIES VS_DEBUGGER_WORKING_DIRECTORY "${PROJECT_DIR}"


# add_test(TextAnalysis.Tests.All TextAnalysis.Tests)