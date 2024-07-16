#pragma once

#include <vector>
#include <optional>

#include "MDecl.h"
#include "MBodyDeclOuter.h"
#include "MTypeDecl.h"
#include "MBodyDeclOuter.h"
#include "MFuncDecl.h"
#include "MNames.h"
#include "MCommonFuncDeclComponent.h"
#include "MLambdaMemberVarDecl.h"
#include "MFuncReturn.h"

namespace Citron
{

class MLambdaDecl 
    : public MDecl
    , public MTypeDecl
    , public MBodyDeclOuter
    , public MFuncDecl
    , private MCommonFuncDeclComponent
{
    std::weak_ptr<MBodyDeclOuter> outer;
    MName name;

    // 가지고 있어야 할 멤버 변수들, type, name, ref 여부
    std::vector<MLambdaMemberVarDecl> memberVars;

    // return은 확정이 안되었을 수 있다
    std::optional<MFuncReturn> oReturn;

public:
    void Accept(MDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MTypeDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MBodyDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MFuncDeclVisitor& visitor) override { visitor.Visit(*this); }
};

}