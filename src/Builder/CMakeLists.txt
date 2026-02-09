
add_library(Builder SHARED)

set_target_properties(Builder PROPERTIES DEFINE_SYMBOL BUILDER_EXPORTS)

target_include_directories(Builder PRIVATE Public/Builder PUBLIC Public)


target_link_libraries(Builder PRIVATE
        Common
)

include(CMakeSources.g.txt)