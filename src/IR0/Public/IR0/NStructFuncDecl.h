#pragma once

#include "IR0Config.h"
#include <memory>
#include <vector>

#include "RStructFuncDecl.h"

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
    std::weak_ptr<NStructDecl> _struct;
    RAccessor accessor;
    std::string name;
    std::vector<std::string> typeParams;
    bool bStatic;

public:
    IR0_API NStructFuncDecl(std::weak_ptr<NStructDecl> _struct, RAccessor accessor, std::string name, std::vector<std::string>&& typeParams, bool bStatic);
    IR0_API void InitFuncReturnAndParams(RTypePtr funcReturn, std::vector<RFuncParameter> funcParameters, bool bLastParameterVariadic);
    using NCommonFuncDeclComponent::InitBody;

public:
    // from NDecl
    RDecl* GetRDecl() override { return this; }
    NDecl* GetNOuter() override;
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(*this); }

    // from NFuncDecl
    NDecl* GetNDecl() override { return this; }
    RFuncReturn GetUnboundFuncReturn() override { return NCommonFuncDeclComponent::GetUnboundFuncReturn(); }
    bool IsSeqFunc() override { return NCommonFuncDeclComponent::IsSeqFunc(); }
    void Accept(NFuncDeclVisitor& visitor) override { visitor.Visit(*this); }

    // from NFuncDeclOuter
    // NDecl* GetNDecl() override { return this; }
    void Accept(NFuncDeclOuterVisitor& visitor) override { visitor.Visit(*this); }

    // from RDecl
    IR0_API RDecl* GetROuter() override;
    RAccessor GetAccessor() override { return accessor; }
    IR0_API RIdentifier GetIdentifier() override;
    IR0_API std::optional<RMember> GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
    IR0_API std::optional<RMember> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory) override;

    // from RFuncDecl
    // RDecl* GetRDecl() override { return this; }
    bool IsStatic() override { return NCommonFuncDeclComponent::IsStatic(); }
    size_t GetTypeParamCount() override { return NCommonFuncDeclComponent::GetTypeParamCount(); }
    size_t GetParamCount() override { return NCommonFuncDeclComponent::GetParamCount(); }
    RTypePtr GetReturnType(RTypeArguments& typeArgs, RTypeFactory& factory) override { return NCommonFuncDeclComponent::GetReturnType(typeArgs, factory); }
    RFuncReturn GetFuncReturn(RTypeArguments& typeArgs, RTypeFactory& factory) override { return NCommonFuncDeclComponent::GetFuncReturn(typeArgs, factory); }
    RFuncParameter GetFuncParam(RTypeArguments& typeArgs, size_t index, RTypeFactory& factory) override { return NCommonFuncDeclComponent::GetFuncParam(typeArgs, index, factory); }

    // from RFuncDeclOuter
    // RDecl* GetRDecl() override { return this; }

    // from RStructFuncDecl
    // RTypePtr GetReturnType(RTypeArguments& typeArgs, RTypeFactory& factory) override { return NCommonFuncDeclComponent::GetReturnType(typeArgs, factory); }
};

}