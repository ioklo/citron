#pragma once

#include <memory>

#include "MDecl.h"
#include "MType.h"

#include "MAccessor.h"
#include "MNames.h"

namespace Citron
{

class MType;
using MTypePtr = std::shared_ptr<MType>;

class MStructVarDecl
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