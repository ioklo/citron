#pragma once

#include "NSymbolConfig.h"


#include "RSymbol/RStructVarDecl.h"

namespace Citron
{
class NStructVarDecl
    : public RStructVarDecl
{
public:
    NStructDecl* _struct;

    RAccessor accessor;
    bool bStatic;
    RType* declType; // lazy-init
    std::string name;
    size_t index;

public:
    NSYMBOL_API NStructVarDecl(NStructDecl* _struct, RAccessor accessor, bool bStatic, const std::string& name, RType* declType, size_t index);
    NSYMBOL_API RType* GetUnboundDeclType();

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

    // from RStructVarDecl
    NSYMBOL_API RType* GetDeclType(RTypeArguments* typeArgs) override;
    bool IsStatic() override { return bStatic; }
    size_t GetIndex() override { return index; }
};

}