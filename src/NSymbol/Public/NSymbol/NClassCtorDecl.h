#pragma once
#include "NSymbolConfig.h"

#include <vector>
#include <string>
#include <optional>

#include "RSymbol/RClassCtorDecl.h"

#include "NGenericsComponent.h"
#include "NFuncDeclImpl_UsingNCommonFuncDeclComponent.h"

namespace Citron
{

class NClassCtorDecl
    : public NFuncDeclImpl_UsingNCommonFuncDeclComponent<RClassCtorDecl>
    , private NGenericsComponent
{
public:
    NClassDecl* _class;
    RAccessor accessor;
    bool bTrivial;

public:
    NSYMBOL_API NClassCtorDecl(NClassDecl* _class, RAccessor accessor, bool bTrivial);
    NSYMBOL_API void Init(std::vector<RFuncParameter>&& parameters, bool bLastParamVariadic);
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

    // from RClassCtorDecl
    size_t GetTypeParamCount() override { return NGenericsComponent::GetTypeParamCount(); }
    AnyPtrSizedRange<RTypeParamDecl*> GetTypeParams() override { return NGenericsComponent::GetTypeParams(); }
    NSYMBOL_API RClassDecl* GetClassDecl() override;
};

}