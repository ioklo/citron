# CMakeList.txt : CMake project for Infra, include source and define
# project specific logic here.
#

# Add source to this project's executable.
add_library(Symbol SHARED
        std.h

        #MLabmdaDecl.cpp
  #MModuleDecl.cpp
  #MNames.cpp
  #MSymbolId.cpp
  #MTopLevelDeclComponent.cpp
  #MTypeDeclContainerComponent.cpp
  #MTypeIds.cpp
)

MESSAGE(STATUS "hi2")
if ("${CMAKE_CXX_COMPILER_ID}" MATCHES "Clang")
  MESSAGE(STATUS "hi")
add_custom_command(
  OUTPUT
    std.pcm
  COMMAND
    ${CMAKE_CXX_COMPILER} -std=c++20 -xc++-user-header --precompile ${CMAKE_CURRENT_SOURCE_DIR}/std.h -o std.pcm
  DEPENDS
    ${CMAKE_CURRENT_SOURCE_DIR}/std.h
)
endif()

add_custom_target(StdHeaderUnit
  DEPENDS
    std.pcm
)

add_dependencies(Symbol StdHeaderUnit)

# -

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
target_compile_options(Symbol PRIVATE "$<$<CXX_COMPILER_ID:Clang>:-fmodule-file=${CMAKE_CURRENT_BINARY_DIR}/std.pcm>")
target_compile_options(Symbol PRIVATE "$<$<CXX_COMPILER_ID:Clang>:-Wno-experimental-header-units>")


#get_property(IS_IN_TRY_COMPILE GLOBAL PROPERTY IN_TRY_COMPILE)
#if(NOT IS_IN_TRY_COMPILE)
#  target_compile_options(Symbol PRIVATE "$<$<CXX_COMPILER_ID:Clang>:-fmodule-file=std.pcm>")
#endif()



