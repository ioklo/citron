
add_library(Syntax SHARED)

set_target_properties(Syntax PROPERTIES DEFINE_SYMBOL SYNTAX_EXPORTS)

target_include_directories(Syntax
        PRIVATE Public/Syntax
        INTERFACE Public)

target_link_libraries(Syntax PRIVATE
        Common
        Infra
)

include(CMakeSources.g.txt)