#include "RLoc.h"

#include <Infra/NotImplementedException.h>

#include "RExp.h"
#include "RLambdaMemberVarDecl.h"
#include "RStructMemberVarDecl.h"
#include "RClassMemberVarDecl.h"
#include "REnumElemMemberVarDecl.h"


namespace Citron {

RTempLoc::RTempLoc(const RExpPtr& exp)
    : exp(exp)
{
}

RTypePtr RTempLoc::GetType(RTypeFactory& factory)
{
    return exp->GetType(factory);
}

RLocalVarLoc::RLocalVarLoc(const RName& name, const RTypePtr& declType)
    : name(name), declType(declType)
{

}

RTypePtr RLocalVarLoc::GetType(RTypeFactory& factory)
{
    return declType;
}

RLambdaMemberVarLoc::RLambdaMemberVarLoc(const std::shared_ptr<RLambdaMemberVarDecl>& memberVarDecl, const RTypeArgumentsPtr& typeArgs)
    : memberVarDecl(memberVarDecl), typeArgs(typeArgs)
{
}

RTypePtr RLambdaMemberVarLoc::GetType(RTypeFactory& factory)
{
    return memberVarDecl->GetDeclType(*typeArgs, factory);
}

RListIndexerLoc::RListIndexerLoc(const RLocPtr& list, const RLocPtr& index, const RTypePtr& itemType)
    : list(list), index(index), itemType(itemType)
{
}

RTypePtr RListIndexerLoc::GetType(RTypeFactory& factory)
{
    return itemType;
}

RStructMemberLoc::RStructMemberLoc(const RLocPtr& instance, const std::shared_ptr<RStructMemberVarDecl>& memberVarDecl, const RTypeArgumentsPtr& typeArgs)
    : instance(instance), memberVarDecl(memberVarDecl), typeArgs(typeArgs)
{
}

RTypePtr RStructMemberLoc::GetType(RTypeFactory& factory)
{
    return memberVarDecl->GetDeclType(*typeArgs, factory);

}

RClassMemberLoc::RClassMemberLoc(const RLocPtr& instance, const std::shared_ptr<RClassMemberVarDecl>& memberVarDecl, const RTypeArgumentsPtr& typeArgs)
    : instance(instance), memberVarDecl(memberVarDecl), typeArgs(typeArgs)
{
}


RTypePtr RClassMemberLoc::GetType(RTypeFactory& factory)
{
    return memberVarDecl->GetDeclType(*typeArgs, factory);
}

REnumElemMemberLoc::REnumElemMemberLoc(const RLocPtr& instance, std::shared_ptr<REnumElemMemberVarDecl>& memberVarDecl, const RTypeArgumentsPtr& typeArgs)
    : instance(instance), memberVarDecl(memberVarDecl), typeArgs(typeArgs)
{
}

RTypePtr REnumElemMemberLoc::GetType(RTypeFactory& factory)
{
    return memberVarDecl->GetDeclType(*typeArgs, factory);
}

RThisLoc::RThisLoc(const RTypePtr& type)
    : type(type)
{
}

RTypePtr RThisLoc::GetType(RTypeFactory& factory)
{
    return type;
}

RLocalDerefLoc::RLocalDerefLoc(const RLocPtr& innerLoc)
    : innerLoc(innerLoc)
{
}

RTypePtr RLocalDerefLoc::GetType(RTypeFactory& factory)
{
    auto type = innerLoc->GetType(factory);
    
    if (auto* localPtrType = dynamic_cast<RLocalPtrType*>(type.get()))
        return localPtrType->innerType;

    // 에러, 어떻게 해야할지 생각해본다
    throw NotImplementedException();
}

RBoxDerefLoc::RBoxDerefLoc(const RLocPtr& innerLoc)
    : innerLoc(innerLoc)
{
}

RTypePtr RBoxDerefLoc::GetType(RTypeFactory& factory)
{
    auto type = innerLoc->GetType(factory);

    if (auto* boxPtrType = dynamic_cast<RBoxPtrType*>(type.get()))
        return boxPtrType->innerType;

    // 에러, 어떻게 해야할지 생각해본다
    throw NotImplementedException();
}

RNullableValueLoc::RNullableValueLoc(const RLocPtr& loc)
    : loc(loc)
{

}

RTypePtr RNullableValueLoc::GetType(RTypeFactory& factory)
{
    auto type = loc->GetType(factory);

    if (auto* nullableType = dynamic_cast<RNullableValueType*>(type.get()))
        return nullableType->innerType;

    // 에러, 어떻게 해야할지 생각해본다
    throw NotImplementedException();
}

}