#pragma once

#include "MDecl.h"
#include "MBodyDeclOuter.h"
#include "MFuncDecl.h"
#include "MAccessor.h"
#include "MNames.h"
#include "MCommonFuncDeclComponent.h"

namespace Citron
{

class MClassDecl;

class MClassMemberFuncDecl 
    : public MDecl
    , public MBodyDeclOuter
    , public MFuncDecl
    , private MCommonFuncDeclComponent
{
    std::weak_ptr<MClassDecl> _class;
    MAccessor accessor;
    MName name;
    std::vector<std::string> typeParams;
    bool bStatic;

public:
    void Accept(MDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MBodyDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MFuncDeclVisitor& visitor) override { visitor.Visit(*this); }
};

}