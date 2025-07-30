
add_library(Symbol SHARED)

target_compile_options(Symbol PRIVATE
        -Wno-unqualified-std-cast-call
)

target_include_directories(Symbol
        PRIVATE Public/Symbol
        PUBLIC Public)

target_link_libraries(Symbol PRIVATE Infra)

target_sources(Symbol
        PRIVATE
        MDeclId.cpp
        MEnumElemDecl.cpp
        MModule.cpp
        MNames.cpp
        MNamespaceDeclContainerComponent.cpp
        MType.cpp
        MTypeArguments.cpp
        MTypeDeclContainerComponent.cpp

        PUBLIC FILE_SET headers
        TYPE HEADERS
        FILES
        Public/Symbol/SymbolConfig.h
        Public/Symbol/Symbols.h
        Public/Symbol/MAccessor.h
        Public/Symbol/MBodyDeclOuter.h
        Public/Symbol/MClassCtorDecl.h
        Public/Symbol/MClassDecl.h
        Public/Symbol/MClassFuncDecl.h
        Public/Symbol/MClassVarDecl.h
        Public/Symbol/MCommonFuncDeclComponent.h
        Public/Symbol/MDecl.h
        Public/Symbol/MDeclId.h
        Public/Symbol/MDecls.h
        Public/Symbol/MEnumDecl.h
        Public/Symbol/MEnumElemDecl.h
        Public/Symbol/MEnumElemVarDecl.h
        Public/Symbol/MFunc.h
        Public/Symbol/MFuncDecl.h
        Public/Symbol/MFuncDeclContainerComponent.h
        Public/Symbol/MFuncParameter.h
        Public/Symbol/MFuncReturn.h
        Public/Symbol/MGlobalFuncDecl.h
        Public/Symbol/MIdentifier.h
        Public/Symbol/MInterfaceDecl.h
        Public/Symbol/MModule.h
        Public/Symbol/MNames.h
        Public/Symbol/MNamespaceDecl.h
        Public/Symbol/MNamespaceDeclContainerComponent.h
        Public/Symbol/MStructCtorDecl.h
        Public/Symbol/MStructDecl.h
        Public/Symbol/MStructFuncDecl.h
        Public/Symbol/MStructVarDecl.h
        Public/Symbol/MType.h
        Public/Symbol/MTypeArguments.h
        Public/Symbol/MTypeDecl.h
        Public/Symbol/MTypeDeclContainerComponent.h
        Public/Symbol/MTypeDeclOuter.h
)