#include "RTypes.h"
#include <vector>

#include "Infra/Exceptions.h"

#include "RTypeFactory.h"
#include "RTypeArguments.h"
#include "RStructDecl.h"
#include "RClassDecl.h"
#include "REnumDecl.h"
#include "REnumElemDecl.h"
#include "RLambdaDecl.h"

using namespace std;

namespace Citron {

RType_NullableValue::RType_NullableValue(RType* innerType)
    : innerType(innerType)
{
}

RType* RType_NullableValue::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    auto* appliedInnerType = innerType->Apply(typeArgs, factory);
    return factory.MakeNullableValueType(appliedInnerType);
}

optional<RMember> RType_NullableValue::GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount)
{
    // 사용자가 검색해서 쓸 수 있는 멤버는 없다
    return nullopt;
}

RType_NullableRef::RType_NullableRef(RType* innerType)
    : innerType(innerType)
{
}

RType* RType_NullableRef::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{   
    return factory.MakeNullableRefType(innerType->Apply(typeArgs, factory));
}

optional<RMember> RType_NullableRef::GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount)
{
    return nullopt;
}

RType_TypeVar::RType_TypeVar(int index)
    : index(index)
{
}

RType* RType_TypeVar::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    return typeArgs.Get(index);
}

optional<RMember> RType_TypeVar::GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount)
{
    return nullopt;
}

RType_Void::RType_Void()
{
}

RType* RType_Void::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    return factory.MakeVoidType();
}

optional<RMember> RType_Void::GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount)
{
    return nullopt;
}

RType_Tuple::RType_Tuple(std::vector<RTupleVar>&& vars)
    : vars(move(vars))
{
}

RType* RType_Tuple::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    vector<RTupleVar> appliedVars;
    for (auto& var : vars)
    {
        auto appliedDeclType = var.declType->Apply(typeArgs, factory);
        appliedVars.push_back(RTupleVar { appliedDeclType, var.name });
    }

    return factory.MakeTupleType(move(appliedVars));
}

optional<RMember> RType_Tuple::GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount)
{
    throw NotImplementedException();
}

RType_Func::RType_Func(bool bLocal, RType* retType, std::vector<Parameter>&& params)
    : bLocal(bLocal), retType(retType), params(move(params))
{
}

RType* RType_Func::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    auto* appliedRetType = retType->Apply(typeArgs, factory);

    vector<RType_Func::Parameter> appliedParams;
    appliedParams.reserve(params.size());

    for (auto& param : params)
    {
        auto* appliedParamType = param.type->Apply(typeArgs, factory);
        appliedParams.emplace_back(param.bOut, appliedParamType);
    }

    return factory.MakeFuncType(bLocal, appliedRetType, move(appliedParams));
}

optional<RMember> RType_Func::GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount)
{
    return nullopt;
}

RType_Func::Parameter::Parameter(bool bOut, RType* type)
    : bOut(bOut), type(type)
{

}

RType_LocalPtr::RType_LocalPtr(RType* innerType)
    : innerType(innerType)
{
}

RType* RType_LocalPtr::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    auto* appliedInnerType = innerType->Apply(typeArgs, factory);
    return factory.MakeLocalPtrType(appliedInnerType);
}


optional<RMember> RType_LocalPtr::GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount)
{
    return nullopt;
}

RType_BoxPtr::RType_BoxPtr(RType* innerType)
    : innerType(innerType)
{

}

RType* RType_BoxPtr::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    auto* appliedInnerType = innerType->Apply(typeArgs, factory);
    return factory.MakeBoxPtrType(appliedInnerType);
}

optional<RMember> RType_BoxPtr::GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount)
{
    return nullopt;
}

RType_Class::RType_Class(RClassDecl* decl, RTypeArguments* typeArgs)
    : decl(decl), typeArgs(typeArgs)
{
}

std::optional<RMember_ClassVar> RType_Class::GetVar(const RName& name)
{
    return decl->GetVar(typeArgs, name);
}

bool RType_Class::IsBaseOf(RType_Class& derivedClass)
{
    throw NotImplementedException();
}

RType* RType_Class::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    auto* appliedTypeArgs = this->typeArgs->Apply(typeArgs, factory);
    return factory.MakeClassType(decl, appliedTypeArgs);
}

optional<RMember> RType_Class::GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount)
{
    return decl->GetMember(typeArgs, name, explicitTypeArgsExceptOuterCount);
}

RType_Struct::RType_Struct(RStructDecl* decl, RTypeArguments* typeArgs)
    : decl(decl), typeArgs(typeArgs)
{
}

std::optional<RMember_StructVar> RType_Struct::GetVar(const RName& name)
{
    return decl->GetVar(typeArgs, name);
}

RStructCtorDecl* RType_Struct::GetUnboundTrivialCtor()
{
    return decl->GetUnboundTrivialCtor_RStructCtorDecl();
}

RType* RType_Struct::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    auto* appliedTypeArgs = this->typeArgs->Apply(typeArgs, factory);
    return factory.MakeStructType(decl, appliedTypeArgs);
}

optional<RMember> RType_Struct::GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount)
{
    return decl->GetMember(typeArgs, name, explicitTypeArgsExceptOuterCount);
}

RType_Enum::RType_Enum(REnumDecl* decl, RTypeArguments* typeArgs)
    : decl(decl), typeArgs(typeArgs)
{   
}

RType* RType_Enum::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    auto* appliedTypeArgs = this->typeArgs->Apply(typeArgs, factory);
    return factory.MakeEnumType(decl, appliedTypeArgs);
}

optional<RMember> RType_Enum::GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount)
{
    return decl->GetMember(typeArgs, name, explicitTypeArgsExceptOuterCount);
}

RType_EnumElem::RType_EnumElem(REnumElemDecl* decl, RTypeArguments* typeArgs)
    : decl(decl), typeArgs(typeArgs)
{
}

std::optional<RMember_EnumElemVar> RType_EnumElem::GetVar(const RName& name)
{
    return decl->GetVar(typeArgs, name);
}

RType_Enum* RType_EnumElem::GetBaseEnumType(RTypeFactory& factory)
{
    auto enumDecl = decl->GetBaseEnumDecl();

    // enumElem은 typeArgs를 추가로 받지 않기 때문에 그냥 써도 괜찮을 것 같다
    return factory.MakeEnumType(enumDecl, typeArgs);
}


RType* RType_EnumElem::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    auto* appliedTypeArgs = this->typeArgs->Apply(typeArgs, factory);
    return factory.MakeEnumElemType(decl, appliedTypeArgs);
}

optional<RMember> RType_EnumElem::GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount)
{
    return decl->GetMember(typeArgs, name, explicitTypeArgsExceptOuterCount);
}

RType_Interface::RType_Interface(RInterfaceDecl* decl, RTypeArguments* typeArgs, bool bLocal)
    : decl(decl), typeArgs(typeArgs), bLocal(bLocal)
{
}

RType* RType_Interface::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    auto* appliedTypeArgs = this->typeArgs->Apply(typeArgs, factory);
    return factory.MakeInterfaceType(decl, appliedTypeArgs, bLocal);
}

optional<RMember> RType_Interface::GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount)
{
    throw NotImplementedException();
}

RType_Lambda::RType_Lambda(RLambdaDecl* decl, RTypeArguments* outerTypeArgs)
    : decl(decl), outerTypeArgs(outerTypeArgs)
{
}

vector<RFuncParameter> RType_Lambda::GetPartiallyBoundParameters()
{
    throw NotImplementedException();
}

RType* RType_Lambda::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    auto* appliedOuterTypeArgs = outerTypeArgs->Apply(typeArgs, factory);
    return factory.MakeLambdaType(decl, appliedOuterTypeArgs);
}

optional<RMember> RType_Lambda::GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount)
{
    return decl->GetMember(outerTypeArgs, name, explicitTypeArgsExceptOuterCount);
}

} // Citron