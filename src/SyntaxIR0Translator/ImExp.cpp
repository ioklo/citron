#include "ImExp.h"
#include "RSymbol/DeclWithOuterTypeArgs.h"
#include "RSymbol/RStructFuncDecl.h"

using namespace std;

namespace Citron {

ImExp_Namespace::ImExp_Namespace(RNamespaceDecl* _namespace)
    : _namespace(_namespace)
{
}

ImExp_GlobalFuncs::ImExp_GlobalFuncs(const std::vector<DeclWithOuterTypeArgs<RGlobalFuncDecl>>& items, RTypeArguments* partialTypeArgs)
    : FuncsWithPartialTypeArgsComponent<RGlobalFuncDecl>(items, partialTypeArgs)
{
}

ImExp_GlobalFuncs::~ImExp_GlobalFuncs() = default;

ImExp_TypeVar::ImExp_TypeVar(RType_TypeVar* type)
    : type(type)
{
}

ImExp_Class::ImExp_Class(RClassDecl* classDecl, RTypeArguments* typeArgs)
    : classDecl(classDecl), typeArgs(typeArgs)
{
}

ImExp_ClassFuncs::ImExp_ClassFuncs(const std::vector<DeclWithOuterTypeArgs<RClassFuncDecl>>& items, RTypeArguments* partialTypeArgsExceptOuter, bool hasExplicitInstance, ReExp* explicitInstance)
    : FuncsWithPartialTypeArgsComponent<RClassFuncDecl>(items, partialTypeArgsExceptOuter), hasExplicitInstance(hasExplicitInstance), explicitInstance(explicitInstance)
{
}

ImExp_ClassFuncs::~ImExp_ClassFuncs() = default;

ImExp_Struct::ImExp_Struct(RStructDecl* structDecl, RTypeArguments* typeArgs)
    : structDecl(structDecl), typeArgs(typeArgs)
{
}

ImExp_StructFuncs::ImExp_StructFuncs(const std::vector<DeclWithOuterTypeArgs<RStructFuncDecl>>& items, RTypeArguments* partialTypeArgsExceptOuter, bool hasExplicitInstance, ReExp* explicitInstance)
    : FuncsWithPartialTypeArgsComponent<RStructFuncDecl>(items, partialTypeArgsExceptOuter), hasExplicitInstance(hasExplicitInstance), explicitInstance(explicitInstance)
{

}

ImExp_StructFuncs::~ImExp_StructFuncs() = default;

ImExp_Enum::ImExp_Enum(REnumDecl* decl, RTypeArguments* typeArgs)
    : decl(decl), typeArgs(typeArgs)
{

}

ImExp_EnumElem::ImExp_EnumElem(REnumElemDecl* decl, RTypeArguments* typeArgs)
    : decl(decl), typeArgs(typeArgs)
{
}

ImExp_ThisVar::ImExp_ThisVar(RType* type)
    : type(type)
{

}

ImExp_LocalVar::ImExp_LocalVar(RType* type, const RName& name)
    : type(type), name(name)
{

}

ImExp_LocalRef::ImExp_LocalRef(RType* type, const RName& name)
    : type(type), name(name)
{

}

ImExp_LambdaVar::ImExp_LambdaVar(NLambdaVarDecl* decl, RTypeArguments* typeArgs)
    : decl(decl), typeArgs(typeArgs)
{

}

ImExp_ClassVar::ImExp_ClassVar(RClassVarDecl* decl, RTypeArguments* typeArgs, bool hasExplicitInstance, ReExp* explicitInstance)
    : decl(decl), typeArgs(typeArgs), hasExplicitInstance(hasExplicitInstance), explicitInstance(explicitInstance)
{
}

ImExp_StructVar::ImExp_StructVar(RStructVarDecl* decl, RTypeArguments* typeArgs, bool hasExplicitInstance, ReExp* explicitInstance)
    : decl(decl), typeArgs(typeArgs), hasExplicitInstance(hasExplicitInstance), explicitInstance(explicitInstance)
{
}

ImExp_EnumElemVar::ImExp_EnumElemVar(REnumElemVarDecl* decl, RTypeArguments* typeArgs, ReExp* instance)
    : decl(decl), typeArgs(typeArgs), instance(instance)
{

}

ImExp_ListIndexer::ImExp_ListIndexer(ReExp* instance, ReExp* index, RType* itemType)
    : instance{instance}, index{index}, itemType{itemType}
{

}

ImExp_PtrDeref::ImExp_PtrDeref(ReExp* target)
    : target(target)
{

}

ImExp_BoxDeref::ImExp_BoxDeref(ReExp* target)
    : target(target)
{

}

ImExp_Else::ImExp_Else(MExp* exp)
    : exp(exp)
{
}

} // namespace Citron
