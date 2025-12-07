
add_library(Builder SHARED)

set_target_properties(Builder PROPERTIES DEFINE_SYMBOL BUILDER_EXPORTS)

target_include_directories(Builder PRIVATE Public/Builder PUBLIC Public)


target_link_libraries(Builder PRIVATE
        Common
)

target_sources(Builder
        PRIVATE
        Builder.cpp

        PUBLIC FILE_SET headers
        TYPE HEADERS
        FILES
        Public/Builder/BuilderConfig.h
        Public/Builder/Builder.h
        Public/Builder/IFileSystem.h
)