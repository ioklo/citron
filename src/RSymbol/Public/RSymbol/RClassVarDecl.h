#pragma once
#include "RSymbolConfig.h"
#include "RDecl.h"

namespace Citron {

class RType;
class RTypeArguments;

class RClassVarDecl final : public RDecl
{
    RClassDecl* _class;

    RClassMemberAccessor accessor;
    bool bStatic;
    RType* declType;
    RName name;

public:
    RSYMBOL_API RClassVarDecl(RClassDecl* _class, RClassMemberAccessor accessor, bool bStatic, RType* declType, TakeRef<RName> name);

    RType* GetUnboundDeclType() { return declType; }
    bool IsStatic() { return bStatic; }

public: // from RDecl
    RSYMBOL_API RDecl* GetOuter() override;
    RSYMBOL_API RIdentifier GetIdentifier() override;
    RSYMBOL_API size_t GetTypeParamCount() override;
    RSYMBOL_API RTypeParamDecl* GetTypeParam(size_t index) override;
    RSYMBOL_API RTypeDecl* GetTypeMember(InRef<RName> name, size_t typeParamCount) override;
    RSYMBOL_API std::optional<RDeclRes> GetMember(RTypeArguments* typeArgs, InRef<RName> name, size_t explicitTypeParamsExceptOuterCount) override;
    RSYMBOL_API std::optional<RDeclRes> ResolveIdentifier(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount) override;
};

} // namespace Citron
