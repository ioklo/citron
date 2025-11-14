#pragma once
#include "NSymbolConfig.h"

#include <optional>

#include "RSymbol/REnumElemVarDecl.h"
#include "NDecl.h"

namespace Citron {

class NEnumElemVarDecl
    : public NDecl
    , public REnumElemVarDecl
{
public:
    NEnumElemDecl* enumElem;
    std::string name;

    RType* declType; // lazy-init

public:
    NSYMBOL_API NEnumElemVarDecl(NEnumElemDecl* outer);
    NSYMBOL_API void Init(const std::string& name, RType* declType);

public:
    // from NDecl
    RDecl* GetRDecl() override { return this; }
    NDecl* GetNOuter() override;
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(this); }

    // from RDecl
    NSYMBOL_API RDecl* GetROuter() override;
    RAccessor GetAccessor() override { return RAccessor::Public; }
    NSYMBOL_API RIdentifier GetIdentifier() override;
    NSYMBOL_API std::optional<RMember> GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
    NSYMBOL_API std::optional<RMember> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RFactory& factory) override;

    // from REnumElemVarDecl
    NSYMBOL_API RType* GetDeclType(RTypeArguments& typeArgs, RFactory& factory) override;
};

}