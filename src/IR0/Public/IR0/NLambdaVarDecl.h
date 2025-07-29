#pragma once

#include "IR0Config.h"
#include <memory>
#include <optional>

#include "RLambdaVarDecl.h"
#include "NDecl.h"

namespace Citron
{
class RType;

class NLambdaDecl;

class NLambdaVarDecl
    : public NDecl
    , public RLambdaVarDecl
{
public:
    std::weak_ptr<NLambdaDecl> lambda;
    RTypePtr type;
    RName name;

    IR0_API NLambdaVarDecl(const RTypePtr& type, const RName& name);
    IR0_API void InitLambda(const std::shared_ptr<NLambdaDecl>& lambda);

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

    // from RLambdaVarDecl
    IR0_API RName GetName() override { return name; }
    IR0_API RTypePtr GetUnboundDeclType() override;

public:
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(*this); }

};

}