module Citron.SyntaxIR0Translator:ReExp;

import Citron.NDecls;

namespace Citron::SyntaxIR0Translator {

ReExp_ThisVar::ReExp_ThisVar(const RTypePtr& type)
    : type(type)
{
}

ReExp_LocalVar::ReExp_LocalVar(const RTypePtr& type, const std::string& name)
    : type(type), name(name)
{
}

ReExp_LambdaVar::ReExp_LambdaVar(const std::shared_ptr<NLambdaVarDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : decl(decl), typeArgs(typeArgs)
{
}

RTypePtr ReExp_LambdaVar::GetType(RTypeFactory& factory)
{
    return decl->GetDeclType(*typeArgs, factory);
}

ReExp_ClassVar::ReExp_ClassVar(const std::shared_ptr<RClassVarDecl>& decl, const RTypeArgumentsPtr& typeArgs, bool hasExplicitInstance, const ReExpPtr& explicitInstance)
    : decl(decl), typeArgs(typeArgs), hasExplicitInstance(hasExplicitInstance), explicitInstance(explicitInstance)
{
}

//RTypePtr ReExp_ClassVar::GetType(RTypeFactory& factory)
//{
//    return decl->GetDeclType(typeArgs);
//}

ReExp_StructVar::ReExp_StructVar(const std::shared_ptr<RStructVarDecl>& decl, const RTypeArgumentsPtr& typeArgs, bool hasExplicitInstance, const ReExpPtr& explicitInstance)
    : decl(decl), typeArgs(typeArgs), hasExplicitInstance(hasExplicitInstance), explicitInstance(explicitInstance)
{
}

//RTypePtr ReExp_StructVar::GetType(RTypeFactory& factory)
//{
//    return decl->GetDeclType(typeArgs);
//}
//

ReExp_EnumElemVar::ReExp_EnumElemVar(const std::shared_ptr<REnumElemVarDecl>& decl, const RTypeArgumentsPtr& typeArgs, const ReExpPtr& instance)
    : decl(decl), typeArgs(typeArgs), instance(instance)
{
}

//RTypePtr ReEnumElemMemberVarExp::GetType(RTypeFactory& factory)
//{
//    return decl->GetDeclType(typeArgs);
//}

ReExp_LocalDeref::ReExp_LocalDeref(const ReExpPtr& target)
    : target(target)
{

}

//RTypePtr ReLocalDerefExp::GetType(RTypeFactory& factory)
//{
//    return ((RLocalPtrType*)target->GetType().get())->GetInnerType(); // TODO: remove reinterpret cast
//}

ReExp_BoxDeref::ReExp_BoxDeref(const ReExpPtr& target)
    : target(target)
{

}

//RTypePtr ReBoxDerefExp::GetType(RTypeFactory& factory)
//{
//    return ((RBoxPtrType*)target->GetType().get())->GetInnerType(); // TODO: remove reinterpret cast
//}


ReExp_ListIndexer::ReExp_ListIndexer(const ReExpPtr& instance, const ReExpPtr& index, const RTypePtr& itemType)
    : instance(instance), index(index), itemType(itemType)
{

}


ReExp_Else::ReExp_Else(const NExpPtr& nExp)
    : nExp(nExp)
{
}

//RTypePtr ReElseExp::GetType(RTypeFactory& factory)
//{
//    return rExp->GetType();
//}



}