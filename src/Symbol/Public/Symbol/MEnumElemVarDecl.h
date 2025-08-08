#pragma once

#include <optional>

#include "MDecl.h"
#include "MNames.h"

namespace Citron {

class MType;

class MEnumElemVarDecl
    : public MDecl
{
    MEnumElemDecl* outer;
    MName name;

    MType* declType; // lazy-init

public:
    void Accept(MDeclVisitor& visitor) override { visitor.Visit(*this); }
};


}