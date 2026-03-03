#pragma once
#include "NSymbolConfig.h"

#include <vector>
#include <span>
#include <memory>

#include "RSymbol/RStructCtorDecl.h"

#include "NDecl.h"
#include "NFuncDeclOuter.h"
#include "NFuncDecl.h"
#include "NGenericsComponent.h"
#include "NCommonFuncDeclComponent.h"

namespace Citron {

class NStructCtorDecl
    : public NDecl
    , public NFuncDecl
    , public NFuncDeclOuter
    , public RStructCtorDecl
    , private NGenericsComponent
    , private NCommonFuncDeclComponent
{
public:
    NStructDecl* _struct;
    RAccessor accessor;
    RStructCtorKind kind;

public:
    NSYMBOL_API NStructCtorDecl(NStructDecl* _struct, RAccessor accessor, RStructCtorKind kind);
    NSYMBOL_API void InitFuncParameters(std::vector<RFuncParameter>&& parameters, bool bLastParameterVariadic);
    NSYMBOL_API ~NStructCtorDecl();

public:
    // from NDecl
    RDecl* GetRDecl() override { return this; }
    NSYMBOL_API NDecl* GetNOuter() override;
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(this); }

    // from NFuncDecl
    NDecl* GetNDecl() override { return this; }
    NSYMBOL_API NFuncDeclOuter* GetNFuncDeclOuter() override;
    RFuncDecl* GetRFuncDecl() override { return this; }
    bool IsSeqFunc() override { return NCommonFuncDeclComponent::IsSeqFunc(); }
    void Accept(NFuncDeclVisitor& visitor) override { visitor.Visit(this); }

    // from NFuncDeclOuter
    // NDecl* GetNDecl() override { return this; }
    NSYMBOL_API void Accept(NFuncDeclOuterVisitor& visitor) override;

    // from RDecl
    NSYMBOL_API RDecl* GetROuter() override;
    RAccessor GetAccessor() override { return accessor; }
    NSYMBOL_API RIdentifier GetIdentifier() override;
    size_t GetTypeParamCount() override { return NGenericsComponent::GetTypeParamCount(); }
    RTypeParamDecl* GetTypeParam(size_t index) override { return NGenericsComponent::GetTypeParam(index); }
    NSYMBOL_API RTypeDecl* GetTypeMember(const RName& name, size_t typeParamCount) override;
    NSYMBOL_API std::optional<RDeclRes> GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
    NSYMBOL_API std::optional<RDeclRes> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount) override;

    // from RFuncDecl
    // RDecl* GetRDecl() override;
    RThisKind GetThisKind() override { return NCommonFuncDeclComponent::GetThisKind(); }
    // size_t GetTypeParamCount() override { return NGenericsComponent::GetTypeParamCount(); }
    size_t GetParamCount() override { return NCommonFuncDeclComponent::GetParamCount(); }
    RType* GetReturnType(RTypeArguments& typeArgs) override { return NCommonFuncDeclComponent::GetReturnType(typeArgs); }
    RFuncReturn GetFuncReturn(RTypeArguments& typeArgs) override { return NCommonFuncDeclComponent::GetFuncReturn(typeArgs); }
    RFuncParameter GetFuncParam(RTypeArguments& typeArgs, size_t index) override { return NCommonFuncDeclComponent::GetFuncParam(typeArgs, index); }
    RFuncReturn GetUnboundFuncReturn() override { return NCommonFuncDeclComponent::GetUnboundFuncReturn(); }
    std::span<RFuncParameter> GetUnboundFuncParams() override { return NCommonFuncDeclComponent::GetUnboundFuncParams(); }

    // from RFuncDeclOuter
    // RDecl* GetRDecl() override { return this; }

    // from RStructCtorDecl
    NSYMBOL_API RStructDecl* GetStructDecl() override;
    RStructCtorKind GetKind() override { return kind; }
};

}