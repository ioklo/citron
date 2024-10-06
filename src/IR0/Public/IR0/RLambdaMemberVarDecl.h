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

    IR0_API RTypePtr GetDeclType(RTypeArguments& typeArgs, RTypeFactory& factory);

public:
    IR0_API RDecl* GetOuter() override;
    IR0_API RIdentifier GetIdentifier() override;

public:
    void Accept(RDeclVisitor& visitor) override { visitor.Visit(*this); }
    
};

}