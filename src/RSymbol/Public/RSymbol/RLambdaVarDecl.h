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
    RSYMBOL_API RDecl* GetOuter() final;
    RSYMBOL_API RIdentifier GetIdentifier() final;
    RSYMBOL_API size_t GetTypeParamCount() final;
    RSYMBOL_API RTypeParam* GetTypeParam(size_t index) final;
    RSYMBOL_API RTypeParam* GetTypeParam(InRef<RName> name) final;
    RSYMBOL_API RTypeDecl* GetTypeMember(InRef<RName> name) final;
    RSYMBOL_API std::optional<RMember> GetMember(InRef<RName> name) final;
};


} // namespace Citron
