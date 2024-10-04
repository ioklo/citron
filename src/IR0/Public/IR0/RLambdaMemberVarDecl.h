#pragma once

#include <memory>

#include "RDecl.h"
#include "RNames.h"
#include "RType.h"

namespace Citron
{

class RLambdaDecl;

class RLambdaMemberVarDecl
    : public RDecl
{
public:
    std::weak_ptr<RLambdaDecl> lambda;
    RTypePtr type;
    RName name;

public:
    void Accept(RDeclVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr GetDeclType(RTypeArguments& typeArgs, RTypeFactory& factory);
};

}