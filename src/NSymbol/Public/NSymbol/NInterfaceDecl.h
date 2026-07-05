#pragma once

#include "NSymbolConfig.h"
#include <vector>

#include "RSymbol/RInterfaceDecl.h"
#include "NGenericsComponent.h"

namespace Citron {

class NInterfaceDecl
    : public RInterfaceDecl
    , private NGenericsComponent
{
    NTypeDeclOuter* outer;
    RAccessor accessor;
    RName name;

public:
    NInterfaceDecl(NTypeDeclOuter* outer, RAccessor accessor, RName&& name);
    using NGenericsComponent::InitTypeParams;

    // from NDecl
    RDecl* GetRDecl() override { return this; }
    NSYMBOL_API NDecl* GetNOuter() override;
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(this); }

    // from NTypeDecl
    NDecl* GetNDecl() override { return this; }
    RTypeDecl* GetRTypeDecl() override { return this; }
    RDeclRes ToRDeclRes(RTypeArguments* typeArgs) override;
    void Accept(NTypeDeclVisitor& visitor) override { visitor.Visit(this); }

    // from RDecl
    NSYMBOL_API RDecl* GetROuter() override;
    RAccessor GetAccessor() override { return accessor; }
    NSYMBOL_API RIdentifier GetIdentifier() override;
    NSYMBOL_API RTypeDecl* GetTypeMember(const RName& name, size_t typeParamCount) override;
    NSYMBOL_API std::optional<RDeclRes> GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
    NSYMBOL_API std::optional<RDeclRes> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount) override;

    // from RTypeDecl
    // RDecl* GetRDecl() override { return this; }
    RType* GetOpenType() override;

    // from RInterfaceDecl
    size_t GetTypeParamCount() override { return NGenericsComponent::GetTypeParamCount(); }
    AnyPtrSizedRange<RTypeParamDecl*> GetTypeParams() override { return NGenericsComponent::GetTypeParams(); }
};

}