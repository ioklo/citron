
add_library(NSymbol SHARED)

set_target_properties(NSymbol PROPERTIES DEFINE_SYMBOL NSYMBOL_EXPORTS)

target_include_directories(NSymbol
        PRIVATE Public/NSymbol
        PUBLIC Public)

target_link_libraries(NSymbol PRIVATE
        Common
        Infra
        ESymbol
        RSymbol
)

include(CMakeSources.g.txt)