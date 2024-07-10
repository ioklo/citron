# CMakeList.txt : CMake project for Infra, include source and define
# project specific logic here.
#

# Add source to this project's executable.
add_library(Symbol

  Copy.h
  Symbols.h
  
  MAccessor.h
  MBodyDeclOuter.h
  MBodyOuter.h
  MClass.h
  MClassConstructor.h
  MClassConstructorDecl.h
  MClassDecl.h
  MClassMemberFunc.h
  MClassMemberFuncDecl.h
  MClassMemberVar.h
  MClassMemberVarDecl.h
  MCommonFuncDeclComponent.h
  MDecl.h
  MEnum.h
  MEnumDecl.h
  MEnumElem.h
  MEnumElemDecl.h
  MEnumElemMemberVar.h
  MEnumElemMemberVarDecl.h
  MFunc.h
  MFuncDeclContainerComponent.h
  MFuncParameter.h
  MFuncReturn.h
  MGlobalFunc.h
  MGlobalFuncDecl.h
  MIdentifier.h
  MInterfaceDecl.h
  
  MLambda.h

  MLambdaDecl.h
  MLabmdaDecl.cpp

  MLambdaMemberVar.h
  MLambdaMemberVarDecl.h
  
  MModuleDecl.h
  MModuleDecl.cpp
  
  MNames.h
  MNames.cpp

  MNamespaceDecl.h
  MNamespaceDeclContainerComponent.h
  MStruct.h
  MStructConstructor.h
  MStructConstructorDecl.h
  MStructDecl.h
  MStructMemberFunc.h
  MStructMemberFuncDecl.h
  MStructMemberVar.h
  MStructMemberVarDecl.h
  MSymbol.h
  MSymbolComponent.h
  
  MSymbolId.h
  MSymbolId.cpp
  
  MTopLevelOuter.h
  MTopLevelOuterDecl.h

  MType.h
  MTypeDecl.h
  
  MTypeDeclContainerComponent.h
  MTypeDeclContainerComponent.cpp

  MTypeDeclOuter.h
  MTypeId.h
  MTypeIds.h
  MTypeIds.cpp
  
  MTypeOuter.h
  MTypes.h
)


# TODO: Add tests and install targets if needed.
target_include_directories (Symbol PUBLIC ../../include)
target_compile_definitions(
  Symbol 
  PRIVATE 
    SYMBOL_EXPORT
  PUBLIC
    $<$<STREQUAL:$<TARGET_PROPERTY:Infra,TYPE>,SHARED_LIBRARY>:INFRA_IMPORT>
)
target_link_libraries(Symbol PUBLIC Infra)

#get_property(IS_IN_TRY_COMPILE GLOBAL PROPERTY IN_TRY_COMPILE)
#if(NOT IS_IN_TRY_COMPILE)
#  target_compile_options(Symbol PRIVATE "$<$<CXX_COMPILER_ID:Clang>:-fmodule-file=std.pcm>")
#endif()



