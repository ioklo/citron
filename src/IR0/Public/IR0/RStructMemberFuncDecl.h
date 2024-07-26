#pragma once

#include <memory>
#include <vector>

#include "RDecl.h"
#include "RBodyDeclOuter.h"
#include "RFuncDecl.h"
#include "RAccessor.h"
#include "RNames.h"
#include "RCommonFuncDeclComponent.h"

namespace Citron
{

class RStructDecl;

class RStructMemberFuncDecl 
    : public RDecl
    , public RBodyDeclOuter
    , public RFuncDecl
    , private RCommonFuncDeclComponent
{
    std::weak_ptr<RStructDecl> _struct;
    RAccessor accessor;
    RName name;
    std::vector<std::string> typeParams;
    bool bStatic;
    
public:
    void Accept(RDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RBodyDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RFuncDeclVisitor& visitor) override { visitor.Visit(*this); }
};

}