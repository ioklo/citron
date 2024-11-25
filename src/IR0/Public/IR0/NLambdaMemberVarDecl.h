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

    IR0_API NLambdaMemberVarDecl(const RTypePtr& type, const RName& name);
    IR0_API void InitLambda(const std::shared_ptr<NLambdaDecl>& lambda);

    IR0_API RTypePtr GetUnboundDeclType();

public:
    // from NDecl
    RDecl* GetRDecl() override { return this; }
    NDecl* GetNOuter() override;
    IR0_API RTypePtr GetDeclType(RTypeArguments& typeArgs, RTypeFactory& factory) override;

    // from RDecl
    IR0_API RDecl* GetROuter() override;
    RAccessor GetAccessor() override { return RAccessor::Public; }
    IR0_API RIdentifier GetIdentifier() override;
    IR0_API std::optional<RMember> GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
    IR0_API std::optional<RMember> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory) override;

public:
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(*this); }
    
};

}