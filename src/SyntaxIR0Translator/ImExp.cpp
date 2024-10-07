#include "pch.h"
#include "ImExp.h"

namespace Citron::SyntaxIR0Translator {

ImNamespaceExp::ImNamespaceExp(std::shared_ptr<RNamespaceDecl> _namespace)
    : _namespace(_namespace)
{
}

ImGlobalFuncsExp::ImGlobalFuncsExp(std::vector<DeclWithOuterTypeArgs<RGlobalFuncDecl>>&& data, const std::shared_ptr<RTypeArguments>& partialTypeArgs)
    : FuncsWithPartialTypeArgsComponent<RGlobalFuncDecl>(std::move(data), partialTypeArgs)
{
}

ImTypeVarExp::ImTypeVarExp(const std::shared_ptr<RTypeVarType>& type)
    : type(type)
{
}

ImClassExp::ImClassExp(const std::shared_ptr<RClassDecl>& classDecl, const RTypeArgumentsPtr& typeArgs)
    : classDecl(classDecl), typeArgs(typeArgs)
{
}

ImClassMemberFuncsExp::ImClassMemberFuncsExp(std::vector<DeclWithOuterTypeArgs<RClassMemberFuncDecl>>&& data, const std::shared_ptr<RTypeArguments>& partialTypeArgs, bool hasExplicitInstance, const ReExpPtr& explicitInstance)
    : FuncsWithPartialTypeArgsComponent<RClassMemberFuncDecl>(std::move(data), partialTypeArgs), hasExplicitInstance(hasExplicitInstance), explicitInstance(explicitInstance)
{
}

ImStructExp::ImStructExp(const std::shared_ptr<RStructDecl>& structDecl, const RTypeArgumentsPtr& typeArgs)
    : structDecl(structDecl), typeArgs(typeArgs)
{
}

ImStructMemberFuncsExp::ImStructMemberFuncsExp(std::vector<DeclWithOuterTypeArgs<RStructMemberFuncDecl>>&& data, const std::shared_ptr<RTypeArguments>& partialTypeArgs)
    : FuncsWithPartialTypeArgsComponent<RStructMemberFuncDecl>(std::move(data), partialTypeArgs)
{
}

ImEnumExp::ImEnumExp(const std::shared_ptr<REnumDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : decl(decl), typeArgs(typeArgs)
{

}

ImEnumElemExp::ImEnumElemExp(const std::shared_ptr<REnumElemDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : decl(decl), typeArgs(typeArgs)
{
}

ImThisVarExp::ImThisVarExp(const RTypePtr& type)
    : type(type)
{

}

ImLocalVarExp::ImLocalVarExp(const RTypePtr& type, const std::string& name)
    : type(type), name(name)
{

}

ImLambdaMemberVarExp::ImLambdaMemberVarExp(const std::shared_ptr<RLambdaMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : decl(decl), typeArgs(typeArgs)
{

}

ImClassMemberVarExp::ImClassMemberVarExp(const std::shared_ptr<RClassMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs, bool hasExplicitInstance, const ReExpPtr& explicitInstance)
    : decl(decl), typeArgs(typeArgs), hasExplicitInstance(hasExplicitInstance), explicitInstance(explicitInstance)
{
}

ImStructMemberVarExp::ImStructMemberVarExp(const std::shared_ptr<RStructMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs, bool hasExplicitInstance, const ReExpPtr& explicitInstance)
    : decl(decl), typeArgs(typeArgs), hasExplicitInstance(hasExplicitInstance), explicitInstance(explicitInstance)
{
}

ImEnumElemMemberVarExp::ImEnumElemMemberVarExp(const std::shared_ptr<REnumElemMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs, const ReExpPtr& instance)
    : decl(decl), typeArgs(typeArgs), instance(instance)
{

}

ImListIndexerExp::ImListIndexerExp(ReExpPtr&& instance, RLocPtr&& index, RTypePtr&& itemType)
    : instance(std::move(instance)), index(std::move(index)), itemType(std::move(itemType))
{

}

ImLocalDerefExp::ImLocalDerefExp(const ReExpPtr& target)
    : target(target)
{

}

ImBoxDerefExp::ImBoxDerefExp(const ReExpPtr& target)
    : target(target)
{

}

ImElseExp::ImElseExp(const RExpPtr& exp)
    : exp(exp)
{
}

} // namespace Citron::SyntaxIR0Translator
