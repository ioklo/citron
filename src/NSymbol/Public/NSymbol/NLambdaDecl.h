#pragma once
#include "NSymbolConfig.h"

#include <vector>
#include <optional>
#include <unordered_map>

#include "RSymbol/RLambdaDecl.h"

#include "NDecl.h"
#include "NFuncDeclOuter.h"
#include "NTypeDecl.h"
#include "NFuncDeclOuter.h"
#include "NFuncDecl.h"
#include "NCommonFuncDeclComponent.h"
#include "NLambdaVarDecl.h"

namespace Citron
{
class NLambdaDecl
    : public NDecl
    , public NTypeDecl
    , public NFuncDeclOuter
    , public NFuncDecl
    , public RLambdaDecl
    , private NCommonFuncDeclComponent
{
    NFuncDeclOuter* outer;
    RName name;

    // 가지고 있어야 할 멤버 변수들, type, name, ref 여부
    std::optional<std::vector<NLambdaVarDecl*>> vars;

    //
    std::unordered_map<RName, NLambdaVarDecl*> varsMap;

public:
    NSYMBOL_API NLambdaDecl(NFuncDeclOuter* outer, RName&& name);
    NSYMBOL_API void Init(RFuncReturn&& funcReturn, std::vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic);
    NSYMBOL_API void InitVars(std::vector<NLambdaVarDecl*>&& vars);

    using NCommonFuncDeclComponent::GetReturnType;

public:
    // from NDecl
    RDecl* GetRDecl() override { return this; }
    NDecl* GetNOuter() override;
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(this); }

    // from NTypeDecl
    NDecl* GetNDecl() override { return this; }
    RMember ToRMember(RTypeArguments* typeArgs) override;
    void Accept(NTypeDeclVisitor& visitor) override { visitor.Visit(this); }

    // from NFuncDecl
    // NDecl* GetNDecl() override { return this; }
    RFuncReturn GetUnboundFuncReturn() override { return NCommonFuncDeclComponent::GetUnboundFuncReturn(); }
    bool IsSeqFunc() override { return NCommonFuncDeclComponent::IsSeqFunc(); }
    void Accept(NFuncDeclVisitor& visitor) override { visitor.Visit(this); }

    // from NFuncDeclOuter
    // NDecl* GetNDecl() override { return this; }
    void Accept(NFuncDeclOuterVisitor& visitor) override { visitor.Visit(this); }

    // from RDecl
    NSYMBOL_API RDecl* GetROuter() override;
    RAccessor GetAccessor() override { return RAccessor::Public; }
    NSYMBOL_API RIdentifier GetIdentifier() override;
    NSYMBOL_API std::optional<RMember> GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
    NSYMBOL_API std::optional<RMember> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RFactory& factory) override;

    // from RFuncDeclOuter
    // RDecl* GetRDecl() override { return this; }

    // from RFuncDecl
    NSYMBOL_API bool IsStatic() override { return NCommonFuncDeclComponent::IsStatic(); }
    NSYMBOL_API size_t GetTypeParamCount() override { return NCommonFuncDeclComponent::GetTypeParamCount(); }
    NSYMBOL_API size_t GetParamCount() override { return NCommonFuncDeclComponent::GetParamCount(); }
    NSYMBOL_API RType* GetReturnType(RTypeArguments& typeArgs, RFactory& factory) override { return NCommonFuncDeclComponent::GetReturnType(typeArgs, factory); }
    NSYMBOL_API RFuncReturn GetFuncReturn(RTypeArguments& typeArgs, RFactory& factory) override { return NCommonFuncDeclComponent::GetFuncReturn(typeArgs, factory); }
    NSYMBOL_API RFuncParameter GetFuncParam(RTypeArguments& typeArgs, size_t index, RFactory& factory) override { return NCommonFuncDeclComponent::GetFuncParam(typeArgs, index, factory); }
};

}