#pragma once

#include "EDecl.h"
#include "EType.h"

#include "EAccessor.h"
#include "ENames.h"

namespace Citron
{

class EType;

class EClassVarDecl
    : public EDecl
{
    EClassDecl* _class;

    EAccessor accessor;
    bool bStatic;
    EType* declType;
    EName name;

public:
    void Accept(EDeclVisitor& visitor) override { visitor.Visit(this); }
};

}