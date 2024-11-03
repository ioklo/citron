#include "NLoc.h"

#include <Infra/Exceptions.h>

#include "NExp.h"
#include "RLambdaMemberVarDecl.h"
#include "RStructMemberVarDecl.h"
#include "RClassMemberVarDecl.h"
#include "REnumElemMemberVarDecl.h"


namespace Citron {

NLoc_Temp::NLoc_Temp(const NExpPtr& exp)
    : exp(exp)
{
}

NLoc_Temp::NLoc_Temp(NExpPtr&& exp)
    : exp(std::move(exp))
{
}

RTypePtr NLoc_Temp::GetType(RTypeFactory& factory)
{
    return exp->GetType(factory);
}

NLoc_LocalVar::NLoc_LocalVar(const RName& name, const RTypePtr& declType)
    : name(name), declType(declType)
{

}

RTypePtr NLoc_LocalVar::GetType(RTypeFactory& factory)
{
    return declType;
}

NLoc_LambdaMemberVar::NLoc_LambdaMemberVar(const std::shared_ptr<RLambdaMemberVarDecl>& memberVarDecl, const RTypeArgumentsPtr& typeArgs)
    : memberVarDecl(memberVarDecl), typeArgs(typeArgs)
{
}

RTypePtr NLoc_LambdaMemberVar::GetType(RTypeFactory& factory)
{
    return memberVarDecl->GetDeclType(*typeArgs, factory);
}

NLoc_ListIndexer::NLoc_ListIndexer(NLocPtr&& list, const NLocPtr& index, const RTypePtr& itemType)
    : list(std::move(list)), index(index), itemType(itemType)
{
}

RTypePtr NLoc_ListIndexer::GetType(RTypeFactory& factory)
{
    return itemType;
}

NLoc_StructMember::NLoc_StructMember(const NLocPtr& instance, const std::shared_ptr<RStructMemberVarDecl>& memberVarDecl, const RTypeArgumentsPtr& typeArgs)
    : instance(instance), memberVarDecl(memberVarDecl), typeArgs(typeArgs)
{
}

RTypePtr NLoc_StructMember::GetType(RTypeFactory& factory)
{
    return memberVarDecl->GetDeclType(*typeArgs, factory);

}

NLoc_ClassMember::NLoc_ClassMember(NLocPtr&& instance, const std::shared_ptr<RClassMemberVarDecl>& memberVarDecl, const RTypeArgumentsPtr& typeArgs)
    : instance(std::move(instance)), memberVarDecl(memberVarDecl), typeArgs(typeArgs)
{
}


RTypePtr NLoc_ClassMember::GetType(RTypeFactory& factory)
{
    return memberVarDecl->GetDeclType(*typeArgs, factory);
}

NLoc_EnumElemMember::NLoc_EnumElemMember(const NLocPtr& instance, std::shared_ptr<REnumElemMemberVarDecl>& memberVarDecl, const RTypeArgumentsPtr& typeArgs)
    : instance(instance), memberVarDecl(memberVarDecl), typeArgs(typeArgs)
{
}

RTypePtr NLoc_EnumElemMember::GetType(RTypeFactory& factory)
{
    return memberVarDecl->GetDeclType(*typeArgs, factory);
}

NLoc_This::NLoc_This(RTypePtr type)
    : type(std::move(type))
{
}

RTypePtr NLoc_This::GetType(RTypeFactory& factory)
{
    return type;
}

NLoc_LocalDeref::NLoc_LocalDeref(NLocPtr&& innerLoc)
    : innerLoc(std::move(innerLoc))
{
}

RTypePtr NLoc_LocalDeref::GetType(RTypeFactory& factory)
{
    auto type = innerLoc->GetType(factory);
    
    if (auto* localPtrType = dynamic_cast<RType_LocalPtr*>(type.get()))
        return localPtrType->innerType;

    // 에러, 어떻게 해야할지 생각해본다
    throw NotImplementedException();
}

NLoc_BoxDeref::NLoc_BoxDeref(NLocPtr&& innerLoc)
    : innerLoc(std::move(innerLoc))
{
}

RTypePtr NLoc_BoxDeref::GetType(RTypeFactory& factory)
{
    auto type = innerLoc->GetType(factory);

    if (auto* boxPtrType = dynamic_cast<RType_BoxPtr*>(type.get()))
        return boxPtrType->innerType;

    // 에러, 어떻게 해야할지 생각해본다
    throw NotImplementedException();
}

NLoc_NullableValue::NLoc_NullableValue(const NLocPtr& loc)
    : loc(loc)
{

}

RTypePtr NLoc_NullableValue::GetType(RTypeFactory& factory)
{
    auto type = loc->GetType(factory);

    if (auto* nullableType = dynamic_cast<RType_NullableValue*>(type.get()))
        return nullableType->innerType;

    // 에러, 어떻게 해야할지 생각해본다
    throw NotImplementedException();
}

}