#pragma once
#include "RDecl.h"

namespace Citron {

class RType;
class RFactory;

class REnumElemVarDecl : public RDecl
{
public:
    virtual RType* GetDeclType(RTypeArguments* typeArgs) = 0;
};


} // namespace Citron
