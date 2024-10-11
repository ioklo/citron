#include "RType.h"
#include <vector>

#include "RTypeFactory.h"
#include "RTypeArguments.h"

using namespace std;

namespace Citron {

RType_NullableValue::RType_NullableValue(RTypePtr&& innerType)
    : innerType(std::move(innerType))
{
}

RTypePtr RType_NullableValue::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    auto appliedInnerType = innerType->Apply(typeArgs, factory);
    return factory.MakeNullableValueType(std::move(appliedInnerType));
}

RType_NullableRef::RType_NullableRef(RTypePtr&& innerType)
    : innerType(std::move(innerType))
{
}

RTypePtr RType_NullableRef::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{   
    return factory.MakeNullableRefType(innerType->Apply(typeArgs, factory));
}

RType_TypeVar::RType_TypeVar(int index)
    : index(index)
{
}

RTypePtr RType_TypeVar::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    return typeArgs.Get(index);
}

RType_Void::RType_Void()
{
}

RTypePtr RType_Void::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    return factory.MakeVoidType();
}

RType_Tuple::RType_Tuple(std::vector<RTupleMemberVar>&& memberVars)
    : memberVars(std::move(memberVars))
{
}

RTypePtr RType_Tuple::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    vector<RTupleMemberVar> appliedMemberVars;
    for (auto& memberVar : memberVars)
    {
        auto appliedDeclType = memberVar.declType->Apply(typeArgs, factory);
        appliedMemberVars.push_back(RTupleMemberVar { appliedDeclType, memberVar.name });
    }

    return factory.MakeTupleType(std::move(appliedMemberVars));
}

RType_Func::RType_Func(bool bLocal, RTypePtr&& retType, std::vector<Parameter>&& params)
    : bLocal(bLocal), retType(std::move(retType)), params(std::move(params))
{
}

RTypePtr RType_Func::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    auto appliedRetType = retType->Apply(typeArgs, factory);

    vector<RType_Func::Parameter> appliedParams;
    appliedParams.reserve(params.size());

    for (auto& param : params)
    {
        auto appliedParamType = param.type->Apply(typeArgs, factory);
        appliedParams.emplace_back(param.bOut, std::move(appliedParamType));
    }

    return factory.MakeFuncType(bLocal, std::move(appliedRetType), std::move(appliedParams));
}

RType_Func::Parameter::Parameter(bool bOut, RTypePtr&& type)
    : bOut(bOut), type(std::move(type))
{

}

RType_LocalPtr::RType_LocalPtr(RTypePtr&& innerType)
    : innerType(std::move(innerType))
{
}

RTypePtr RType_LocalPtr::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    auto appliedInnerType = innerType->Apply(typeArgs, factory);
    return factory.MakeLocalPtrType(std::move(appliedInnerType));
}


RType_BoxPtr::RType_BoxPtr(const RTypePtr& innerType)
    : innerType(innerType)
{

}

RTypePtr RType_BoxPtr::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    auto appliedInnerType = innerType->Apply(typeArgs, factory);
    return factory.MakeBoxPtrType(std::move(appliedInnerType));
}

RType_Class::RType_Class(const std::shared_ptr<RClassDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : decl(decl), typeArgs(typeArgs)
{
}

RTypePtr RType_Class::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    auto appliedTypeArgs = this->typeArgs->Apply(typeArgs, factory);
    return factory.MakeClassType(decl, std::move(appliedTypeArgs));
}

RType_Struct::RType_Struct(const std::shared_ptr<RStructDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : decl(decl), typeArgs(typeArgs)
{
}

RTypePtr RType_Struct::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    auto appliedTypeArgs = this->typeArgs->Apply(typeArgs, factory);
    return factory.MakeStructType(decl, std::move(appliedTypeArgs));
}

RType_Enum::RType_Enum(const std::shared_ptr<REnumDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : decl(decl), typeArgs(typeArgs)
{   
}

RTypePtr RType_Enum::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    auto appliedTypeArgs = this->typeArgs->Apply(typeArgs, factory);
    return factory.MakeEnumType(decl, std::move(appliedTypeArgs));
}

RType_EnumElem::RType_EnumElem(const std::shared_ptr<REnumElemDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : decl(decl), typeArgs(typeArgs)
{
}

RTypePtr RType_EnumElem::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    auto appliedTypeArgs = this->typeArgs->Apply(typeArgs, factory);
    return factory.MakeEnumElemType(decl, std::move(appliedTypeArgs));
}

RType_Interface::RType_Interface(const std::shared_ptr<RInterfaceDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : decl(decl), typeArgs(typeArgs)
{
}

RTypePtr RType_Interface::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    auto appliedTypeArgs = this->typeArgs->Apply(typeArgs, factory);
    return factory.MakeInterfaceType(decl, std::move(appliedTypeArgs));
}

RType_Lambda::RType_Lambda(const std::shared_ptr<RLambdaDecl>& decl, const RTypeArgumentsPtr& outerTypeArgs)
    : decl(decl), outerTypeArgs(outerTypeArgs)
{
}

RTypePtr RType_Lambda::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    auto appliedOuterTypeArgs = outerTypeArgs->Apply(typeArgs, factory);
    return factory.MakeLambdaType(decl, std::move(appliedOuterTypeArgs));
}

} // Citron