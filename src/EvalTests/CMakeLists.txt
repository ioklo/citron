
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
        MqTranslator
        QEvaluator
#        QIrLLVMTranslator
        GTest::gtest_main
)

include(CMakeSources.g.txt)

find_package(GTest CONFIG REQUIRED)

add_custom_command(TARGET EvalTests POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy_if_different
    $<TARGET_RUNTIME_DLLS:EvalTests>
    $<TARGET_FILE_DIR:EvalTests>
    COMMAND_EXPAND_LISTS
)

include(GoogleTest)
gtest_discover_tests(EvalTests
  DISCOVERY_MODE PRE_TEST
)