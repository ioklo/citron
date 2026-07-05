#pragma once

#include "NSymbolConfig.h"

#include "RSymbol/RClassVarDecl.h"
#include "RSymbol/RAccessor.h"
#include "RSymbol/RTypes.h"

namespace Citron
{

class NClassDecl;

class NClassVarDecl
    : public RClassVarDecl
{
public:
    NClassDecl* _class;

    RAccessor accessor;
    bool bStatic;
    RType* declType;
    RName name;

public:
    // from NDecl
    RDecl* GetRDecl() override { return this; }
    NSYMBOL_API NDecl* GetNOuter() override;
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(this); }

    // from RDecl
    NSYMBOL_API RDecl* GetROuter() override;
    RAccessor GetAccessor() override { return accessor; }
    NSYMBOL_API RIdentifier GetIdentifier() override;
    NSYMBOL_API RTypeDecl* GetTypeMember(const RName& name, size_t typeParamCount) override;
    NSYMBOL_API std::optional<RDeclRes> GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
    NSYMBOL_API std::optional<RDeclRes> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount) override;

    // from RClassVarDecl
    NSYMBOL_API RType* GetDeclType(RTypeArguments* typeArgs) override;
};

}