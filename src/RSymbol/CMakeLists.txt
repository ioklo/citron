
add_library(RSymbol SHARED)

set_target_properties(RSymbol PROPERTIES DEFINE_SYMBOL RSYMBOL_EXPORTS)

target_include_directories(RSymbol
        PRIVATE Public/RSymbol
        PUBLIC Public)

target_link_libraries(RSymbol PRIVATE
        Common
        Infra
        ESymbol
)

include(CMakeSources.g.txt)