
add_library(SyntaxIR0Translator SHARED)

set_target_properties(SyntaxIR0Translator PROPERTIES DEFINE_SYMBOL SYNTAXIR0TRANSLATOR_EXPORTS)

target_include_directories(SyntaxIR0Translator
        PRIVATE Public/SyntaxIR0Translator
        PUBLIC Public)

target_link_libraries(SyntaxIR0Translator PRIVATE
        Common
        Infra
        Syntax
        Logging
        ESymbol
        RSymbol
        NSymbol
        MIR
)

include(CMakeSources.g.txt)