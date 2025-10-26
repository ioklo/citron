
add_library(RSymbol SHARED)

set_target_properties(RSymbol PROPERTIES DEFINE_SYMBOL RSYMBOL_EXPORTS)

target_compile_options(RSymbol PRIVATE
        -Wno-unqualified-std-cast-call
)

target_include_directories(RSymbol
        PRIVATE Public/RSymbol
        PUBLIC Public)

target_link_libraries(RSymbol PRIVATE Infra ESymbol)

target_sources(RSymbol
        PRIVATE
        RDecl.cpp
        RFactory.cpp
        RFuncParameter.cpp
        RGlobalFuncDecl.cpp
        RMember.cpp
        RModule.cpp
        RNames.cpp
        RNamespaceDeclGroup.cpp
        RTypeArguments.cpp
        RTypes.cpp

        PUBLIC FILE_SET headers
        TYPE HEADERS
        FILES
        
        Public/RSymbol/DeclWithOuterTypeArgs.h
        Public/RSymbol/RAccessor.h
        Public/RSymbol/RClassCtorDecl.h
        Public/RSymbol/RClassDecl.h
        Public/RSymbol/RClassFuncDecl.h
        Public/RSymbol/RClassVarDecl.h
        Public/RSymbol/RDecl.h
        Public/RSymbol/RDecls.h
        Public/RSymbol/REnumDecl.h
        Public/RSymbol/REnumElemDecl.h
        Public/RSymbol/REnumElemVarDecl.h
        Public/RSymbol/RFactory.h
        Public/RSymbol/RFuncDecl.h
        Public/RSymbol/RFuncDeclOuter.h
        Public/RSymbol/RFuncParameter.h
        Public/RSymbol/RFuncReturn.h
        Public/RSymbol/RGlobalFuncDecl.h
        Public/RSymbol/RIdentifier.h
        Public/RSymbol/RInterfaceDecl.h
        Public/RSymbol/RLambdaDecl.h
        Public/RSymbol/RLambdaVarDecl.h
        Public/RSymbol/RMember.h
        Public/RSymbol/RModule.h
        Public/RSymbol/RNames.h
        Public/RSymbol/RNamespaceDecl.h
        Public/RSymbol/RNamespaceDeclGroup.h
        Public/RSymbol/RStructCtorDecl.h
        Public/RSymbol/RStructDecl.h
        Public/RSymbol/RStructFuncDecl.h
        Public/RSymbol/RStructVarDecl.h
        Public/RSymbol/RSymbolConfig.h
        Public/RSymbol/RTypeArguments.h
        Public/RSymbol/RTypeDecl.h
        Public/RSymbol/RTypeDeclOuter.h
        Public/RSymbol/RTypes.h

)