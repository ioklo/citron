#include "pch.h"
#include "ReExp.h"
#include <IR0/RLambdaMemberVarDecl.h>

namespace Citron::SyntaxIR0Translator {

ReThisVarExp::ReThisVarExp(const RTypePtr& type)
    : type(type)
{
}

ReLocalVarExp::ReLocalVarExp(const RTypePtr& type, const std::string& name)
    : type(type), name(name)
{
}

ReLambdaMemberVarExp::ReLambdaMemberVarExp(const std::shared_ptr<RLambdaMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : decl(decl), typeArgs(typeArgs)
{
}

RTypePtr ReLambdaMemberVarExp::GetType()
{
    return decl->GetDeclType(typeArgs);
}

ReClassMemberVarExp::ReClassMemberVarExp(const std::shared_ptr<RClassMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs, bool hasExplicitInstance, const ReExpPtr& explicitInstance)
    : decl(decl), typeArgs(typeArgs), hasExplicitInstance(hasExplicitInstance), explicitInstance(explicitInstance)
{
}

//RTypePtr ReClassMemberVarExp::GetType()
//{
//    return decl->GetDeclType(typeArgs);
//}

ReStructMemberVarExp::ReStructMemberVarExp(const std::shared_ptr<RStructMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs, bool hasExplicitInstance, const ReExpPtr& explicitInstance)
    : decl(decl), typeArgs(typeArgs), hasExplicitInstance(hasExplicitInstance), explicitInstance(explicitInstance)
{
}

//RTypePtr ReStructMemberVarExp::GetType()
//{
//    return decl->GetDeclType(typeArgs);
//}
//

ReEnumElemMemberVarExp::ReEnumElemMemberVarExp(const std::shared_ptr<REnumElemMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs, const ReExpPtr& instance)
    : decl(decl), typeArgs(typeArgs), instance(instance)
{
}

//RTypePtr ReEnumElemMemberVarExp::GetType()
//{
//    return decl->GetDeclType(typeArgs);
//}

ReLocalDerefExp::ReLocalDerefExp(const ReExpPtr& target)
    : target(target)
{

}

//RTypePtr ReLocalDerefExp::GetType()
//{
//    return ((RLocalPtrType*)target->GetType().get())->GetInnerType(); // TODO: remove reinterpret cast
//}

ReBoxDerefExp::ReBoxDerefExp(const ReExpPtr& target)
    : target(target)
{

}

//RTypePtr ReBoxDerefExp::GetType()
//{
//    return ((RBoxPtrType*)target->GetType().get())->GetInnerType(); // TODO: remove reinterpret cast
//}


ReListIndexerExp::ReListIndexerExp(const ReExpPtr& instance, const RLocPtr& index, const RTypePtr& itemType)
    : instance(instance), index(index), itemType(itemType)
{

}


ReElseExp::ReElseExp(const RExpPtr& rExp)
    : rExp(rExp)
{
}

//RTypePtr ReElseExp::GetType()
//{
//    return rExp->GetType();
//}



}