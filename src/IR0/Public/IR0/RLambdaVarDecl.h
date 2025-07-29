#pragma once

#include <memory>

#include "RDecl.h"

namespace Citron {

class RType;
using RTypePtr = std::shared_ptr<RType>;

class RTypeFactory;

class RLambdaVarDecl
    : public RDecl
{
public:
    virtual RName GetName() = 0;
    virtual RTypePtr GetUnboundDeclType() = 0;
    virtual RTypePtr GetDeclType(RTypeArguments& typeArgs, RTypeFactory& factory) = 0;
    void Accept(RDeclVisitor& visitor) final { visitor.Visit(*this); }
};

// M버전이 없다


} // namespace Citron
