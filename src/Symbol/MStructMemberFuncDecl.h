#pragma once

#include <memory>
#include <vector>


#include "SymbolMacros.h"
#include "MDecl.h"
#include "MBodyDeclOuter.h"
#include "MFuncDecl.h"
#include "MAccessor.h"
#include "MNames.h"
#include "MCommonFuncDeclComponent.h"

namespace Citron
{

class MStructDecl;

class MStructMemberFuncDecl 
    : public MDecl
    , public MBodyDeclOuter
    , public MFuncDecl
    , private MCommonFuncDeclComponent
{
    std::weak_ptr<MStructDecl> _struct;
    MAccessor accessor;
    MName name;
    std::vector<std::string> typeParams;
    bool bStatic;
    
public:
    // DECLARE_DEFAULTS(MStructMemberFuncDecl)
    void Accept(MDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MBodyDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(MFuncDeclVisitor& visitor) override { visitor.Visit(*this); }
};

}