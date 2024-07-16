#pragma once

#include <memory>
#include <vector>

#include "SymbolMacros.h"
#include "MDecl.h"
#include "MBodyDeclOuter.h"
#include "MFuncDecl.h"
#include "MAccessor.h"
#include "MCommonFuncDeclComponent.h"

namespace Citron
{

class MStructDecl;
class MFuncParameter;

class MStructConstructorDecl 
    : public MDecl
    , public MBodyDeclOuter
    , public MFuncDecl
    , private MCommonFuncDeclComponent
{
    std::weak_ptr<MStructDecl> _struct;
    MAccessor accessor;
    std::vector<MFuncParameter> parameters;
    bool bTrivial;

public:
    // DECLARE_DEFAULTS(MStructConstructorDecl)
    void Accept(MDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MBodyDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MFuncDeclVisitor& visitor) override { visitor.Visit(*this); }
};


}