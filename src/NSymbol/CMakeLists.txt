
add_library(NSymbol SHARED)

target_compile_options(NSymbol PRIVATE
        -Wno-unqualified-std-cast-call
)

target_include_directories(NSymbol
        PRIVATE Public/NSymbol
        PUBLIC Public)

target_link_libraries(NSymbol PRIVATE Infra ESymbol RSymbol)

target_sources(NSymbol
        PRIVATE
        NClassCtorDecl.cpp
        NClassDecl.cpp
        NClassFuncDecl.cpp
        NClassVarDecl.cpp
        NCommonFuncDeclComponent.cpp
        NDecl.cpp
        NEnumDecl.cpp
        NEnumElemDecl.cpp
        NEnumElemVarDecl.cpp
        NFactory.cpp
        NGlobalFuncDecl.cpp
        NInterfaceDecl.cpp
        NLambdaDecl.cpp
        NLambdaVarDecl.cpp
        NModule.cpp
        NNamespaceDecl.cpp
        NNamespaceDeclContainerComponent.cpp
        NStructCtorDecl.cpp
        NStructDecl.cpp
        NStructFuncDecl.cpp
        NStructVarDecl.cpp
        NTypeDeclContainerComponent.cpp

        PUBLIC FILE_SET headers
        TYPE HEADERS
        FILES

        Public/NSymbol/NClassCtorDecl.h
        Public/NSymbol/NClassDecl.h
        Public/NSymbol/NClassFuncDecl.h
        Public/NSymbol/NClassVarDecl.h
        Public/NSymbol/NCommonFuncDeclComponent.h
        Public/NSymbol/NDecl.h
        Public/NSymbol/NEnumDecl.h
        Public/NSymbol/NEnumElemDecl.h
        Public/NSymbol/NEnumElemVarDecl.h
        Public/NSymbol/NFactory.h
        Public/NSymbol/NFuncDecl.h
        Public/NSymbol/NFuncDeclContainerComponent.h
        Public/NSymbol/NFuncDeclOuter.h
        Public/NSymbol/NGlobalFuncDecl.h
        Public/NSymbol/NInterfaceDecl.h
        Public/NSymbol/NLambdaDecl.h
        Public/NSymbol/NLambdaVarDecl.h
        Public/NSymbol/NModule.h
        Public/NSymbol/NNamespaceDecl.h
        Public/NSymbol/NNamespaceDeclContainerComponent.h
        Public/NSymbol/NStructCtorDecl.h
        Public/NSymbol/NStructDecl.h
        Public/NSymbol/NStructFuncDecl.h
        Public/NSymbol/NStructVarDecl.h
        Public/NSymbol/NSymbolConfig.h
        Public/NSymbol/NTypeDecl.h
        Public/NSymbol/NTypeDeclContainerComponent.h
        Public/NSymbol/NTypeDeclOuter.h


)