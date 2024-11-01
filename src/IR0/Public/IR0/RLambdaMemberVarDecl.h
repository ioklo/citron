#pragma once

#include <memory>
#include "RDecl.h"

namespace Citron {

class RLambdaMemberVarDecl
    : public RDecl
{
public:
    virtual RTypePtr GetDeclType(RTypeArguments& typeArgs, RTypeFactory& factory) = 0;
    void Accept(RDeclVisitor& visitor) final { visitor.Visit(*this); }
};

// M버전이 없다


} // namespace Citron
