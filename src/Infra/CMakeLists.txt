
add_library(Infra SHARED)

set_target_properties(Infra PROPERTIES DEFINE_SYMBOL INFRA_EXPORTS)

target_include_directories(Infra
        PRIVATE Public/Infra
        INTERFACE Public)


target_link_libraries(Infra PRIVATE
        Common
)

include(CMakeSources.g.txt)