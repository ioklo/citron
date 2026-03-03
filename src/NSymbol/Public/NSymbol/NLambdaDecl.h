#pragma once
#include "NSymbolConfig.h"

#include <vector>
#include <optional>
#include <unordered_map>
#include <memory>

#include "RSymbol/RLambdaDecl.h"

#include "NDecl.h"
#include "NFuncDeclOuter.h"
#include "NTypeDecl.h"
#include "NFuncDeclOuter.h"
#include "NFuncDecl.h"
#include "NGenericsComponent.h"
#include "NCommonFuncDeclComponent.h"
#include "NLambdaVarDecl.h"

namespace Citron
{

using RFactoryPtr = std::shared_ptr<class RFactory>;

class NLambdaDecl
    : public NDecl
    , public NTypeDecl
    , public NFuncDeclOuter
    , public NFuncDecl
    , public RLambdaDecl
    , private NGenericsComponent
    , private NCommonFuncDeclComponent
{
    NFuncDeclOuter* outer;
    RName name;

    // 가지고 있어야 할 멤버 변수들, type, name, ref 여부
    std::optional<std::vector<NLambdaVarDecl*>> vars;
    std::unordered_map<RName, NLambdaVarDecl*> varsMap;

public:
    NSYMBOL_API NLambdaDecl(NFuncDeclOuter* outer, RName&& name);
    NSYMBOL_API void Init(RFuncReturn&& funcReturn, std::vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic);
    NSYMBOL_API void InitVars(std::vector<NLambdaVarDecl*>&& vars);

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

    // from NFuncDecl
    // NDecl* GetNDecl() override { return this; }
    NFuncDeclOuter* GetNFuncDeclOuter() override { return outer; }
    RFuncDecl* GetRFuncDecl() override { return this; }
    bool IsSeqFunc() override { return NCommonFuncDeclComponent::IsSeqFunc(); }
    void Accept(NFuncDeclVisitor& visitor) override { visitor.Visit(this); }

    // from NFuncDeclOuter
    // NDecl* GetNDecl() override { return this; }
    NSYMBOL_API void Accept(NFuncDeclOuterVisitor& visitor) override;

    // from RDecl
    NSYMBOL_API RDecl* GetROuter() override;
    RAccessor GetAccessor() override { return RAccessor::Public; }
    NSYMBOL_API RIdentifier GetIdentifier() override;
    size_t GetTypeParamCount() override { return NGenericsComponent::GetTypeParamCount(); }
    RTypeParamDecl* GetTypeParam(size_t index) override { return NGenericsComponent::GetTypeParam(index); }
    NSYMBOL_API RTypeDecl* GetTypeMember(const RName& name, size_t typeParamCount) override;
    NSYMBOL_API std::optional<RDeclRes> GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
    NSYMBOL_API std::optional<RDeclRes> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount) override;

    // from RFuncDeclOuter
    // RDecl* GetRDecl() override { return this; }

    // from RFuncDecl
    // RDecl* GetRDecl() override { return this; }
    RThisKind GetThisKind() override { return NCommonFuncDeclComponent::GetThisKind(); }
    // size_t GetTypeParamCount() override { return 0; }
    size_t GetParamCount() override { return NCommonFuncDeclComponent::GetParamCount(); }
    RType* GetReturnType(RTypeArguments& typeArgs) override { return NCommonFuncDeclComponent::GetReturnType(typeArgs); }
    RFuncReturn GetFuncReturn(RTypeArguments& typeArgs) override { return NCommonFuncDeclComponent::GetFuncReturn(typeArgs); }
    RFuncParameter GetFuncParam(RTypeArguments& typeArgs, size_t index) override { return NCommonFuncDeclComponent::GetFuncParam(typeArgs, index); }
    RFuncReturn GetUnboundFuncReturn() override { return NCommonFuncDeclComponent::GetUnboundFuncReturn(); }
    std::span<RFuncParameter> GetUnboundFuncParams() override { return NCommonFuncDeclComponent::GetUnboundFuncParams(); }
};

}