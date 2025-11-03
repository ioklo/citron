
add_library(QIR SHARED)

set_target_properties(QIR PROPERTIES DEFINE_SYMBOL QIR_EXPORTS)

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
        QData.cpp
        QFactory.cpp
        QValues.cpp

        PUBLIC FILE_SET headers
        TYPE HEADERS
        FILES

        Public/QIR/QBlock.h
        Public/QIR/QData.h
        Public/QIR/QFactory.h
        Public/QIR/QFuncBody.h
        Public/QIR/QInsts.h
        Public/QIR/QIRConfig.h
        Public/QIR/QValues.h

)