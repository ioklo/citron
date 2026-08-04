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
#include "RTypeParam.h"

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

RType_NullableInplace::RType_NullableInplace(RType* innerType, RFactory* factory)
    : innerType{innerType}, factory{factory}
{
}

RType* RType_NullableInplace::Apply(RTypeArguments* typeArgs)
{   
    return factory->MakeNullableInplaceType(innerType->Apply(typeArgs));
}

RType_TypeVar::RType_TypeVar(RTypeParam* typeParam)
    : typeParam{typeParam}
{
}

RType* RType_TypeVar::Apply(RTypeArguments* typeArgs)
{
    size_t globalIndex = typeParam->GetGlobalIndex();
    return typeArgs->Get(globalIndex);
}

RType_Void::RType_Void()
{
}

RType* RType_Void::Apply(RTypeArguments* typeArgs)
{
    return this;
}

RType_Tuple::RType_Tuple(vector<RTupleVar>&& vars, RFactory* factory)
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

RType_Func::RType_Func(bool bLocal, RType* retType, vector<Parameter>&& params, RFactory* factory)
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

RType_Shared::RType_Shared(RType* innerType, RFactory* factory)
    : innerType{innerType}, factory{factory}
{
}

RType* RType_Shared::Apply(RTypeArguments* typeArgs)
{
    auto* appliedInnerType = innerType->Apply(typeArgs);
    return factory->MakeSharedType(appliedInnerType);
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

RType_Class::RType_Class(TakeRef<RAppliedDecl<RClassDecl>> appliedDecl, RFactory* factory)
    : appliedDecl{appliedDecl.Take()}, factory{factory}
{
}

optional<RAppliedDecl<RClassVarDecl>> RType_Class::GetVar(InRef<RName> name)
{
    auto* var = appliedDecl.decl->GetUnboundVar(name);
    if (!var) return nullopt;

    return RAppliedDecl<RClassVarDecl>{var, appliedDecl.typeArgs};
}

bool RType_Class::IsBaseOf(RType_Class& derivedClass)
{
    throw NotImplementedException();
}

RType* RType_Class::Apply(RTypeArguments* typeArgs)
{
    auto* appliedTypeArgs = this->appliedDecl.typeArgs->Apply(typeArgs);
    return factory->MakeClassType(RAppliedDecl<RClassDecl>{appliedDecl.decl, appliedTypeArgs});
}

RType_Struct::RType_Struct(TakeRef<RAppliedDecl<RStructDecl>> appliedDecl, RFactory* factory)
    : appliedDecl{appliedDecl.Take()}, factory{factory}
{
}

optional<RAppliedDecl<RStructVarDecl>> RType_Struct::GetVar(InRef<RName> name)
{
    auto* structVar = appliedDecl.decl->GetUnboundVar(name);
    if (!structVar) return nullopt;

    return RAppliedDecl<RStructVarDecl>{structVar, appliedDecl.typeArgs};
}

RStructCtorDecl* RType_Struct::GetUnboundTrivialCtor()
{
    return appliedDecl.decl->GetUnboundTrivialCtor();
}

RType* RType_Struct::Apply(RTypeArguments* typeArgs)
{
    auto* appliedTypeArgs = this->appliedDecl.typeArgs->Apply(typeArgs);
    return factory->MakeStructType(RAppliedDecl<RStructDecl>{appliedDecl.decl, appliedTypeArgs});
}

RType_Enum::RType_Enum(TakeRef<RAppliedDecl<REnumDecl>> appliedDecl, RFactory* factory)
    : appliedDecl{appliedDecl.Take()}, factory{factory}
{   
}

RType* RType_Enum::Apply(RTypeArguments* typeArgs)
{
    auto* appliedTypeArgs = this->appliedDecl.typeArgs->Apply(typeArgs);
    return factory->MakeEnumType(RAppliedDecl<REnumDecl>{appliedDecl.decl, appliedTypeArgs});
}

RCopyStrategy RType_Enum::GetCopyStrategy()
{   
    // TODO: [34] enum [BitwiseCopyable] 지원
    throw NotImplementedException{};
}

RType_EnumElem::RType_EnumElem(TakeRef<RAppliedDecl<REnumElemDecl>> appliedDecl, RFactory* factory)
    : appliedDecl{appliedDecl.Take()}, factory{factory}
{
}

optional<RAppliedDecl<REnumElemVarDecl>> RType_EnumElem::GetVar(InRef<RName> name)
{
    if (auto* var = appliedDecl.decl->GetUnboundVar(name))
        return RAppliedDecl<REnumElemVarDecl>{var, appliedDecl.typeArgs};

    return nullopt;
}

RType_Enum* RType_EnumElem::GetEnumType()
{
    auto enumDecl = appliedDecl.decl->GetEnum();

    // enumElem은 typeArgs를 추가로 받지 않기 때문에 그냥 써도 괜찮을 것 같다
    return factory->MakeEnumType(RAppliedDecl<REnumDecl>{enumDecl, appliedDecl.typeArgs});
}


RType* RType_EnumElem::Apply(RTypeArguments* typeArgs)
{
    auto* appliedTypeArgs = this->appliedDecl.typeArgs->Apply(typeArgs);
    return factory->MakeEnumElemType(RAppliedDecl<REnumElemDecl>{appliedDecl.decl, appliedTypeArgs});
}

RCopyStrategy RType_EnumElem::GetCopyStrategy()
{
    for(size_t i = 0, count = appliedDecl.decl->GetVarCount(); i < count; i++)
    {
        auto* varDecl = appliedDecl.decl->GetUnboundVar(i);
        auto* varType = varDecl->GetUnboundDeclType()->Apply(appliedDecl.typeArgs);
        if (varType->GetCopyStrategy() == RCopyStrategy::NonBitwise)
            return RCopyStrategy::NonBitwise;
    }

    return RCopyStrategy::Bitwise;
}


RType_Interface::RType_Interface(TakeRef<RAppliedDecl<RInterfaceDecl>> appliedDecl, bool bLocal, RFactory* factory)
    : appliedDecl{appliedDecl.Take()}, bLocal{bLocal}, factory{factory}
{
}

RType* RType_Interface::Apply(RTypeArguments* typeArgs)
{
    auto* appliedTypeArgs = this->appliedDecl.typeArgs->Apply(typeArgs);
    return factory->MakeInterfaceType(RAppliedDecl<RInterfaceDecl>{appliedDecl.decl, appliedTypeArgs}, bLocal);
}

RType_Lambda::RType_Lambda(RAppliedDecl<RLambdaDecl>&& appliedDecl, RFactory* factory)
    : appliedDecl{std::move(appliedDecl)}, factory{factory}
{
}

vector<RFuncParameter> RType_Lambda::GetPartiallyBoundParameters()
{
    throw NotImplementedException();
}

RType* RType_Lambda::Apply(RTypeArguments* typeArgs)
{
    auto* appliedTypeArgs = this->appliedDecl.typeArgs->Apply(typeArgs);
    return factory->MakeLambdaType(RAppliedDecl<RLambdaDecl>{appliedDecl.decl, appliedTypeArgs});
}

RCopyStrategy RType_Lambda::GetCopyStrategy()
{
    // TODO: [35] lambda의 [BitwiseCopyable] 지원
    throw NotImplementedException{};
}

RType_Opaque::RType_Opaque(TakeRef<RAppliedDecl<RTraitDecl>> appliedTrait, TakeRef<RAppliedDecl<RDecl>> appliedOwnerFunc, RFactory* factory, PrivateKey pk)
    : appliedTrait{appliedTrait.Take()}, appliedOwnerFunc{appliedOwnerFunc.Take()}, factory{factory}
{
}

RType* RType_Opaque::Apply(RTypeArguments* typeArgs)
{
    return factory->MakeOpaqueType(appliedTrait.Apply(typeArgs), appliedOwnerFunc.Apply(typeArgs));
}

RCopyStrategy RType_Opaque::GetCopyStrategy()
{
    return RCopyStrategy::NonBitwise;
}

void RType_Opaque::Accept(RTypeVisitor& visitor)
{
    return visitor.Visit(this);
}

} // Citron