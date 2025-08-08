#pragma once

#include "MDecl.h"
#include "MType.h"

#include "MAccessor.h"
#include "MNames.h"

namespace Citron
{

class MType;

class MClassVarDecl
    : public MDecl
{
    MClassDecl* _class;

    MAccessor accessor;
    bool bStatic;
    MType* declType;
    MName name;

public:
    void Accept(MDeclVisitor& visitor) override { visitor.Visit(*this); }
};

}