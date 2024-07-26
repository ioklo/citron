#pragma once

#include <memory>

#include "MDecl.h"
#include "MAccessor.h"
#include "MType.h"
#include "MNames.h"

namespace Citron
{

class MStructDecl;

class MStructMemberVarDecl
    : public MDecl
{
    std::weak_ptr<MStructDecl> _struct;

    MAccessor accessor;
    bool bStatic;
    MTypePtr declType;
    MName name;

public:
    void Accept(MDeclVisitor& visitor) override { visitor.Visit(*this); }
};



}