#pragma once

#include "MDecl.h"
#include "MAccessor.h"
#include "MType.h"
#include "MNames.h"

namespace Citron
{

class MClassDecl;

class MClassMemberVarDecl
    : public MDecl
{
    std::weak_ptr<MClassDecl> _class;

    MAccessor accessor;
    bool bStatic;
    MTypePtr declType;
    MName name;

public:
    void Accept(MDeclVisitor& visitor) override { visitor.Visit(*this); }
};

}