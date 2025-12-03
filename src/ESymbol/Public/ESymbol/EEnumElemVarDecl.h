#pragma once

#include <optional>

#include "EDecl.h"
#include "ENames.h"

namespace Citron {

class EType;

class EEnumElemVarDecl
    : public EDecl
{
    EEnumElemDecl* outer;
    EName name;

    EType* declType; // lazy-init

public:
    void Accept(EDeclVisitor& visitor) override { visitor.Visit(this); }
};


}