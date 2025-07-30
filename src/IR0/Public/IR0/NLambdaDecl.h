#pragma once

#include "IR0Config.h"
#include <vector>
#include <optional>
#include <unordered_map>
#include <memory>

#include "RLambdaDecl.h"

#include "NDecl.h"
#include "NFuncDeclOuter.h"
#include "NTypeDecl.h"
#include "NFuncDeclOuter.h"
#include "NFuncDecl.h"
#include "NCommonFuncDeclComponent.h"
#include "NLambdaVarDecl.h"

namespace Citron
{
class NStmt;
using NStmtPtr = std::shared_ptr<NStmt>;

class NLambdaDecl
    : public NDecl
    , public NTypeDecl
    , public NFuncDeclOuter
    , public NFuncDecl
    , public RLambdaDecl
    , private NCommonFuncDeclComponent
{
    NFuncDeclOuterWPtr outer;
    RName name;

    // 가지고 있어야 할 멤버 변수들, type, name, ref 여부
    std::optional<std::vector<std::shared_ptr<NLambdaVarDecl>>> vars;

    //
    std::unordered_map<RName, std::shared_ptr<NLambdaVarDecl>> varsMap;

public:
    NLambdaDecl(NFuncDeclOuterWPtr&& outer, RName&& name, RFuncReturn&& funcReturn, std::vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic);
    void Init(std::vector<std::shared_ptr<NLambdaVarDecl>>&& vars, std::vector<NStmtPtr>&& body);

    using NCommonFuncDeclComponent::GetReturnType;

public:
    // from NDecl
    RDecl* GetRDecl() override { return this; }
    NDecl* GetNOuter() override;
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(*this); }

    // from NTypeDecl
    NDecl* GetNDecl() override { return this; }
    RMember ToRMember(const std::shared_ptr<NTypeDecl>& sharedThis, const RTypeArgumentsPtr& typeArgs) override;
    void Accept(NTypeDeclVisitor& visitor) override { visitor.Visit(*this); }

    // from NFuncDecl
    // NDecl* GetNDecl() override { return this; }
    RFuncReturn GetUnboundFuncReturn() override { return NCommonFuncDeclComponent::GetUnboundFuncReturn(); }
    bool IsSeqFunc() override { return NCommonFuncDeclComponent::IsSeqFunc(); }
    void Accept(NFuncDeclVisitor& visitor) override { visitor.Visit(*this); }

    // from NFuncDeclOuter
    // NDecl* GetNDecl() override { return this; }
    void Accept(NFuncDeclOuterVisitor& visitor) override { visitor.Visit(*this); }

    // from RDecl
    IR0_API RDecl* GetROuter() override;
    RAccessor GetAccessor() override { return RAccessor::Public; }
    IR0_API RIdentifier GetIdentifier() override;
    IR0_API std::optional<RMember> GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
    IR0_API std::optional<RMember> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory) override;

    // from RFuncDeclOuter
    // RDecl* GetRDecl() override { return this; }

    // from RFuncDecl
    IR0_API bool IsStatic() override { return NCommonFuncDeclComponent::IsStatic(); }
    IR0_API size_t GetTypeParamCount() override { return NCommonFuncDeclComponent::GetTypeParamCount(); }
    IR0_API size_t GetParamCount() override { return NCommonFuncDeclComponent::GetParamCount(); }
    IR0_API RTypePtr GetReturnType(RTypeArguments& typeArgs, RTypeFactory& factory) override { return NCommonFuncDeclComponent::GetReturnType(typeArgs, factory); }
    IR0_API RFuncReturn GetFuncReturn(RTypeArguments& typeArgs, RTypeFactory& factory) override { return NCommonFuncDeclComponent::GetFuncReturn(typeArgs, factory); }
    IR0_API RFuncParameter GetFuncParam(RTypeArguments& typeArgs, size_t index, RTypeFactory& factory) override { return NCommonFuncDeclComponent::GetFuncParam(typeArgs, index, factory); }
};

}