#include "pch.h"
#include "ReExp.h"
#include <IR0/RLambdaMemberVarDecl.h>

namespace Citron::SyntaxIR0Translator {

ReExp_ThisVar::ReExp_ThisVar(const RTypePtr& type)
    : type(type)
{
}

ReExp_LocalVar::ReExp_LocalVar(const RTypePtr& type, const std::string& name)
    : type(type), name(name)
{
}

ReExp_LambdaMemberVar::ReExp_LambdaMemberVar(const std::shared_ptr<RLambdaMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : decl(decl), typeArgs(typeArgs)
{
}

RTypePtr ReExp_LambdaMemberVar::GetType(RTypeFactory& factory)
{
    return decl->GetDeclType(*typeArgs, factory);
}

ReExp_ClassMemberVar::ReExp_ClassMemberVar(const std::shared_ptr<RClassMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs, bool hasExplicitInstance, const ReExpPtr& explicitInstance)
    : decl(decl), typeArgs(typeArgs), hasExplicitInstance(hasExplicitInstance), explicitInstance(explicitInstance)
{
}

//RTypePtr ReClassMemberVarExp::GetType(RTypeFactory& factory)
//{
//    return decl->GetDeclType(typeArgs);
//}

ReExp_StructMemberVar::ReExp_StructMemberVar(const std::shared_ptr<RStructMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs, bool hasExplicitInstance, const ReExpPtr& explicitInstance)
    : decl(decl), typeArgs(typeArgs), hasExplicitInstance(hasExplicitInstance), explicitInstance(explicitInstance)
{
}

//RTypePtr ReStructMemberVarExp::GetType(RTypeFactory& factory)
//{
//    return decl->GetDeclType(typeArgs);
//}
//

ReExp_EnumElemMemberVar::ReExp_EnumElemMemberVar(const std::shared_ptr<REnumElemMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs, const ReExpPtr& instance)
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


ReExp_ListIndexer::ReExp_ListIndexer(const ReExpPtr& instance, const RLocPtr& index, const RTypePtr& itemType)
    : instance(instance), index(index), itemType(itemType)
{

}


ReExp_Else::ReExp_Else(const RExpPtr& rExp)
    : rExp(rExp)
{
}

//RTypePtr ReElseExp::GetType(RTypeFactory& factory)
//{
//    return rExp->GetType();
//}



}