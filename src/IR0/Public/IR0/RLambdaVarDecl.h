#pragma once


#include "RDecl.h"

namespace Citron {

class RType;
class IR0Factory;

class RLambdaVarDecl
    : public RDecl
{
public:
    virtual RName GetName() = 0;
    virtual RType* GetUnboundDeclType() = 0;
    virtual RType* GetDeclType(RTypeArguments& typeArgs, IR0Factory& factory) = 0;
    void Accept(RDeclVisitor& visitor) final { visitor.Visit(this); }
};

// M버전이 없다


} // namespace Citron
