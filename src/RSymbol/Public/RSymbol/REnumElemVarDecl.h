#pragma once
#include "RDecl.h"

namespace Citron {

class RType;
class RFactory;

class REnumElemVarDecl final : public RDecl
{
    REnumElemDecl* enumElem;
    RName name;
    RType* declType; // lazy-init

public:
    RSYMBOL_API REnumElemVarDecl(REnumElemDecl* outer, TakeRef<RName> name);
    void InitDeclType(RType* declType) { this->declType = declType; }

    RType* GetUnboundDeclType() { return declType; }

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
