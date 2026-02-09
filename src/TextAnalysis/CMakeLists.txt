
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

include(CMakeSources.g.txt)