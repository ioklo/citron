#pragma once
#include "NSymbolConfig.h"

#include <vector>
#include <optional>
#include <memory>
#include <string>

#include "RSymbol/RGlobalFuncDecl.h"
#include "NDecl.h"
#include "NGenericsComponent.h"
#include "NFuncDeclImpl_UsingNCommonFuncDeclComponent.h"

namespace Citron {

class NGlobalFuncDecl
    : public NDecl
    , public NFuncDeclImpl_UsingNCommonFuncDeclComponent<RGlobalFuncDecl>
    , private NGenericsComponent
{
public:
    using RDeclType = RGlobalFuncDecl;
    using RDeclResType = RDeclRes_GlobalFuncs;

public:
    NNamespaceDecl* outer;
    RAccessor accessor;
    RName name;

public:
    NSYMBOL_API NGlobalFuncDecl(NNamespaceDecl* outer, RAccessor accessor, bool bSeqFunc, RName&& name);
    using NGenericsComponent::InitTypeParams;
    NSYMBOL_API void InitFuncReturnAndParams(RFuncReturn&& funcRet, std::vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic);
    using NCommonFuncDeclComponent::IsSeqFunc;

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
};

}