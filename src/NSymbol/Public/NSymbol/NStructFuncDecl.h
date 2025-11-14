#pragma once

#include "NSymbolConfig.h"
#include <vector>

#include "RSymbol/RStructFuncDecl.h"

#include "NDecl.h"
#include "NFuncDeclOuter.h"
#include "NFuncDecl.h"
#include "NCommonFuncDeclComponent.h"

namespace Citron {

class NStructFuncDecl
    : public NDecl
    , public NFuncDecl
    , public NFuncDeclOuter
    , public RStructFuncDecl
    , private NCommonFuncDeclComponent
{
public:
    using RDeclType = RStructFuncDecl;
    using RMemberType = RMember_StructFuncs;

public:
    NStructDecl* _struct;
    RAccessor accessor;
    std::string name;
    bool bStatic;

public:
    NSYMBOL_API NStructFuncDecl(NStructDecl* _struct);
    NSYMBOL_API void Init(RAccessor accessor, std::string name, std::vector<std::string>&& typeParams, bool bStatic);
    NSYMBOL_API void InitFuncReturnAndParams(RType* funcReturn, std::vector<RFuncParameter> funcParameters, bool bLastParameterVariadic);

public:
    // from NDecl
    RDecl* GetRDecl() override { return this; }
    NDecl* GetNOuter() override;
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(this); }

    // from NFuncDecl
    NDecl* GetNDecl() override { return this; }
    RFuncReturn GetUnboundFuncReturn() override { return NCommonFuncDeclComponent::GetUnboundFuncReturn(); }
    bool IsSeqFunc() override { return NCommonFuncDeclComponent::IsSeqFunc(); }
    void Accept(NFuncDeclVisitor& visitor) override { visitor.Visit(this); }

    // from NFuncDeclOuter
    // NDecl* GetNDecl() override { return this; }
    void Accept(NFuncDeclOuterVisitor& visitor) override { visitor.Visit(this); }

    // from RDecl
    NSYMBOL_API RDecl* GetROuter() override;
    RAccessor GetAccessor() override { return accessor; }
    NSYMBOL_API RIdentifier GetIdentifier() override;
    NSYMBOL_API std::optional<RMember> GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
    NSYMBOL_API std::optional<RMember> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RFactory& factory) override;

    // from RFuncDecl
    // RDecl* GetRDecl() override { return this; }
    bool IsStatic() override { return NCommonFuncDeclComponent::IsStatic(); }
    size_t GetTypeParamCount() override { return NCommonFuncDeclComponent::GetTypeParamCount(); }
    size_t GetParamCount() override { return NCommonFuncDeclComponent::GetParamCount(); }
    RType* GetReturnType(RTypeArguments& typeArgs, RFactory& factory) override { return NCommonFuncDeclComponent::GetReturnType(typeArgs, factory); }
    RFuncReturn GetFuncReturn(RTypeArguments& typeArgs, RFactory& factory) override { return NCommonFuncDeclComponent::GetFuncReturn(typeArgs, factory); }
    RFuncParameter GetFuncParam(RTypeArguments& typeArgs, size_t index, RFactory& factory) override { return NCommonFuncDeclComponent::GetFuncParam(typeArgs, index, factory); }

    // from RFuncDeclOuter
    // RDecl* GetRDecl() override { return this; }

    // from RStructFuncDecl
    // RType* GetReturnType(RTypeArguments& typeArgs, RFactory& factory) override { return NCommonFuncDeclComponent::GetReturnType(typeArgs, factory); }
};

}