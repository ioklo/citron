# CMakeList.txt : CMake project for Infra, include source and define
# project specific logic here.
#

# Add source to this project's executable.
add_library(Symbol SHARED

)

target_sources(Symbol PRIVATE
  FILE_SET CXX_MODULES
  FILES
    Copy.ixx
    Symbols.ixx

    MAccessor.ixx
    MBodyDeclOuter.ixx
    MBodyOuter.ixx
    MClass.ixx
    MClassConstructor.ixx
    MClassConstructorDecl.ixx
    MClassDecl.ixx
    MClassMemberFunc.ixx
    MClassMemberFuncDecl.ixx
    MClassMemberVar.ixx
    MClassMemberVarDecl.ixx
    MCommonFuncDeclComponent.ixx
    MDecl.ixx
    MEnum.ixx
    MEnumDecl.ixx
    MEnumElem.ixx
    MEnumElemDecl.ixx
    MEnumElemMemberVar.ixx
    MEnumElemMemberVarDecl.ixx
    MFunc.ixx
    MFuncDeclContainerComponent.ixx
    MFuncParameter.ixx
    MFuncReturn.ixx
    MGlobalFunc.ixx
    MGlobalFuncDecl.ixx
    MIdentifier.ixx
    MInterfaceDecl.ixx

    MLambda.ixx
    MLambdaDecl.ixx
    MLambdaMemberVar.ixx
    MLambdaMemberVarDecl.ixx

    MModuleDecl.ixx

    MNames.ixx
    MNamespaceDecl.ixx
    MNamespaceDeclContainerComponent.ixx
    MStruct.ixx
    MStructConstructor.ixx
    MStructConstructorDecl.ixx
    MStructDecl.ixx
    MStructMemberFunc.ixx
    MStructMemberFuncDecl.ixx
    MStructMemberVar.ixx
    MStructMemberVarDecl.ixx
    MSymbol.ixx
    MSymbolComponent.ixx

    MSymbolId.ixx

    MTopLevelOuter.ixx
    MTopLevelOuterDecl.ixx
    MType.ixx
    MTypeDecl.ixx

    MTypeDeclContainerComponent.ixx
    MTypeDeclOuter.ixx
    MTypeId.ixx
    MTypeIds.ixx

    MTypeOuter.ixx
    MTypes.ixx
)

# TODO: Add tests and install targets if needed.
target_include_directories (Symbol PUBLIC ../../include)
target_compile_definitions(Symbol PRIVATE SYMBOL_EXPORT)
target_link_libraries(Symbol PUBLIC Infra)

target_compile_features(Symbol PRIVATE cxx_std_20)

target_compile_options(Symbol PRIVATE "$<$<C_COMPILER_ID:MSVC>:/utf-8>")
target_compile_options(Symbol PRIVATE "$<$<CXX_COMPILER_ID:MSVC>:/utf-8>")


