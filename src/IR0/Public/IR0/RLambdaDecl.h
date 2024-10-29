#pragma once

#include <memory>

#include "RDecl.h"
#include "RFuncDecl.h"
#include "RFuncDeclOuter.h"
#include "RTypeDecl.h"

namespace Citron {

class RLambdaDecl
    : public RDecl
    , public RFuncDecl
    , public RFuncDeclOuter
    , public RTypeDecl
{
public:
    void Accept(RDeclVisitor& visitor) final { visitor.Visit(*this); }
    void Accept(RFuncDeclVisitor& visitor) final { visitor.Visit(*this); }
    void Accept(RFuncDeclOuterVisitor& visitor) final { visitor.Visit(*this); }
    void Accept(RTypeDeclVisitor& visitor) final { visitor.Visit(*this); }
};

// M버전이 없다

} // namespace Citron
