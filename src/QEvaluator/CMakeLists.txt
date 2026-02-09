
add_library(QEvaluator SHARED)

set_target_properties(QEvaluator PROPERTIES DEFINE_SYMBOL QEVALUATOR_EXPORTS)

target_include_directories(QEvaluator
        PRIVATE Public/QEvaluator
        INTERFACE Public)

target_link_libraries(QEvaluator PRIVATE
        Common
        Infra
        Logging
        RSymbol
        NSymbol
        QIR
)

include(CMakeSources.g.txt)