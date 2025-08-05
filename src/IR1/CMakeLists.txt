add_library(IR1 SHARED)

target_compile_options(IR1 PRIVATE
        -Wno-unqualified-std-cast-call
)

target_include_directories(IR1
        PRIVATE Public/IR1
        PUBLIC Public)

target_link_libraries(IR1 PRIVATE Infra Symbol)

target_sources(IR1
        PRIVATE
        QFactory.cpp

        PUBLIC FILE_SET headers
        TYPE HEADERS
        FILES
        Public/IR1/IR1Config.h
        Public/IR1/QFactory.h
)