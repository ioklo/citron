
add_library(MIR SHARED)

target_compile_options(MIR PRIVATE
        -Wno-unqualified-std-cast-call
)

target_include_directories(MIR
        PRIVATE Public/MIR
        PUBLIC Public)

target_link_libraries(MIR PRIVATE Infra ESymbol RSymbol NSymbol)

target_sources(MIR
        PRIVATE
        MExp.cpp 
        MFactory.cpp 
        MLoc.cpp 
        MStmt.cpp

        PUBLIC FILE_SET headers
        TYPE HEADERS
        FILES
        
        Public/MIR/MArgument.h
        Public/MIR/MExp.h
        Public/MIR/MFactory.h
        Public/MIR/MIRConfig.h
        Public/MIR/MLoc.h
        Public/MIR/MStmt.h

)