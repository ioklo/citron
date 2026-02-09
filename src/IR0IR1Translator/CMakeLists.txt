
add_library(IR0IR1Translator SHARED
        )

set_target_properties(IR0IR1Translator PROPERTIES DEFINE_SYMBOL IR0IR1TRANSLATOR_EXPORTS)

target_include_directories(IR0IR1Translator
        PRIVATE Public/IR0IR1Translator
        PUBLIC Public)

target_link_libraries(IR0IR1Translator PRIVATE
        Common
        Logging
        Infra
        ESymbol
        RSymbol
        NSymbol
        MIR
        QIR
)
include(CMakeSources.g.txt)