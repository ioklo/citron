#pragma once

#include <vector>
#include <optional>
#include <unordered_map>
#include <memory>

#include "NDecl.h"
#include "NFuncDeclOuter.h"
#include "NTypeDecl.h"
#include "NFuncDeclOuter.h"
#include "NFuncDecl.h"
#include "RNames.h"
#include "NCommonFuncDeclComponent.h"
#include "NLambdaMemberVarDecl.h"
#include "RFuncReturn.h"
#include "RLambdaDecl.h"

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
    NFuncDeclOuterWPtr outer;
    RName name;

    // 가지고 있어야 할 멤버 변수들, type, name, ref 여부
    std::optional<std::vector<std::shared_ptr<NLambdaMemberVarDecl>>> memberVars;

    //
    std::unordered_map<RName, std::shared_ptr<NLambdaMemberVarDecl>> memberVarsMap;

public:
    NLambdaDecl(NFuncDeclOuterWPtr&& outer, RName&& name, RFuncReturn&& funcReturn, std::vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic);
    void Init(std::vector<std::shared_ptr<NLambdaMemberVarDecl>>&& memberVars, std::vector<NStmtPtr>&& body);

    using NCommonFuncDeclComponent::GetReturnType;

public:
    // from NDecl
    RAccessor GetAccessor() override { return RAccessor::Public; }
    IR0_API NDecl* GetOuter() override;
    IR0_API RIdentifier GetIdentifier() override;

    // from NFuncDeclOuter, NTypeDecl, NFuncDecl
    IR0_API NDecl* GetDecl() override;
    RMember ToRMember(const std::shared_ptr<NTypeDecl>& sharedThis, const RTypeArgumentsPtr& typeArgs) override;

    // from RDecl
    IR0_API std::optional<RMember> GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;

    void Accept(NDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(NTypeDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(NFuncDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(NFuncDeclVisitor& visitor) override { visitor.Visit(*this); }
};

}