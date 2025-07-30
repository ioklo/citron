
add_library(Logging SHARED)

target_compile_options(Logging PRIVATE
        -Wno-unqualified-std-cast-call
)

target_include_directories(Logging PRIVATE Public/Logging PUBLIC Public)

target_sources(Logging
        PRIVATE
        Diag.cpp
        Logger.cpp

        PUBLIC FILE_SET headers
        TYPE HEADERS
        FILES
        Public/Logging/LoggingConfig.h
        Public/Logging/Diag.h
        Public/Logging/Logger.h
)