
add_library(QIR SHARED)

set_target_properties(QIR PROPERTIES DEFINE_SYMBOL QIR_EXPORTS)

target_include_directories(QIR
        PRIVATE Public/QIR
        PUBLIC Public)

target_link_libraries(QIR PRIVATE
        Common
        Infra
        NSymbol
        RSymbol
)

include(CMakeSources.g.txt)
