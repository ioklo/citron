
add_library(ESymbol SHARED)

set_target_properties(ESymbol PROPERTIES DEFINE_SYMBOL ESYMBOL_EXPORTS)

target_include_directories(ESymbol
        PRIVATE Public/ESymbol
        PUBLIC Public)

target_link_libraries(ESymbol PRIVATE
        Common
        Infra
)

include(CMakeSources.g.txt)