#pragma once
#include "NSymbolConfig.h"

#include <memory>

#include "RSymbol/RClassFuncDecl.h"

#include "NGenericsComponent.h"
#include "NFuncDeclImpl_UsingNCommonFuncDeclComponent.h"

namespace Citron {

class NClassFuncDecl
    : public NFuncDeclImpl_UsingNCommonFuncDeclComponent<RClassFuncDecl>
    , private NGenericsComponent
{
public:
    NClassDecl* _class;
    RAccessor accessor;
    RName name;
    bool _static;

public:
    NClassFuncDecl(NClassDecl* _class, RAccessor accessor, RName&& name, bool bStatic, bool bSeqFunc);
    using NGenericsComponent::InitTypeParams;
    void Init(RFuncReturn&& funcReturn, std::vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic);
    using NCommonFuncDeclComponent::IsSeqFunc;
    
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

    // from RClassFuncDecl
    size_t GetTypeParamCount() override { return NGenericsComponent::GetTypeParamCount(); }
    AnyPtrSizedRange<RTypeParamDecl*> GetTypeParams() override { return NGenericsComponent::GetTypeParams(); }
};

}