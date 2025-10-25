
add_library(QIR SHARED)

target_compile_options(QIR PRIVATE
        -Wno-unqualified-std-cast-call
)

target_include_directories(QIR
        PRIVATE Public/QIR
        PUBLIC Public)

target_link_libraries(QIR PRIVATE)

target_sources(QIR
        PRIVATE
        QBlock.cpp
        QFactory.cpp
        QValues.cpp

        PUBLIC FILE_SET headers
        TYPE HEADERS
        FILES

        Public/QIR/QBlock.h
        Public/QIR/QFactory.h
        Public/QIR/QInsts.h
        Public/QIR/QIRConfig.h
        Public/QIR/QValues.h

)