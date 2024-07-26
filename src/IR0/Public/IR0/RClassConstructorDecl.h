#pragma once
#include <vector>
#include <memory>

#include "RDecl.h"
#include "RBodyDeclOuter.h"
#include "RFuncDecl.h"
#include "RAccessor.h"
#include "RCommonFuncDeclComponent.h"

namespace Citron
{

class RClassDecl;
class RFuncParameter;

class RClassConstructorDecl 
    : public RDecl
    , public RBodyDeclOuter
    , public RFuncDecl
    , private RCommonFuncDeclComponent
{
    std::weak_ptr<RClassDecl> _class;
    RAccessor accessor;
    std::vector<RFuncParameter> parameters;
    bool bTrivial;

public:
    void Accept(RDeclVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RBodyDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(RFuncDeclVisitor& visitor) override { visitor.Visit(*this); }
};


}