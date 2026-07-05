#pragma once

#include "RDecl.h"

namespace Citron {

class RType;
class RTypeArguments;

class RClassVarDecl : public RDecl
{
public:
    virtual RType* GetDeclType(RTypeArguments* typeArgs) = 0;
    virtual bool IsStatic() = 0;
};

} // namespace Citron
