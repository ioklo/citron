#include "RTypes.h"
#include <vector>

#include "Infra/Exceptions.h"

#include "RFactory.h"
#include "RTypeArguments.h"
#include "RStructDecl.h"
#include "RClassDecl.h"
#include "REnumDecl.h"
#include "REnumElemDecl.h"
#include "REnumElemVarDecl.h"
#include "RLambdaDecl.h"
#include "RTypeParamDecl.h"

using namespace std;

namespace Citron {

void RType_Nullable::Accept(RTypeVisitor& visitor) { visitor.Visit(this); }
void RType_NullableInplace::Accept(RTypeVisitor& visitor) { visitor.Visit(this); }
void RType_TypeVar::Accept(RTypeVisitor& visitor) { visitor.Visit(this); }
void RType_Void::Accept(RTypeVisitor& visitor) { visitor.Visit(this); }
void RType_Primitive::Accept(RTypeVisitor& visitor) { visitor.Visit(this); }
void RType_Tuple::Accept(RTypeVisitor& visitor) { visitor.Visit(this); }
void RType_Func::Accept(RTypeVisitor& visitor) { visitor.Visit(this); }
void RType_Ptr::Accept(RTypeVisitor& visitor) { visitor.Visit(this); }
void RType_Shared::Accept(RTypeVisitor& visitor) { visitor.Visit(this); }
void RType_Box::Accept(RTypeVisitor& visitor) { visitor.Visit(this); }
void RType_Class::Accept(RTypeVisitor& visitor) { visitor.Visit(this); }
void RType_Struct::Accept(RTypeVisitor& visitor) { visitor.Visit(this); }
void RType_Enum::Accept(RTypeVisitor& visitor) { visitor.Visit(this); }
void RType_EnumElem::Accept(RTypeVisitor& visitor) { visitor.Visit(this); }
void RType_Interface::Accept(RTypeVisitor& visitor) { visitor.Visit(this); }
void RType_Lambda::Accept(RTypeVisitor& visitor) { visitor.Visit(this); }

RType_Nullable::RType_Nullable(RType* innerType, RFactory* factory)
    : innerType{innerType}, factory{factory}
{
}

RType* RType_Nullable::Apply(RTypeArguments* typeArgs)
{
    auto* appliedInnerType = innerType->Apply(typeArgs);
    return factory->MakeNullableType(appliedInnerType);
}

optional<RDeclRes> RType_Nullable::GetMember(const RName& name, size_t explicitMemberTypeArgsCount)
{
    // 사용자가 검색해서 쓸 수 있는 멤버는 없다
    return nullopt;
}

RType_NullableInplace::RType_NullableInplace(RType* innerType, RFactory* factory)
    : innerType{innerType}, factory{factory}
{
}

RType* RType_NullableInplace::Apply(RTypeArguments* typeArgs)
{   
    return factory->MakeNullableInplaceType(innerType->Apply(typeArgs));
}

optional<RDeclRes> RType_NullableInplace::GetMember(const RName& name, size_t explicitMemberTypeArgsCount)
{
    return nullopt;
}

RType_TypeVar::RType_TypeVar(RTypeParamDecl* decl)
    : decl{decl}
{
}

RType* RType_TypeVar::Apply(RTypeArguments* typeArgs)
{
    size_t globalIndex = decl->GetGlobalIndex();
    return typeArgs->Get(globalIndex);
}

optional<RDeclRes> RType_TypeVar::GetMember(const RName& name, size_t explicitMemberTypeArgsCount)
{
    return nullopt;
}

RType_Void::RType_Void()
{
}

RType* RType_Void::Apply(RTypeArguments* typeArgs)
{
    return this;
}

optional<RDeclRes> RType_Void::GetMember(const RName& name, size_t explicitMemberTypeArgsCount)
{
    return nullopt;
}

RType_Tuple::RType_Tuple(std::vector<RTupleVar>&& vars, RFactory* factory)
    : vars{move(vars)}, factory{factory}
{
}

RType* RType_Tuple::Apply(RTypeArguments* typeArgs)
{
    vector<RTupleVar> appliedVars;
    for (auto& var : vars)
    {
        auto appliedDeclType = var.declType->Apply(typeArgs);
        appliedVars.push_back(RTupleVar { appliedDeclType, var.name });
    }

    return factory->MakeTupleType(move(appliedVars));
}

RCopyStrategy RType_Tuple::GetCopyStrategy()
{
    for (auto& var : vars)
        if (var.declType->GetCopyStrategy() == RCopyStrategy::NonBitwise) // 하나라도 아니라면
            return RCopyStrategy::NonBitwise;

    return RCopyStrategy::Bitwise;
}

optional<RDeclRes> RType_Tuple::GetMember(const RName& name, size_t explicitMemberTypeArgsCount)
{
    throw NotImplementedException();
}

RType_Func::RType_Func(bool bLocal, RType* retType, std::vector<Parameter>&& params, RFactory* factory)
    : bLocal{bLocal}, retType{retType}, params{move(params)}, factory{factory}
{
}

RType* RType_Func::Apply(RTypeArguments* typeArgs)
{
    auto* appliedRetType = retType->Apply(typeArgs);

    vector<RType_Func::Parameter> appliedParams;
    appliedParams.reserve(params.size());

    for (auto& param : params)
    {
        auto* appliedParamType = param.type->Apply(typeArgs);
        appliedParams.emplace_back(param.kind, appliedParamType);
    }

    return factory->MakeFuncType(bLocal, appliedRetType, move(appliedParams));
}

optional<RDeclRes> RType_Func::GetMember(const RName& name, size_t explicitMemberTypeArgsCount)
{
    return nullopt;
}

RType_Func::Parameter::Parameter(RFuncParameterKind kind, RType* type)
    : kind{kind}, type{type}
{

}

RType_Ptr::RType_Ptr(RType* innerType, RFactory* factory)
    : innerType{innerType}, factory{factory}
{
}

RType* RType_Ptr::Apply(RTypeArguments* typeArgs)
{
    auto* appliedInnerType = innerType->Apply(typeArgs);
    return factory->MakePtrType(appliedInnerType);
}

optional<RDeclRes> RType_Ptr::GetMember(const RName& name, size_t explicitMemberTypeArgsCount)
{
    return nullopt;
}

RType_Shared::RType_Shared(RType* innerType, RFactory* factory)
    : innerType{innerType}, factory{factory}
{
}

RType* RType_Shared::Apply(RTypeArguments* typeArgs)
{
    auto* appliedInnerType = innerType->Apply(typeArgs);
    return factory->MakeSharedType(appliedInnerType);
}

std::optional<RDeclRes> RType_Shared::GetMember(const RName& name, size_t explicitMemberTypeArgsCount)
{
    return nullopt;
}

RType_Box::RType_Box(RType* innerType, RFactory* factory)
    : innerType{innerType}, factory{factory}
{

}

RType* RType_Box::Apply(RTypeArguments* typeArgs)
{
    auto* appliedInnerType = innerType->Apply(typeArgs);
    return factory->MakeBoxType(appliedInnerType);
}

optional<RDeclRes> RType_Box::GetMember(const RName& name, size_t explicitMemberTypeArgsCount)
{
    return nullopt;
}

RType_Class::RType_Class(RClassDecl* decl, RTypeArguments* typeArgs, RFactory* factory)
    : decl{decl}, typeArgs{typeArgs}, factory{factory}
{
}

std::optional<RDeclRes_ClassVar> RType_Class::GetVar(const RName& name)
{
    return decl->GetVar(typeArgs, name);
}

bool RType_Class::IsBaseOf(RType_Class& derivedClass)
{
    throw NotImplementedException();
}

RType* RType_Class::Apply(RTypeArguments* typeArgs)
{
    auto* appliedTypeArgs = this->typeArgs->Apply(typeArgs);
    return factory->MakeClassType(decl, appliedTypeArgs);
}

optional<RDeclRes> RType_Class::GetMember(const RName& name, size_t explicitMemberTypeArgsCount)
{
    return decl->GetMember(typeArgs, name, explicitMemberTypeArgsCount);
}

RType_Struct::RType_Struct(RStructDecl* decl, RTypeArguments* typeArgs, RFactory* factory)
    : decl{decl}, typeArgs{typeArgs}, factory{factory}
{
}

std::optional<RDeclRes_StructVar> RType_Struct::GetVar(const RName& name)
{
    return decl->GetVar(typeArgs, name);
}

RStructCtorDecl* RType_Struct::GetUnboundTrivialCtor()
{
    return decl->GetUnboundTrivialCtor_RStructCtorDecl();
}

RType* RType_Struct::Apply(RTypeArguments* typeArgs)
{
    auto* appliedTypeArgs = this->typeArgs->Apply(typeArgs);
    return factory->MakeStructType(decl, appliedTypeArgs);
}

optional<RDeclRes> RType_Struct::GetMember(const RName& name, size_t explicitMemberTypeArgsCount)
{
    return decl->GetMember(typeArgs, name, explicitMemberTypeArgsCount);
}

RType_Enum::RType_Enum(REnumDecl* decl, RTypeArguments* typeArgs, RFactory* factory)
    : decl{decl}, typeArgs{typeArgs}, factory{factory}
{   
}

RType* RType_Enum::Apply(RTypeArguments* typeArgs)
{
    auto* appliedTypeArgs = this->typeArgs->Apply(typeArgs);
    return factory->MakeEnumType(decl, appliedTypeArgs);
}

RCopyStrategy RType_Enum::GetCopyStrategy()
{   
    // TODO: [34] enum [BitwiseCopyable] 지원
    throw NotImplementedException{};
}

optional<RDeclRes> RType_Enum::GetMember(const RName& name, size_t explicitMemberTypeArgsCount)
{
    return decl->GetMember(typeArgs, name, explicitMemberTypeArgsCount);
}

RType_EnumElem::RType_EnumElem(REnumElemDecl* decl, RTypeArguments* typeArgs, RFactory* factory)
    : decl{decl}, typeArgs{typeArgs}, factory{factory}
{
}

std::optional<RDeclRes_EnumElemVar> RType_EnumElem::GetVar(const RName& name)
{
    return decl->GetVar(typeArgs, name);
}

RType_Enum* RType_EnumElem::GetBaseEnumType()
{
    auto enumDecl = decl->GetBaseEnumDecl();

    // enumElem은 typeArgs를 추가로 받지 않기 때문에 그냥 써도 괜찮을 것 같다
    return factory->MakeEnumType(enumDecl, typeArgs);
}


RType* RType_EnumElem::Apply(RTypeArguments* typeArgs)
{
    auto* appliedTypeArgs = this->typeArgs->Apply(typeArgs);
    return factory->MakeEnumElemType(decl, appliedTypeArgs);
}

RCopyStrategy RType_EnumElem::GetCopyStrategy()
{
    for(size_t i = 0, count = decl->GetVarCount(); i < count; i++)
    {
        auto* varDecl = decl->GetVarDecl(i);
        auto* varType = varDecl->GetDeclType(typeArgs);
        if (varType->GetCopyStrategy() == RCopyStrategy::NonBitwise)
            return RCopyStrategy::NonBitwise;
    }

    return RCopyStrategy::Bitwise;
}

optional<RDeclRes> RType_EnumElem::GetMember(const RName& name, size_t explicitMemberTypeArgsCount)
{
    return decl->GetMember(typeArgs, name, explicitMemberTypeArgsCount);
}

RType_Interface::RType_Interface(RInterfaceDecl* decl, RTypeArguments* typeArgs, bool bLocal, RFactory* factory)
    : decl{decl}, typeArgs{typeArgs}, bLocal{bLocal}, factory{factory}
{
}

RType* RType_Interface::Apply(RTypeArguments* typeArgs)
{
    auto* appliedTypeArgs = this->typeArgs->Apply(typeArgs);
    return factory->MakeInterfaceType(decl, appliedTypeArgs, bLocal);
}

optional<RDeclRes> RType_Interface::GetMember(const RName& name, size_t explicitMemberTypeArgsCount)
{
    throw NotImplementedException();
}

RType_Lambda::RType_Lambda(RLambdaDecl* decl, RTypeArguments* outerTypeArgs, RFactory* factory)
    : decl{decl}, outerTypeArgs{outerTypeArgs}, factory{factory}
{
}

vector<RFuncParameter> RType_Lambda::GetPartiallyBoundParameters()
{
    throw NotImplementedException();
}

RType* RType_Lambda::Apply(RTypeArguments* typeArgs)
{
    auto* appliedOuterTypeArgs = outerTypeArgs->Apply(typeArgs);
    return factory->MakeLambdaType(decl, appliedOuterTypeArgs);
}

RCopyStrategy RType_Lambda::GetCopyStrategy()
{
    // TODO: [35] lambda의 [BitwiseCopyable] 지원
    throw NotImplementedException{};
}

optional<RDeclRes> RType_Lambda::GetMember(const RName& name, size_t explicitMemberTypeArgsCount)
{
    return decl->GetMember(outerTypeArgs, name, explicitMemberTypeArgsCount);
}

} // Citron