
add_library(QEvaluator SHARED)

set_target_properties(QEvaluator PROPERTIES DEFINE_SYMBOL QEVALUATOR_EXPORTS)

target_compile_options(QEvaluator PRIVATE
        -Wno-unqualified-std-cast-call
)

target_include_directories(QEvaluator
        PRIVATE Public/QEvaluator
        INTERFACE Public)

target_link_libraries(QEvaluator PRIVATE Infra Logging RSymbol NSymbol QIR)


target_sources(QEvaluator
        PRIVATE
        QEvaluation.cpp

        PUBLIC FILE_SET headers
        TYPE HEADERS
        FILES
        Public/QEvaluator/QEvaluation.h
        Public/QEvaluator/QEvaluatorConfig.h
)

