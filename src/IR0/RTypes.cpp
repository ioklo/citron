module Citron.RDecls:RTypes;
import <vector>;

import Citron.Exceptions;

import :RTypeFactory;
import :RTypeArguments;
import :RStructDecl;
import :RClassDecl;
import :REnumDecl;
import :REnumElemDecl;
import :RLambdaDecl;

using namespace std;

namespace Citron {

RType_NullableValue::RType_NullableValue(RTypePtr&& innerType)
    : innerType(move(innerType))
{
}

RTypePtr RType_NullableValue::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    auto appliedInnerType = innerType->Apply(typeArgs, factory);
    return factory.MakeNullableValueType(move(appliedInnerType));
}

optional<RMember> RType_NullableValue::GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount)
{
    // 사용자가 검색해서 쓸 수 있는 멤버는 없다
    return nullopt;
}

RType_NullableRef::RType_NullableRef(RTypePtr&& innerType)
    : innerType(move(innerType))
{
}

RTypePtr RType_NullableRef::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
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

RTypePtr RType_TypeVar::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
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

RTypePtr RType_Void::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
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

RTypePtr RType_Tuple::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
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

RType_Func::RType_Func(bool bLocal, RTypePtr&& retType, std::vector<Parameter>&& params)
    : bLocal(bLocal), retType(move(retType)), params(move(params))
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
        appliedParams.emplace_back(param.bOut, move(appliedParamType));
    }

    return factory.MakeFuncType(bLocal, move(appliedRetType), move(appliedParams));
}

optional<RMember> RType_Func::GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount)
{
    return nullopt;
}

RType_Func::Parameter::Parameter(bool bOut, RTypePtr&& type)
    : bOut(bOut), type(move(type))
{

}

RType_LocalPtr::RType_LocalPtr(RTypePtr&& innerType)
    : innerType(move(innerType))
{
}

RTypePtr RType_LocalPtr::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    auto appliedInnerType = innerType->Apply(typeArgs, factory);
    return factory.MakeLocalPtrType(move(appliedInnerType));
}


optional<RMember> RType_LocalPtr::GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount)
{
    return nullopt;
}

RType_BoxPtr::RType_BoxPtr(const RTypePtr& innerType)
    : innerType(innerType)
{

}

RTypePtr RType_BoxPtr::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    auto appliedInnerType = innerType->Apply(typeArgs, factory);
    return factory.MakeBoxPtrType(move(appliedInnerType));
}

optional<RMember> RType_BoxPtr::GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount)
{
    return nullopt;
}

RType_Class::RType_Class(const std::shared_ptr<RClassDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : decl(decl), typeArgs(typeArgs)
{
}

std::optional<RMember_ClassVar> RType_Class::GetVar(const RName& name)
{
    return decl->GetVar(typeArgs, name);
}

RTypePtr RType_Class::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    auto appliedTypeArgs = this->typeArgs->Apply(typeArgs, factory);
    return factory.MakeClassType(decl, move(appliedTypeArgs));
}

optional<RMember> RType_Class::GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount)
{
    return decl->GetMember(typeArgs, name, explicitTypeArgsExceptOuterCount);
}

RType_Struct::RType_Struct(const std::shared_ptr<RStructDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : decl(decl), typeArgs(typeArgs)
{
}

std::optional<RMember_StructVar> RType_Struct::GetVar(const RName& name)
{
    return decl->GetVar(typeArgs, name);
}

std::shared_ptr<Citron::RStructCtorDecl> RType_Struct::GetUnboundTrivialCtor()
{
    return decl->GetUnboundTrivialCtor_RStructCtorDecl();
}

RTypePtr RType_Struct::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    auto appliedTypeArgs = this->typeArgs->Apply(typeArgs, factory);
    return factory.MakeStructType(decl, move(appliedTypeArgs));
}

optional<RMember> RType_Struct::GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount)
{
    return decl->GetMember(typeArgs, name, explicitTypeArgsExceptOuterCount);
}

RType_Enum::RType_Enum(const std::shared_ptr<REnumDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : decl(decl), typeArgs(typeArgs)
{   
}

RTypePtr RType_Enum::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    auto appliedTypeArgs = this->typeArgs->Apply(typeArgs, factory);
    return factory.MakeEnumType(decl, move(appliedTypeArgs));
}

optional<RMember> RType_Enum::GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount)
{
    return decl->GetMember(typeArgs, name, explicitTypeArgsExceptOuterCount);
}

RType_EnumElem::RType_EnumElem(const std::shared_ptr<REnumElemDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : decl(decl), typeArgs(typeArgs)
{
}

std::optional<RMember_EnumElemVar> RType_EnumElem::GetVar(const RName& name)
{
    return decl->GetVar(typeArgs, name);
}

shared_ptr<RType_Enum> RType_EnumElem::GetBaseEnumType(RTypeFactory& factory)
{
    auto enumDecl = decl->GetBaseEnumDecl();

    // enumElem은 typeArgs를 추가로 받지 않기 때문에 그냥 써도 괜찮을 것 같다
    return factory.MakeEnumType(enumDecl, typeArgs);
}


RTypePtr RType_EnumElem::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    auto appliedTypeArgs = this->typeArgs->Apply(typeArgs, factory);
    return factory.MakeEnumElemType(decl, move(appliedTypeArgs));
}

optional<RMember> RType_EnumElem::GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount)
{
    return decl->GetMember(typeArgs, name, explicitTypeArgsExceptOuterCount);
}

RType_Interface::RType_Interface(const std::shared_ptr<RInterfaceDecl>& decl, const RTypeArgumentsPtr& typeArgs, bool bLocal)
    : decl(decl), typeArgs(typeArgs), bLocal(bLocal)
{
}

RTypePtr RType_Interface::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    auto appliedTypeArgs = this->typeArgs->Apply(typeArgs, factory);
    return factory.MakeInterfaceType(decl, move(appliedTypeArgs), bLocal);
}

optional<RMember> RType_Interface::GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount)
{
    throw NotImplementedException();
}

RType_Lambda::RType_Lambda(const std::shared_ptr<RLambdaDecl>& decl, const RTypeArgumentsPtr& outerTypeArgs)
    : decl(decl), outerTypeArgs(outerTypeArgs)
{
}

RTypePtr RType_Lambda::Apply(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    auto appliedOuterTypeArgs = outerTypeArgs->Apply(typeArgs, factory);
    return factory.MakeLambdaType(decl, move(appliedOuterTypeArgs));
}

optional<RMember> RType_Lambda::GetMember(const RName& name, size_t explicitTypeArgsExceptOuterCount)
{
    return decl->GetMember(outerTypeArgs, name, explicitTypeArgsExceptOuterCount);
}

} // Citron