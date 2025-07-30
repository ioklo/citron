
add_library(Builder SHARED)

target_compile_options(Builder PRIVATE
        -Wno-unqualified-std-cast-call
)

target_include_directories(Builder PRIVATE Public/Builder PUBLIC Public)

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