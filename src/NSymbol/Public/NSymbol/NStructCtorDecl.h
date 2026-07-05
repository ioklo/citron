#pragma once
#include "NSymbolConfig.h"

#include <vector>
#include <span>
#include <memory>

#include "RSymbol/RStructCtorDecl.h"
 
#include "NGenericsComponent.h"
#include "NFuncDeclImpl_UsingNCommonFuncDeclComponent.h"

namespace Citron {

class NStructCtorDecl
    : private NGenericsComponent
    , public NFuncDeclImpl_UsingNCommonFuncDeclComponent<RStructCtorDecl>
{
public:
    NStructDecl* _struct;
    RAccessor accessor;
    RStructCtorKind kind;

public:
    NSYMBOL_API NStructCtorDecl(NStructDecl* _struct, RAccessor accessor, RStructCtorKind kind);
    NSYMBOL_API void InitFuncParameters(std::vector<RFuncParameter>&& parameters, bool bLastParameterVariadic);
    NSYMBOL_API ~NStructCtorDecl();
    using NCommonFuncDeclComponent::IsSeqFunc;
    
public:
    // from NDecl
    RDecl* GetRDecl() override { return this; }
    NSYMBOL_API NDecl* GetNOuter() override;
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(this); }

    // from RDecl
    NSYMBOL_API RDecl* GetROuter() override;
    RAccessor GetAccessor() override { return accessor; }
    NSYMBOL_API RIdentifier GetIdentifier() override;
    NSYMBOL_API RTypeDecl* GetTypeMember(const RName& name, size_t typeParamCount) override;
    NSYMBOL_API std::optional<RDeclRes> GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
    NSYMBOL_API std::optional<RDeclRes> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount) override;

    // from RStructCtorDecl
    size_t GetTypeParamCount() override { return NGenericsComponent::GetTypeParamCount(); }
    AnyPtrSizedRange<RTypeParamDecl*> GetTypeParams() override { return NGenericsComponent::GetTypeParams(); }
    NSYMBOL_API RStructDecl* GetStructDecl() override;
    RStructCtorKind GetKind() override { return kind; }
};

}