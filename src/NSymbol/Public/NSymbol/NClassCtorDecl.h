#pragma once
#include "NSymbolConfig.h"

#include <vector>
#include <string>
#include <optional>

#include "RSymbol/RClassCtorDecl.h"

#include "NDecl.h"
#include "NFuncDeclOuter.h"
#include "NGenericsComponent.h"
#include "NCommonFuncDeclComponent.h"

namespace Citron
{

class NClassCtorDecl
    : public NDecl
    , public RClassCtorDecl
    , private NGenericsComponent
    , private NCommonFuncDeclComponent
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
    size_t GetTypeParamCount() override { return NGenericsComponent::GetTypeParamCount(); }
    RTypeParamDecl* GetTypeParam(size_t index) override { return NGenericsComponent::GetTypeParam(index); }
    NSYMBOL_API RTypeDecl* GetTypeMember(const RName& name, size_t typeParamCount) override;
    NSYMBOL_API std::optional<RDeclRes> GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
    NSYMBOL_API std::optional<RDeclRes> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount) override;

    // from RFuncDecl
    // RDecl* GetRDecl() override { return this; }
    RThisKind GetThisKind() override { return NCommonFuncDeclComponent::GetThisKind(); }
    // size_t GetTypeParamCount() override { return NGenericsComponent::GetTypeParamCount(); }
    // RTypeParamDecl* GetTypeParam(size_t index) override { return NGenericsComponent::GetTypeParam(index); }
    size_t GetParamCount() override { return NCommonFuncDeclComponent::GetParamCount(); }
    RType* GetReturnType(RTypeArguments* typeArgs) override { return NCommonFuncDeclComponent::GetReturnType(typeArgs); }
    RFuncReturn GetFuncReturn(RTypeArguments* typeArgs) override { return NCommonFuncDeclComponent::GetFuncReturn(typeArgs); }
    RFuncParameter GetFuncParam(RTypeArguments* typeArgs, size_t index) override { return NCommonFuncDeclComponent::GetFuncParam(typeArgs, index); }
    RFuncReturn GetUnboundFuncReturn() override { return NCommonFuncDeclComponent::GetUnboundFuncReturn(); }
    std::span<RFuncParameter> GetUnboundFuncParams() override { return NCommonFuncDeclComponent::GetUnboundFuncParams(); }

    // from RClassCtorDecl
    NSYMBOL_API RClassDecl* GetClassDecl() override;
};

}