#pragma once

#include "RDecl.h"

namespace Citron {

class RType;
class RFactory;

class RLambdaVarDecl : public RDecl
{
public:
    virtual RName GetName() = 0;
    virtual RType* GetUnboundDeclType() = 0;
    virtual RType* GetDeclType(RTypeArguments* typeArgs) = 0;
};


} // namespace Citron
