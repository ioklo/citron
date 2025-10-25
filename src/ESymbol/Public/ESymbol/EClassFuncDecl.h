#pragma once

#include <vector>
#include <string>

#include "EDecl.h"
#include "EBodyDeclOuter.h"

#include "EFuncDecl.h"
#include "ECommonFuncDeclComponent.h"

#include "EAccessor.h"
#include "ENames.h"

namespace Citron
{
class EClassFuncDecl
    : public EDecl
    , public EBodyDeclOuter
    , public EFuncDecl
    , private ECommonFuncDeclComponent
{
    EClassDecl* _class;
    EAccessor accessor;
    EName name;
    std::vector<std::string> typeParams;
    bool bStatic;

public:
    void Accept(EDeclVisitor& visitor) override { visitor.Visit(this); }
    void Accept(EBodyDeclOuterVisitor& visitor) override { visitor.Visit(*this); }
    void Accept(EFuncDeclVisitor& visitor) override { visitor.Visit(this); }
};

}