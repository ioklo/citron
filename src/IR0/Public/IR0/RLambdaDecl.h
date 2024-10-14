#pragma once

#include <vector>
#include <optional>

#include "RDecl.h"
#include "RFuncDeclOuter.h"
#include "RTypeDecl.h"
#include "RFuncDeclOuter.h"
#include "RFuncDecl.h"
#include "RNames.h"
#include "RCommonFuncDeclComponent.h"
#include "RLambdaMemberVarDecl.h"
#include "RFuncReturn.h"

namespace Citron
{

class RLambdaDecl 
    : public RTypeDecl
    , public RFuncDeclOuter
    , public RFuncDecl
    , private RCommonFuncDeclComponent
{
    RFuncDeclOuterPtr outer;
    RName name;

    // 가지고 있어야 할 멤버 변수들, type, name, ref 여부
    std::optional<std::vector<RLambdaMemberVarDecl>> memberVars;

public:
    RLambdaDecl(RFuncDeclOuterPtr&& outer, RName name);
    void Init(std::vector<RLambdaMemberVarDecl>&& memberVars, RFuncReturn&& funcReturn, std::vector<RFuncParameter>&& funcParameters, bool bLastParameterVariadic, std::vector<RStmtPtr>&& body);

    using RCommonFuncDeclComponent::GetReturnType;

public:
    // from RDecl
    IR0_API RDecl* GetOuter() override;
    IR0_API RIdentifier GetIdentifier() override;

    // from RFuncDeclOuter
    IR0_API RDecl* GetDecl() override;

    void Accept(RDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RTypeDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RFuncDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RFuncDeclVisitor& visitor) override { visitor.Visit(*this); }
};

}