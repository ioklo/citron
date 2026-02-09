
add_library(Logging SHARED)

set_target_properties(Logging PROPERTIES DEFINE_SYMBOL LOGGING_EXPORTS)

target_include_directories(Logging PRIVATE Public/Logging PUBLIC Public)


target_link_libraries(Logging PRIVATE
        Common
)

include(CMakeSources.g.txt)