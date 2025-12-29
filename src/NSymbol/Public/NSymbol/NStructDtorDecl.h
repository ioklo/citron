#pragma once
#include "NSymbolConfig.h"

#include <cassert>

#include "RSymbol/RStructDtorDecl.h"

#include "NStructDecl.h"
#include "NFuncDecl.h"
#include "NFuncDeclOuter.h"
#include "NCommonFuncDeclComponent.h"

namespace Citron {

class NStructDtorDecl
    : public NDecl
    , public NFuncDecl
    , public NFuncDeclOuter
    , public RStructDtorDecl
    , private NCommonFuncDeclComponent
{
    RAccessor accessor;
    NStructDecl* structDecl;

public:
    NSYMBOL_API NStructDtorDecl(RAccessor accessor, NStructDecl* structDecl);

    // from NDecl
    RDecl* GetRDecl() override { return this; }
    NDecl* GetNOuter() override { return structDecl; }
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(this); }

    // from NFuncDecl
    NDecl* GetNDecl() override { return this; }
    RFuncReturn GetUnboundFuncReturn() override { return NCommonFuncDeclComponent::GetUnboundFuncReturn(); }
    std::span<RFuncParameter> GetUnboundFuncParams() override { return NCommonFuncDeclComponent::GetUnboundFuncParams(); }
    bool IsSeqFunc() override { return NCommonFuncDeclComponent::IsSeqFunc(); }
    void Accept(NFuncDeclVisitor& visitor) override { visitor.Visit(this); }

    // from NFuncDeclOuter
    // NDecl* GetNDecl() override { return this; }
    void Accept(NFuncDeclOuterVisitor& visitor) override { visitor.Visit(this); }

    // from RDecl
    RDecl* GetROuter() override { return structDecl; }
    RAccessor GetAccessor() override { return accessor; }
    RIdentifier GetIdentifier() override { return RIdentifier{RName_Reserved("Dtor"), 0, {}};    }
    std::optional<RMember> GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override { return std::nullopt; }
    std::optional<RMember> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RFactory& factory) override { return std::nullopt; }

    // from RFuncDecl
    // RDecl* GetRDecl() override { return this;
    bool IsStatic() override { return false; }
    size_t GetTypeParamCount() override { return NCommonFuncDeclComponent::GetTypeParamCount(); }
    size_t GetParamCount() override { return NCommonFuncDeclComponent::GetParamCount(); }
    RType* GetReturnType(RTypeArguments& typeArgs, RFactory& factory) override { return NCommonFuncDeclComponent::GetReturnType(typeArgs, factory); }
    RFuncReturn GetFuncReturn(RTypeArguments& typeArgs, RFactory& factory) override { return NCommonFuncDeclComponent::GetFuncReturn(typeArgs, factory); }
    RFuncParameter GetFuncParam(RTypeArguments& typeArgs, size_t index, RFactory& factory) override { return NCommonFuncDeclComponent::GetFuncParam(typeArgs, index, factory); }
    // std::span<RFuncParameter> GetUnboundFuncParams() override { return {}; }

};



} // namespace Citron
