#pragma once

#include <memory>
#include <vector>

#include "RDecl.h"
#include "RBodyDeclOuter.h"
#include "RFuncDecl.h"
#include "RAccessor.h"
#include "RCommonFuncDeclComponent.h"

namespace Citron
{

class RStructDecl;
class RFuncParameter;

class RStructConstructorDecl 
    : public RDecl
    , public RBodyDeclOuter
    , public RFuncDecl
    , private RCommonFuncDeclComponent
{
    std::weak_ptr<RStructDecl> _struct;
    RAccessor accessor;
    std::vector<RFuncParameter> parameters;
    bool bTrivial;

public:
    void Accept(RDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RBodyDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RFuncDeclVisitor& visitor) override { visitor.Visit(*this); }
};


}