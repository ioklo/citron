#pragma once

#include "EDecl.h"
#include "EType.h"

#include "EAccessor.h"
#include "ENames.h"

namespace Citron
{

class EType;

class EStructVarDecl
    : public EDecl
{
    EStructDecl* _struct;

    EAccessor accessor;
    bool bStatic;
    EType* declType;
    EName name;

public:
    void Accept(EDeclVisitor& visitor) override { visitor.Visit(this); }
};



}