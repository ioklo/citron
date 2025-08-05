
add_library(IR0IR1Translator SHARED
        )

target_compile_options(IR0IR1Translator PRIVATE
        -Wno-unqualified-std-cast-call
)

target_include_directories(IR0IR1Translator
        PRIVATE Public/IR0IR1Translator
        PUBLIC Public)

target_link_libraries(IR0IR1Translator PRIVATE Logging Infra Symbol IR0 IR1)

target_sources(IR0IR1Translator
        PRIVATE
        IR0IR1Translator.cpp
        NExpQInstsTranslation.cpp
        NLocQInstsTranslation.cpp
        NStmtQInstsTranslation.cpp
        QBodyContext.cpp

        PUBLIC FILE_SET headers
        TYPE HEADERS
        FILES
        Public/IR0IR1Translator/IR0IR1TranslatorConfig.h
        Public/IR0IR1Translator/IR0IR1Translator.h
        NExpQInstsTranslation.h
        NLocQInstsTranslation.h
        NStmtQInstsTranslation.h
        QBodyContext.h
)