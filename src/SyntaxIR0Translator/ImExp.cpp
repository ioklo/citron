#include "pch.h"
#include "ImExp.h"

namespace Citron::SyntaxIR0Translator {

ImExp_Namespace::ImExp_Namespace(const std::shared_ptr<NNamespaceDecl>& _namespace)
    : _namespace(_namespace)
{
}

ImExp_GlobalFuncs::ImExp_GlobalFuncs(const std::vector<DeclWithOuterTypeArgs<NGlobalFuncDecl>>& items, const std::shared_ptr<RTypeArguments>& partialTypeArgs)
    : FuncsWithPartialTypeArgsComponent<NGlobalFuncDecl>(items, partialTypeArgs)
{
}

ImExp_TypeVar::ImExp_TypeVar(std::shared_ptr<RType_TypeVar>&& type)
    : type(std::move(type))
{
}

ImExp_Class::ImExp_Class(const std::shared_ptr<NClassDecl>& classDecl, RTypeArgumentsPtr&& typeArgs)
    : classDecl(classDecl), typeArgs(std::move(typeArgs))
{
}

ImExp_ClassMemberFuncs::ImExp_ClassMemberFuncs(const std::vector<DeclWithOuterTypeArgs<NClassMemberFuncDecl>>& items, const std::shared_ptr<RTypeArguments>& partialTypeArgsExceptOuter, bool hasExplicitInstance, const ReExpPtr& explicitInstance)
    : FuncsWithPartialTypeArgsComponent<NClassMemberFuncDecl>(items, partialTypeArgsExceptOuter), hasExplicitInstance(hasExplicitInstance), explicitInstance(explicitInstance)
{
}

ImExp_Struct::ImExp_Struct(const std::shared_ptr<NStructDecl>& structDecl, RTypeArgumentsPtr&& typeArgs)
    : structDecl(structDecl), typeArgs(std::move(typeArgs))
{
}

ImExp_StructMemberFuncs::ImExp_StructMemberFuncs(const std::vector<DeclWithOuterTypeArgs<NStructMemberFuncDecl>>& items, const std::shared_ptr<RTypeArguments>& partialTypeArgsExceptOuter, bool hasExplicitInstance, const ReExpPtr& explicitInstance)
    : FuncsWithPartialTypeArgsComponent<NStructMemberFuncDecl>(items, partialTypeArgsExceptOuter), hasExplicitInstance(hasExplicitInstance), explicitInstance(explicitInstance)
{

}

ImExp_Enum::ImExp_Enum(const std::shared_ptr<NEnumDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : decl(decl), typeArgs(typeArgs)
{

}

ImExp_EnumElem::ImExp_EnumElem(const std::shared_ptr<NEnumElemDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : decl(decl), typeArgs(typeArgs)
{
}

ImExp_ThisVar::ImExp_ThisVar(const RTypePtr& type)
    : type(type)
{

}

ImExp_LocalVar::ImExp_LocalVar(const RTypePtr& type, const std::string& name)
    : type(type), name(name)
{

}

ImExp_LambdaMemberVar::ImExp_LambdaMemberVar(const std::shared_ptr<NLambdaMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : decl(decl), typeArgs(typeArgs)
{

}

ImExp_ClassMemberVar::ImExp_ClassMemberVar(const std::shared_ptr<NClassMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs, bool hasExplicitInstance, const ReExpPtr& explicitInstance)
    : decl(decl), typeArgs(typeArgs), hasExplicitInstance(hasExplicitInstance), explicitInstance(explicitInstance)
{
}

ImExp_StructMemberVar::ImExp_StructMemberVar(const std::shared_ptr<NStructMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs, bool hasExplicitInstance, const ReExpPtr& explicitInstance)
    : decl(decl), typeArgs(typeArgs), hasExplicitInstance(hasExplicitInstance), explicitInstance(explicitInstance)
{
}

ImExp_EnumElemMemberVar::ImExp_EnumElemMemberVar(const std::shared_ptr<NEnumElemMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs, const ReExpPtr& instance)
    : decl(decl), typeArgs(typeArgs), instance(instance)
{

}

ImExp_ListIndexer::ImExp_ListIndexer(ReExpPtr&& instance, NLocPtr&& index, RTypePtr&& itemType)
    : instance(std::move(instance)), index(std::move(index)), itemType(std::move(itemType))
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

ImExp_Else::ImExp_Else(const NExpPtr& exp)
    : exp(exp)
{
}

} // namespace Citron::SyntaxIR0Translator
