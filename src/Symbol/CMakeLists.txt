
add_library(Symbol SHARED)

target_compile_options(Symbol PRIVATE
        -Wno-unqualified-std-cast-call
)

target_include_directories(Symbol PUBLIC
        ../Infra/Public
        Public/Symbol)
target_link_libraries(Symbol PUBLIC Infra)

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

        PUBLIC FILE_SET cxx_modules
        TYPE CXX_MODULES
        FILES

        MAccessor.ixx
        MBodyDeclOuter.ixx
        MClassCtorDecl.ixx
        MClassDecl.ixx
        MClassFuncDecl.ixx
        MClassVarDecl.ixx
        MCommonFuncDeclComponent.ixx
        MDecl.ixx
        MDeclId.ixx
        MDecls.ixx

        MEnumDecl.ixx
        MEnumElemDecl.ixx
        MEnumElemVarDecl.ixx

        MFunc.ixx
        MFuncDecl.ixx
        MFuncDeclContainerComponent.ixx

        MFuncParameter.ixx
        MFuncReturn.ixx

        MGlobalFuncDecl.ixx
        MIdentifier.ixx
        MInterfaceDecl.ixx
        MModule.ixx
        MNames.ixx

        MNamespaceDecl.ixx
        MNamespaceDeclContainerComponent.ixx

        MStructCtorDecl.ixx
        MStructDecl.ixx
        MStructFuncDecl.ixx
        MStructVarDecl.ixx

        MType.ixx
        MTypeArguments.ixx
        MTypeDecl.ixx
        MTypeDeclContainerComponent.ixx
        MTypeDeclOuter.ixx

)