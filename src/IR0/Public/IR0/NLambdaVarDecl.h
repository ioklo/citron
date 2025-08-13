#pragma once
#include "IR0Config.h"

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
    NLambdaDecl* lambda;
    RType* type;
    RName name;

    IR0_API NLambdaVarDecl(RType* type, const RName& name);
    IR0_API void InitLambda(NLambdaDecl* lambda);

public:
    // from NDecl
    RDecl* GetRDecl() override { return this; }
    NDecl* GetNOuter() override;
    IR0_API RType* GetDeclType(RTypeArguments& typeArgs, IR0Factory& factory) override;

    // from RDecl
    IR0_API RDecl* GetROuter() override;
    RAccessor GetAccessor() override { return RAccessor::Public; }
    IR0_API RIdentifier GetIdentifier() override;
    IR0_API std::optional<RMember> GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
    IR0_API std::optional<RMember> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, IR0Factory& factory) override;

    // from RLambdaVarDecl
    IR0_API RName GetName() override { return name; }
    IR0_API RType* GetUnboundDeclType() override;

public:
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(this); }

};

}