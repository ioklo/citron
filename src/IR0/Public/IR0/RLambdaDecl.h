#pragma once

#include <vector>
#include <optional>

#include "RDecl.h"
#include "RBodyDeclOuter.h"
#include "RTypeDecl.h"
#include "RBodyDeclOuter.h"
#include "RFuncDecl.h"
#include "RNames.h"
#include "RCommonFuncDeclComponent.h"
#include "RLambdaMemberVarDecl.h"
#include "RFuncReturn.h"

namespace Citron
{

class RLambdaDecl 
    : public RDecl
    , public RTypeDecl
    , public RBodyDeclOuter
    , public RFuncDecl
    , private RCommonFuncDeclComponent
{
    RBodyDeclOuterPtr outer;
    RName name;

    // 가지고 있어야 할 멤버 변수들, type, name, ref 여부
    std::vector<RLambdaMemberVarDecl> memberVars;

    // return은 확정이 안되었을 수 있다
    std::optional<RFuncReturn> oReturn;

public:
    void Accept(RDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RTypeDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RBodyDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RFuncDeclVisitor& visitor) override { visitor.Visit(*this); }
};

}