add_library(RuntimeLibrary STATIC)


target_link_libraries(RuntimeLibrary PRIVATE
        Common
)

target_sources(RuntimeLibrary
        PRIVATE
        RuntimeLibrary.cpp
)
