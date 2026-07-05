#pragma once
#include "NSymbolConfig.h"

#include <optional>

#include "RSymbol/REnumElemVarDecl.h"

namespace Citron {

class NEnumElemVarDecl
    : public REnumElemVarDecl
{
public:
    NEnumElemDecl* enumElem;
    RName name;

    RType* declType; // lazy-init

public:
    NSYMBOL_API NEnumElemVarDecl(NEnumElemDecl* outer, const RName& name);
    NSYMBOL_API void Init(RType* declType);

public:
    // from NDecl
    RDecl* GetRDecl() override { return this; }
    NSYMBOL_API NDecl* GetNOuter() override;
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(this); }

    // from RDecl
    NSYMBOL_API RDecl* GetROuter() override;
    RAccessor GetAccessor() override { return RAccessor::Public; }
    NSYMBOL_API RIdentifier GetIdentifier() override;
    NSYMBOL_API RTypeDecl* GetTypeMember(const RName& name, size_t typeParamCount) override;
    NSYMBOL_API std::optional<RDeclRes> GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
    NSYMBOL_API std::optional<RDeclRes> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount) override;

    // from REnumElemVarDecl
    NSYMBOL_API RType* GetDeclType(RTypeArguments* typeArgs) override;
};

}