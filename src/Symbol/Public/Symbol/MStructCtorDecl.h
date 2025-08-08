#pragma once

#include <vector>

#include "MDecl.h"
#include "MBodyDeclOuter.h"
#include "MFuncDecl.h"
#include "MCommonFuncDeclComponent.h"

#include "MAccessor.h"

namespace Citron
{

class MStructCtorDecl
    : public MDecl
    , public MBodyDeclOuter
    , public MFuncDecl
    , private MCommonFuncDeclComponent
{
    MStructDecl* _struct;
    MAccessor accessor;
    std::vector<MFuncParameter> parameters;
    bool bTrivial;

public:
    void Accept(MDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MBodyDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MFuncDeclVisitor& visitor) override { visitor.Visit(*this); }
};


}