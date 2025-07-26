
add_library(Logging SHARED)

target_compile_options(Logging PRIVATE
        -Wno-unqualified-std-cast-call
)

target_include_directories(Logging PUBLIC Public/Logging)

target_sources(Logging
        PRIVATE
        Diag.cpp
        Logger.cpp

        PUBLIC FILE_SET headers
        TYPE HEADERS
        FILES
        Public/Logging/LoggingConfig.h

        PUBLIC FILE_SET cxx_modules
        TYPE CXX_MODULES
        FILES
        Diag.ixx
        Logger.ixx
)