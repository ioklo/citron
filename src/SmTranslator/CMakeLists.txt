
add_library(SmTranslator SHARED)

set_target_properties(SmTranslator PROPERTIES DEFINE_SYMBOL SMTRANSLATOR_EXPORTS)

target_include_directories(SmTranslator
        PRIVATE Public/SmTranslator
        PUBLIC Public)

target_link_libraries(SmTranslator PRIVATE
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