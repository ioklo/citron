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

class MClassVarDecl
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