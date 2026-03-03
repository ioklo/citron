#pragma once

#include <variant>

namespace Citron {

struct NFuncDeclOuterVisitor;

class NFuncDeclOuter
{
public:
    virtual ~NFuncDeclOuter() {}
    virtual NDecl* GetNDecl() = 0;
    virtual void Accept(NFuncDeclOuterVisitor& visitor) = 0;
};

}

#include "NFuncDeclOuterVisitor.g.h"