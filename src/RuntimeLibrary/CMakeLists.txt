add_library(RuntimeLibrary STATIC)


target_link_libraries(RuntimeLibrary PRIVATE
        Common
)

include(CMakeSources.g.txt)