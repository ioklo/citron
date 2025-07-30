
add_library(IR0 SHARED)

target_compile_options(IR0 PRIVATE
        -Wno-unqualified-std-cast-call
)

target_include_directories(IR0
        PRIVATE Public/IR0
        PUBLIC Public)

target_link_libraries(IR0 PRIVATE Infra Symbol)

target_sources(IR0
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
        NExp.cpp
        NGlobalFuncDecl.cpp
        NInterfaceDecl.cpp
        NLambdaDecl.cpp
        NLambdaVarDecl.cpp
        NLoc.cpp
        NModule.cpp
        NNamespaceDecl.cpp
        NNamespaceDeclContainerComponent.cpp
        NStmt.cpp
        NStructCtorDecl.cpp
        NStructDecl.cpp
        NStructFuncDecl.cpp
        NStructVarDecl.cpp
        NTypeDeclContainerComponent.cpp
        RDecl.cpp
        RFuncParameter.cpp
        RGlobalFuncDecl.cpp
        RMember.cpp
        RModule.cpp
        RNames.cpp
        RNamespaceDeclGroup.cpp
        RTypeArguments.cpp
        RTypeFactory.cpp
        RTypes.cpp

        PUBLIC FILE_SET headers
        TYPE HEADERS
        FILES
        Public/IR0/DeclWithOuterTypeArgs.h
        Public/IR0/IR0Config.h
        Public/IR0/NArgument.h
        Public/IR0/NClassCtorDecl.h
        Public/IR0/NClassDecl.h
        Public/IR0/NClassFuncDecl.h
        Public/IR0/NClassVarDecl.h
        Public/IR0/NCommonFuncDeclComponent.h
        Public/IR0/NDecl.h
        Public/IR0/NDecls.h
        Public/IR0/NEnumDecl.h
        Public/IR0/NEnumElemDecl.h
        Public/IR0/NEnumElemVarDecl.h
        Public/IR0/NExp.h
        Public/IR0/NFuncDecl.h
        Public/IR0/NFuncDeclContainerComponent.h
        Public/IR0/NFuncDeclOuter.h
        Public/IR0/NGlobalFuncDecl.h
        Public/IR0/NInterfaceDecl.h
        Public/IR0/NLambdaDecl.h
        Public/IR0/NLambdaVarDecl.h
        Public/IR0/NLoc.h
        Public/IR0/NModule.h
        Public/IR0/NNamespaceDecl.h
        Public/IR0/NNamespaceDeclContainerComponent.h
        Public/IR0/NStmt.h
        Public/IR0/NStructCtorDecl.h
        Public/IR0/NStructDecl.h
        Public/IR0/NStructFuncDecl.h
        Public/IR0/NStructVarDecl.h
        Public/IR0/NTypeDecl.h
        Public/IR0/NTypeDeclContainerComponent.h
        Public/IR0/NTypeDeclOuter.h
        Public/IR0/RAccessor.h
        Public/IR0/RClassCtorDecl.h
        Public/IR0/RClassDecl.h
        Public/IR0/RClassFuncDecl.h
        Public/IR0/RClassVarDecl.h
        Public/IR0/RDecl.h
        Public/IR0/RDecls.h
        Public/IR0/REnumDecl.h
        Public/IR0/REnumElemDecl.h
        Public/IR0/REnumElemVarDecl.h
        Public/IR0/RFuncDecl.h
        Public/IR0/RFuncDeclOuter.h
        Public/IR0/RFuncParameter.h
        Public/IR0/RFuncReturn.h
        Public/IR0/RGlobalFuncDecl.h
        Public/IR0/RIdentifier.h
        Public/IR0/RInterfaceDecl.h
        Public/IR0/RLambdaDecl.h
        Public/IR0/RLambdaVarDecl.h
        Public/IR0/RMember.h
        Public/IR0/RModule.h
        Public/IR0/RNames.h
        Public/IR0/RNamespaceDecl.h
        Public/IR0/RNamespaceDeclGroup.h
        Public/IR0/RStructCtorDecl.h
        Public/IR0/RStructDecl.h
        Public/IR0/RStructFuncDecl.h
        Public/IR0/RStructVarDecl.h
        Public/IR0/RTypeArguments.h
        Public/IR0/RTypeDecl.h
        Public/IR0/RTypeDeclOuter.h
        Public/IR0/RTypeFactory.h
        Public/IR0/RTypes.h

)