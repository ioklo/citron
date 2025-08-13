#include "ImExp.h"
#include "IR0/DeclWithOuterTypeArgs.h"
#include "IR0/RStructFuncDecl.h"

namespace Citron::SyntaxIR0Translator {

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
    : type(move(type))
{
}

ImExp_Class::ImExp_Class(RClassDecl* classDecl, RTypeArguments* typeArgs)
    : classDecl(classDecl), typeArgs(move(typeArgs))
{
}

ImExp_ClassFuncs::ImExp_ClassFuncs(const std::vector<DeclWithOuterTypeArgs<RClassFuncDecl>>& items, RTypeArguments* partialTypeArgsExceptOuter, bool hasExplicitInstance, const ReExpPtr& explicitInstance)
    : FuncsWithPartialTypeArgsComponent<RClassFuncDecl>(items, partialTypeArgsExceptOuter), hasExplicitInstance(hasExplicitInstance), explicitInstance(explicitInstance)
{
}

ImExp_ClassFuncs::~ImExp_ClassFuncs() = default;

ImExp_Struct::ImExp_Struct(RStructDecl* structDecl, RTypeArguments* typeArgs)
    : structDecl(structDecl), typeArgs(move(typeArgs))
{
}

ImExp_StructFuncs::ImExp_StructFuncs(const std::vector<DeclWithOuterTypeArgs<RStructFuncDecl>>& items, RTypeArguments* partialTypeArgsExceptOuter, bool hasExplicitInstance, const ReExpPtr& explicitInstance)
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

ImExp_LocalVar::ImExp_LocalVar(RType* type, const std::string& name)
    : type(type), name(name)
{

}

ImExp_LambdaVar::ImExp_LambdaVar(NLambdaVarDecl* decl, RTypeArguments* typeArgs)
    : decl(decl), typeArgs(typeArgs)
{

}

ImExp_ClassVar::ImExp_ClassVar(RClassVarDecl* decl, RTypeArguments* typeArgs, bool hasExplicitInstance, const ReExpPtr& explicitInstance)
    : decl(decl), typeArgs(typeArgs), hasExplicitInstance(hasExplicitInstance), explicitInstance(explicitInstance)
{
}

ImExp_StructVar::ImExp_StructVar(RStructVarDecl* decl, RTypeArguments* typeArgs, bool hasExplicitInstance, const ReExpPtr& explicitInstance)
    : decl(decl), typeArgs(typeArgs), hasExplicitInstance(hasExplicitInstance), explicitInstance(explicitInstance)
{
}

ImExp_EnumElemVar::ImExp_EnumElemVar(REnumElemVarDecl* decl, RTypeArguments* typeArgs, const ReExpPtr& instance)
    : decl(decl), typeArgs(typeArgs), instance(instance)
{

}

ImExp_ListIndexer::ImExp_ListIndexer(ReExpPtr&& instance, ReExpPtr&& index, RType* itemType)
    : instance(move(instance)), index(move(index)), itemType(move(itemType))
{

}

ImExp_LocalDeref::ImExp_LocalDeref(const ReExpPtr& target)
    : target(target)
{

}

ImExp_BoxDeref::ImExp_BoxDeref(const ReExpPtr& target)
    : target(target)
{

}

ImExp_Else::ImExp_Else(NExp* exp)
    : exp(exp)
{
}

} // namespace Citron::SyntaxIR0Translator
