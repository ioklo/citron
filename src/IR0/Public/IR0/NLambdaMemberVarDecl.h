#pragma once

#include <memory>

#include "NDecl.h"
#include "RNames.h"
#include "RType.h"
#include "RLambdaMemberVarDecl.h"

namespace Citron
{

class NLambdaDecl;

class NLambdaMemberVarDecl
    : public NDecl
    , public RLambdaMemberVarDecl
{
public:
    std::weak_ptr<NLambdaDecl> lambda;
    RTypePtr type;
    RName name;

    IR0_API NLambdaMemberVarDecl(std::weak_ptr<NLambdaDecl>&& lambda, const RTypePtr& type, RName name);
    IR0_API RTypePtr GetDeclType(RTypeArguments& typeArgs, RTypeFactory& factory);

public:
    RAccessor GetAccessor() override { return RAccessor::Public; }
    IR0_API NDecl* GetOuter() override;
    IR0_API RIdentifier GetIdentifier() override;

public:
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(*this); }
    
};

}