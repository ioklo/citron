
add_library(TextAnalysis SHARED)

set_target_properties(TextAnalysis PROPERTIES DEFINE_SYMBOL TEXTANALYSIS_EXPORTS)

target_include_directories(TextAnalysis
        PRIVATE
          Public/TextAnalysis
          ../../externals/utfcpp/source
        PUBLIC Public)

find_package(ICU REQUIRED COMPONENTS uc i18n)

target_link_libraries(TextAnalysis PRIVATE
        Common
        Infra
        Syntax
        ICU::uc
        ICU::i18n
)
target_sources(TextAnalysis
        PRIVATE
        Buffer.cpp
        ExpParser.cpp
        Lexer.cpp
        ParserMisc.cpp
        ScriptParser.cpp
        StmtParser.cpp
        TypeExpParser.cpp

        PUBLIC FILE_SET headers
        TYPE HEADERS
        FILES
        Public/TextAnalysis/Buffer.h
        Public/TextAnalysis/ExpParser.h
        Public/TextAnalysis/Lexer.h
        Public/TextAnalysis/ParserMisc.h
        Public/TextAnalysis/ScriptParser.h
        Public/TextAnalysis/StmtParser.h
        Public/TextAnalysis/TextAnalysisConfig.h
        Public/TextAnalysis/TypeExpParser.h
)