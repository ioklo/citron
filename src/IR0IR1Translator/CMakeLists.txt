
add_library(IR0IR1Translator SHARED
        )

set_target_properties(IR0IR1Translator PROPERTIES DEFINE_SYMBOL IR0IR1TRANSLATOR_EXPORTS)

target_compile_options(IR0IR1Translator PRIVATE
        -Wno-unqualified-std-cast-call
)

target_include_directories(IR0IR1Translator
        PRIVATE Public/IR0IR1Translator
        PUBLIC Public)

target_link_libraries(IR0IR1Translator PRIVATE Logging Infra ESymbol RSymbol MIR QIR)

target_sources(IR0IR1Translator
        PRIVATE
        MExpQInstsTranslation.cpp
        MLocQInstsTranslation.cpp
        MStmtQInstsTranslation.cpp
        QBodyContext.cpp

        PUBLIC FILE_SET headers
        TYPE HEADERS
        FILES
        Public/IR0IR1Translator/IR0IR1Translator.h
        Public/IR0IR1Translator/IR0IR1TranslatorConfig.h

        MExpQInstsTranslation.h
        MLocQInstsTranslation.h
        MStmtQInstsTranslation.h
        QBodyContext.h
)