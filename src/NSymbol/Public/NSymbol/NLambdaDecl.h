#pragma once
#include "NSymbolConfig.h"

#include <vector>
#include <optional>
#include <unordered_map>
#include <memory>

#include "RSymbol/RLambdaDecl.h"

#include "NFuncDeclOuter.h"
#include "NGenericsComponent.h"
#include "NLambdaVarDecl.h"
#include "NFuncDeclImpl_UsingNCommonFuncDeclComponent.h"

namespace Citron
{

using RFactoryPtr = std::shared_ptr<class RFactory>;

class NLambdaDecl
    : private NGenericsComponent
    , public NFuncDeclImpl_UsingNCommonFuncDeclComponent<RLambdaDecl>
{
public:
    NFuncDeclOuter outer;
    RName name;

    // 가지고 있어야 할 멤버 변수들, type, name, ref 여부
    std::optional<std::vector<NLambdaVarDecl*>> vars;
    std::unordered_map<RName, NLambdaVarDecl*> varsMap;

public:
    NSYMBOL_API NLambdaDecl(NFuncDeclOuter&& outer, RName&& name);
    NSYMBOL_API void Init(RFuncReturn&& funcReturn, RThisKind&& thisKind, std::vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic);
    NSYMBOL_API void InitVars(std::vector<NLambdaVarDecl*>&& vars);
    using NCommonFuncDeclComponent::IsSeqFunc;

public:
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
    RAccessor GetAccessor() override { return RAccessor::Public; }
    NSYMBOL_API RIdentifier GetIdentifier() override;
    NSYMBOL_API RTypeDecl* GetTypeMember(const RName& name, size_t typeParamCount) override;
    NSYMBOL_API std::optional<RDeclRes> GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
    NSYMBOL_API std::optional<RDeclRes> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount) override;

    // from RTypeDecl
    RType* GetOpenType() override;

    // from RLambdaDecl
    size_t GetTypeParamCount() override { return NGenericsComponent::GetTypeParamCount(); }
    AnyPtrSizedRange<RTypeParamDecl*> GetTypeParams() override { return NGenericsComponent::GetTypeParams(); }
};

}