#pragma once

#include "IR0Config.h"

#include <memory>
#include <vector>

#include "RStructCtorDecl.h"

#include "NDecl.h"
#include "NFuncDeclOuter.h"
#include "NFuncDecl.h"
#include "NCommonFuncDeclComponent.h"

namespace Citron {

class NStructCtorDecl
    : public NDecl
    , public NFuncDecl
    , public NFuncDeclOuter
    , public RStructCtorDecl
    , private NCommonFuncDeclComponent
{
public:
    std::weak_ptr<NStructDecl> _struct;
    RAccessor accessor;
    bool bTrivial;

public:
    IR0_API NStructCtorDecl(std::weak_ptr<NStructDecl> _struct, RAccessor accessor, bool bTrivial);
    IR0_API void InitFuncParameters(std::vector<RFuncParameter> parameters, bool bLastParameterVariadic);
    using NCommonFuncDeclComponent::InitBody;
    using NCommonFuncDeclComponent::InitBodyWillBeGenerated;
    IR0_API ~NStructCtorDecl();

public:
    // from NDecl
    RDecl* GetRDecl() override { return this; }
    NDecl* GetNOuter() override;
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(*this); }

    // from NFuncDecl
    NDecl* GetNDecl() override { return this; }
    RFuncReturn GetUnboundFuncReturn() { return NCommonFuncDeclComponent::GetUnboundFuncReturn(); }
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
    // RDecl* GetRDecl() override;
    bool IsStatic() override { return NCommonFuncDeclComponent::IsStatic(); }
    size_t GetTypeParamCount() override { return NCommonFuncDeclComponent::GetTypeParamCount(); }
    size_t GetParamCount() override { return NCommonFuncDeclComponent::GetParamCount(); }
    RTypePtr GetReturnType(RTypeArguments& typeArgs, RTypeFactory& factory) override { return NCommonFuncDeclComponent::GetReturnType(typeArgs, factory); }
    RFuncReturn GetFuncReturn(RTypeArguments& typeArgs, RTypeFactory& factory) override { return NCommonFuncDeclComponent::GetFuncReturn(typeArgs, factory); }
    RFuncParameter GetFuncParam(RTypeArguments& typeArgs, size_t index, RTypeFactory& factory) override { return NCommonFuncDeclComponent::GetFuncParam(typeArgs, index, factory); }

    // from RFuncDeclOuter
    // RDecl* GetRDecl() override { return this; }

    // from RStructCtorDecl
    IR0_API std::shared_ptr<RStructDecl> GetStructDecl() override;
    RFuncParameter& GetUnboundFuncParam(size_t index) override { return NCommonFuncDeclComponent::GetUnboundFuncParam(index); }
};

}