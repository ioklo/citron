#pragma once

#include "RDecl.h"

namespace Citron {

class RLambdaDecl;
class RType;
class RFactory;

// TODO: [65] 2026-07-06, RLambdaDecl제거, RStructDecl을 쓰도록 변경
class RLambdaVarDecl final : public RDecl
{
    RLambdaDecl* lambda;
    RType* type;
    RName name;

public:
    RSYMBOL_API RLambdaVarDecl(RType* type, TakeRef<RName> name);
    RSYMBOL_API void InitLambda(RLambdaDecl* lambda);

    RName GetName() { return name; }
    RType* GetUnboundDeclType() { return type; }

public: // from RDecl
    RSYMBOL_API RDecl* GetOuter() override;
    RSYMBOL_API RIdentifier GetIdentifier() override;
    RSYMBOL_API size_t GetTypeParamCount() override;
    RSYMBOL_API RTypeParamDecl* GetTypeParam(size_t index) override;
    RSYMBOL_API RTypeDecl* GetTypeMember(InRef<RName> name, size_t typeParamCount) override;
    RSYMBOL_API std::optional<RDeclRes> ResolveMember(RTypeArguments* typeArgs, InRef<RName> name, size_t explicitTypeParamsExceptOuterCount) override;
    RSYMBOL_API std::optional<RDeclRes> ResolveIdentifier(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount) override;

};


} // namespace Citron
