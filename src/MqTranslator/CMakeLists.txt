
add_library(MqTranslator SHARED
        )

set_target_properties(MqTranslator PROPERTIES DEFINE_SYMBOL MQTRANSLATOR_EXPORTS)

target_include_directories(MqTranslator
        PRIVATE Public/MqTranslator
        PUBLIC Public)

target_link_libraries(MqTranslator PRIVATE
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