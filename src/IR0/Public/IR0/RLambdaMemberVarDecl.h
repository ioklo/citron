#pragma once

#include <memory>
#include "RDecl.h"

namespace Citron {

class RLambdaMemberVarDecl
    : public RDecl
{
public:
    void Accept(RDeclVisitor& visitor) final { visitor.Visit(*this); }
};

// M버전이 없다


} // namespace Citron
