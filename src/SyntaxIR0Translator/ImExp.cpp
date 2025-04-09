module Citron.SyntaxIR0Translator:ImExp;

namespace Citron::SyntaxIR0Translator {

ImExp_Namespace::ImExp_Namespace(const std::shared_ptr<RNamespaceDecl>& _namespace)
    : _namespace(_namespace)
{
}

ImExp_GlobalFuncs::ImExp_GlobalFuncs(const std::vector<DeclWithOuterTypeArgs<RGlobalFuncDecl>>& items, const std::shared_ptr<RTypeArguments>& partialTypeArgs)
    : FuncsWithPartialTypeArgsComponent<RGlobalFuncDecl>(items, partialTypeArgs)
{
}

ImExp_TypeVar::ImExp_TypeVar(std::shared_ptr<RType_TypeVar>&& type)
    : type(move(type))
{
}

ImExp_Class::ImExp_Class(const std::shared_ptr<RClassDecl>& classDecl, RTypeArgumentsPtr&& typeArgs)
    : classDecl(classDecl), typeArgs(move(typeArgs))
{
}

ImExp_ClassFuncs::ImExp_ClassFuncs(const std::vector<DeclWithOuterTypeArgs<RClassFuncDecl>>& items, const std::shared_ptr<RTypeArguments>& partialTypeArgsExceptOuter, bool hasExplicitInstance, const ReExpPtr& explicitInstance)
    : FuncsWithPartialTypeArgsComponent<RClassFuncDecl>(items, partialTypeArgsExceptOuter), hasExplicitInstance(hasExplicitInstance), explicitInstance(explicitInstance)
{
}

ImExp_Struct::ImExp_Struct(const std::shared_ptr<RStructDecl>& structDecl, RTypeArgumentsPtr&& typeArgs)
    : structDecl(structDecl), typeArgs(move(typeArgs))
{
}

ImExp_StructFuncs::ImExp_StructFuncs(const std::vector<DeclWithOuterTypeArgs<RStructFuncDecl>>& items, const std::shared_ptr<RTypeArguments>& partialTypeArgsExceptOuter, bool hasExplicitInstance, const ReExpPtr& explicitInstance)
    : FuncsWithPartialTypeArgsComponent<RStructFuncDecl>(items, partialTypeArgsExceptOuter), hasExplicitInstance(hasExplicitInstance), explicitInstance(explicitInstance)
{

}

ImExp_Enum::ImExp_Enum(const std::shared_ptr<REnumDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : decl(decl), typeArgs(typeArgs)
{

}

ImExp_EnumElem::ImExp_EnumElem(const std::shared_ptr<REnumElemDecl>& decl, const RTypeArgumentsPtr& typeArgs)
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

ImExp_LambdaVar::ImExp_LambdaVar(const std::shared_ptr<NLambdaVarDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : decl(decl), typeArgs(typeArgs)
{

}

ImExp_ClassVar::ImExp_ClassVar(const std::shared_ptr<RClassVarDecl>& decl, const RTypeArgumentsPtr& typeArgs, bool hasExplicitInstance, const ReExpPtr& explicitInstance)
    : decl(decl), typeArgs(typeArgs), hasExplicitInstance(hasExplicitInstance), explicitInstance(explicitInstance)
{
}

ImExp_StructVar::ImExp_StructVar(const std::shared_ptr<RStructVarDecl>& decl, const RTypeArgumentsPtr& typeArgs, bool hasExplicitInstance, const ReExpPtr& explicitInstance)
    : decl(decl), typeArgs(typeArgs), hasExplicitInstance(hasExplicitInstance), explicitInstance(explicitInstance)
{
}

ImExp_EnumElemVar::ImExp_EnumElemVar(const std::shared_ptr<REnumElemVarDecl>& decl, const RTypeArgumentsPtr& typeArgs, const ReExpPtr& instance)
    : decl(decl), typeArgs(typeArgs), instance(instance)
{

}

ImExp_ListIndexer::ImExp_ListIndexer(ReExpPtr&& instance, NLocPtr&& index, RTypePtr&& itemType)
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

ImExp_Else::ImExp_Else(const NExpPtr& exp)
    : exp(exp)
{
}

} // namespace Citron::SyntaxIR0Translator
