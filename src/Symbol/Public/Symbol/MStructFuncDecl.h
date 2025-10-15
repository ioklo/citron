#pragma once

#include <vector>
#include <string>

#include "MDecl.h"
#include "MBodyDeclOuter.h"
#include "MFuncDecl.h"
#include "MCommonFuncDeclComponent.h"

#include "MAccessor.h"
#include "MNames.h"

namespace Citron
{

class MStructFuncDecl
    : public MDecl
    , public MBodyDeclOuter
    , public MFuncDecl
    , private MCommonFuncDeclComponent
{
    MStructDecl* _struct;
    MAccessor accessor;
    MName name;
    std::vector<std::string> typeParams;
    bool bStatic;

public:
    void Accept(MDeclVisitor& visitor) override { visitor.Visit(this); }
    void Accept(MBodyDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MFuncDeclVisitor& visitor) override { visitor.Visit(this); }
};

}