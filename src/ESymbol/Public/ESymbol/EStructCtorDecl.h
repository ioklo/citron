#pragma once

#include <vector>

#include "EDecl.h"
#include "EBodyDeclOuter.h"
#include "EFuncDecl.h"
#include "ECommonFuncDeclComponent.h"

#include "EAccessor.h"

namespace Citron
{

class EStructCtorDecl
    : public EDecl
    , public EBodyDeclOuter
    , public EFuncDecl
    , private ECommonFuncDeclComponent
{
    EStructDecl* _struct;
    EAccessor accessor;
    std::vector<EFuncParameter> parameters;
    bool bTrivial;

public:
    void Accept(EDeclVisitor& visitor) override { visitor.Visit(this); }
    void Accept(EBodyDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(EFuncDeclVisitor& visitor) override { visitor.Visit(this); }
};


}