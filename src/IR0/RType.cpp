#include "RType.h"
#include <vector>

#include "RTypeFactory.h"
#include "RTypeArguments.h"

using namespace std;

namespace Citron {

RNullableValueType::RNullableValueType(RTypePtr&& innerType)
    : innerType(std::move(innerType))
{
}

RTypePtr RNullableValueType::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    auto appliedInnerType = innerType->Apply(typeArgs, factory);
    return factory.MakeNullableValueType(std::move(appliedInnerType));
}

RNullableRefType::RNullableRefType(RTypePtr&& innerType)
    : innerType(std::move(innerType))
{
}

RTypePtr RNullableRefType::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{   
    return factory.MakeNullableRefType(innerType->Apply(typeArgs, factory));
}

RTypeVarType::RTypeVarType(int index)
    : index(index)
{
}

RTypePtr RTypeVarType::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    return typeArgs.Get(index);
}

RVoidType::RVoidType()
{
}

RTypePtr RVoidType::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    return factory.MakeVoidType();
}

RTupleType::RTupleType(std::vector<RTupleMemberVar>&& memberVars)
    : memberVars(std::move(memberVars))
{
}

RTypePtr RTupleType::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    vector<RTupleMemberVar> appliedMemberVars;
    for (auto& memberVar : memberVars)
    {
        auto appliedDeclType = memberVar.declType->Apply(typeArgs, factory);
        appliedMemberVars.push_back(RTupleMemberVar { appliedDeclType, memberVar.name });
    }

    return factory.MakeTupleType(std::move(appliedMemberVars));
}

RFuncType::RFuncType(bool bLocal, RTypePtr&& retType, std::vector<Parameter>&& params)
    : bLocal(bLocal), retType(std::move(retType)), params(std::move(params))
{
}

RTypePtr RFuncType::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    auto appliedRetType = retType->Apply(typeArgs, factory);

    vector<RFuncType::Parameter> appliedParams;
    appliedParams.reserve(params.size());

    for (auto& param : params)
    {
        auto appliedParamType = param.type->Apply(typeArgs, factory);
        appliedParams.emplace_back(param.bOut, std::move(appliedParamType));
    }

    return factory.MakeFuncType(bLocal, std::move(appliedRetType), std::move(appliedParams));
}

RFuncType::Parameter::Parameter(bool bOut, RTypePtr&& type)
    : bOut(bOut), type(std::move(type))
{

}

RLocalPtrType::RLocalPtrType(RTypePtr&& innerType)
    : innerType(std::move(innerType))
{
}

RTypePtr RLocalPtrType::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    auto appliedInnerType = innerType->Apply(typeArgs, factory);
    return factory.MakeLocalPtrType(std::move(appliedInnerType));
}


RBoxPtrType::RBoxPtrType(const RTypePtr& innerType)
    : innerType(innerType)
{

}

RTypePtr RBoxPtrType::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    auto appliedInnerType = innerType->Apply(typeArgs, factory);
    return factory.MakeBoxPtrType(std::move(appliedInnerType));
}

RInstanceType::RInstanceType(const RDeclIdPtr& declId, const RTypeArgumentsPtr& typeArgs)
    : declId(declId), typeArgs(typeArgs)
{
}

RTypePtr RInstanceType::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    auto appliedTypeArgs = this->typeArgs->Apply(typeArgs, factory);
    return factory.MakeInstanceType(declId, std::move(appliedTypeArgs));
}

} // Citron