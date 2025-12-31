#pragma once

#include "NSymbolConfig.h"
#include <vector>
#include <optional>
#include <string>

#include "NDecl.h"
#include "NFuncDecl.h"
#include "NFuncDeclOuter.h"
#include "NCommonFuncDeclComponent.h"

#include "RSymbol/RGlobalFuncDecl.h"

namespace Citron {

class NGlobalFuncDecl
    : public NDecl
    , public NFuncDecl
    , public NFuncDeclOuter
    , public RGlobalFuncDecl
    , private NCommonFuncDeclComponent
{
public:
    using RDeclType = RGlobalFuncDecl;
    using RMemberType = RMember_GlobalFuncs;

public:
    NNamespaceDecl* outer;
    RAccessor accessor;
    RName name;

public:
    NSYMBOL_API NGlobalFuncDecl(NNamespaceDecl* outer, RAccessor accessor, bool bSeqFunc, RName&& name, std::vector<std::string>&& typeParams);
    using NCommonFuncDeclComponent::InitFuncReturnAndParams;

    // from NDecl
    RDecl* GetRDecl() override { return this; }
    NSYMBOL_API NDecl* GetNOuter() override;
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(this); }

    // from NFuncDecl
    NDecl* GetNDecl() override { return this; }
    NSYMBOL_API NFuncDeclOuter* GetNFuncDeclOuter() override;
    RFuncReturn GetUnboundFuncReturn() override { return NCommonFuncDeclComponent::GetUnboundFuncReturn(); }
    std::span<RFuncParameter> GetUnboundFuncParams() override { return NCommonFuncDeclComponent::GetUnboundFuncParams(); }
    bool IsSeqFunc() override { return NCommonFuncDeclComponent::IsSeqFunc(); }
    void Accept(NFuncDeclVisitor& visitor) override { visitor.Visit(this); }

    // from NFuncDeclOuter
    // NDecl* GetNDecl() override { return this; }
    void Accept(NFuncDeclOuterVisitor& visitor) override { visitor.Visit(this); }

    // from RDecl
    NSYMBOL_API RDecl* GetROuter() override;
    RAccessor GetAccessor() override { return accessor; }
    NSYMBOL_API RIdentifier GetIdentifier() override;
    NSYMBOL_API RTypeDecl* GetTypeMember(const RName& name, size_t typeParamCount) override;
    NSYMBOL_API std::optional<RMember> GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
    NSYMBOL_API std::optional<RMember> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount) override;

    // from RFuncDecl
    // virtual RDecl* GetRDecl() = 0;
    bool IsStatic() override { return NCommonFuncDeclComponent::IsStatic(); }
    size_t GetTypeParamCount() override { return NCommonFuncDeclComponent::GetTypeParamCount(); }
    size_t GetParamCount() override { return NCommonFuncDeclComponent::GetParamCount(); }
    RType* GetReturnType(RTypeArguments& typeArgs) override { return NCommonFuncDeclComponent::GetReturnType(typeArgs, factory); }
    RFuncReturn GetFuncReturn(RTypeArguments& typeArgs) override { return NCommonFuncDeclComponent::GetFuncReturn(typeArgs, factory); }
    RFuncParameter GetFuncParam(RTypeArguments& typeArgs, size_t index) override { return NCommonFuncDeclComponent::GetFuncParam(typeArgs, index, factory); }
    // std::span<RFuncParameter> GetUnboundFuncParams() override { return NCommonFuncDeclComponent::GetUnboundFuncParams(); }
    // virtual void Accept(RFuncDeclVisitor& visitor) = 0;

    // from RFuncDeclOuter
    // RDecl* GetRDecl() override { return this; }
};

}