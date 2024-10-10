#include "RLoc.h"

#include <Infra/Exceptions.h>

#include "RExp.h"
#include "RLambdaMemberVarDecl.h"
#include "RStructMemberVarDecl.h"
#include "RClassMemberVarDecl.h"
#include "REnumElemMemberVarDecl.h"


namespace Citron {

RLoc_Temp::RLoc_Temp(const RExpPtr& exp)
    : exp(exp)
{
}

RLoc_Temp::RLoc_Temp(RExpPtr&& exp)
    : exp(std::move(exp))
{
}

RTypePtr RLoc_Temp::GetType(RTypeFactory& factory)
{
    return exp->GetType(factory);
}

RLoc_LocalVar::RLoc_LocalVar(const std::string& name, const RTypePtr& declType)
    : name(name), declType(declType)
{

}

RTypePtr RLoc_LocalVar::GetType(RTypeFactory& factory)
{
    return declType;
}

RLoc_LambdaMemberVar::RLoc_LambdaMemberVar(const std::shared_ptr<RLambdaMemberVarDecl>& memberVarDecl, const RTypeArgumentsPtr& typeArgs)
    : memberVarDecl(memberVarDecl), typeArgs(typeArgs)
{
}

RTypePtr RLoc_LambdaMemberVar::GetType(RTypeFactory& factory)
{
    return memberVarDecl->GetDeclType(*typeArgs, factory);
}

RLoc_ListIndexer::RLoc_ListIndexer(RLocPtr&& list, const RLocPtr& index, const RTypePtr& itemType)
    : list(std::move(list)), index(index), itemType(itemType)
{
}

RTypePtr RLoc_ListIndexer::GetType(RTypeFactory& factory)
{
    return itemType;
}

RLoc_StructMember::RLoc_StructMember(const RLocPtr& instance, const std::shared_ptr<RStructMemberVarDecl>& memberVarDecl, const RTypeArgumentsPtr& typeArgs)
    : instance(instance), memberVarDecl(memberVarDecl), typeArgs(typeArgs)
{
}

RTypePtr RLoc_StructMember::GetType(RTypeFactory& factory)
{
    return memberVarDecl->GetDeclType(*typeArgs, factory);

}

RLoc_ClassMember::RLoc_ClassMember(RLocPtr&& instance, const std::shared_ptr<RClassMemberVarDecl>& memberVarDecl, const RTypeArgumentsPtr& typeArgs)
    : instance(std::move(instance)), memberVarDecl(memberVarDecl), typeArgs(typeArgs)
{
}


RTypePtr RLoc_ClassMember::GetType(RTypeFactory& factory)
{
    return memberVarDecl->GetDeclType(*typeArgs, factory);
}

RLoc_EnumElemMember::RLoc_EnumElemMember(const RLocPtr& instance, std::shared_ptr<REnumElemMemberVarDecl>& memberVarDecl, const RTypeArgumentsPtr& typeArgs)
    : instance(instance), memberVarDecl(memberVarDecl), typeArgs(typeArgs)
{
}

RTypePtr RLoc_EnumElemMember::GetType(RTypeFactory& factory)
{
    return memberVarDecl->GetDeclType(*typeArgs, factory);
}

RLoc_This::RLoc_This(RTypePtr type)
    : type(std::move(type))
{
}

RTypePtr RLoc_This::GetType(RTypeFactory& factory)
{
    return type;
}

RLoc_LocalDeref::RLoc_LocalDeref(RLocPtr&& innerLoc)
    : innerLoc(std::move(innerLoc))
{
}

RTypePtr RLoc_LocalDeref::GetType(RTypeFactory& factory)
{
    auto type = innerLoc->GetType(factory);
    
    if (auto* localPtrType = dynamic_cast<RType_LocalPtr*>(type.get()))
        return localPtrType->innerType;

    // 에러, 어떻게 해야할지 생각해본다
    throw NotImplementedException();
}

RLoc_BoxDeref::RLoc_BoxDeref(RLocPtr&& innerLoc)
    : innerLoc(std::move(innerLoc))
{
}

RTypePtr RLoc_BoxDeref::GetType(RTypeFactory& factory)
{
    auto type = innerLoc->GetType(factory);

    if (auto* boxPtrType = dynamic_cast<RType_BoxPtr*>(type.get()))
        return boxPtrType->innerType;

    // 에러, 어떻게 해야할지 생각해본다
    throw NotImplementedException();
}

RLoc_NullableValue::RLoc_NullableValue(const RLocPtr& loc)
    : loc(loc)
{

}

RTypePtr RLoc_NullableValue::GetType(RTypeFactory& factory)
{
    auto type = loc->GetType(factory);

    if (auto* nullableType = dynamic_cast<RType_NullableValue*>(type.get()))
        return nullableType->innerType;

    // 에러, 어떻게 해야할지 생각해본다
    throw NotImplementedException();
}

}