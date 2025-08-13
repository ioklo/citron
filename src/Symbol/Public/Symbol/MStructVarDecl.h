#pragma once

#include "MDecl.h"
#include "MType.h"

#include "MAccessor.h"
#include "MNames.h"

namespace Citron
{

class MType;

class MStructVarDecl
    : public MDecl
{
    MStructDecl* _struct;

    MAccessor accessor;
    bool bStatic;
    MType* declType;
    MName name;

public:
    void Accept(MDeclVisitor& visitor) override { visitor.Visit(this); }
};



}