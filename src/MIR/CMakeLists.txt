
add_library(MIR SHARED)

set_target_properties(MIR PROPERTIES DEFINE_SYMBOL MIR_EXPORTS)

target_include_directories(MIR
        PRIVATE Public/MIR
        PUBLIC Public)

target_link_libraries(MIR PRIVATE
        Common
        Infra
        ESymbol
        RSymbol
        NSymbol
)

include(CMakeSources.g.txt)