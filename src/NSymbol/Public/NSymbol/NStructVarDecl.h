#pragma once

#include "NSymbolConfig.h"


#include "RSymbol/RStructVarDecl.h"
#include "NDecl.h"

namespace Citron
{
class NStructVarDecl
    : public NDecl
    , public RStructVarDecl
{
public:
    NStructDecl* _struct;

    RAccessor accessor;
    bool bStatic;
    RType* declType; // lazy-init
    std::string name;

public:
    NSYMBOL_API NStructVarDecl(NStructDecl* _struct);
    NSYMBOL_API void Init(RAccessor accessor, bool bStatic, const std::string& name, RType* declType);

    NSYMBOL_API RType* GetUnboundDeclType();

public:
    // from NDecl
    RDecl* GetRDecl() override { return this; }
    NDecl* GetNOuter() override;
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(this); }

    // from RDecl
    NSYMBOL_API RDecl* GetROuter() override;
    RAccessor GetAccessor() override { return accessor; }
    NSYMBOL_API RIdentifier GetIdentifier() override;
    NSYMBOL_API std::optional<RMember> GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
    NSYMBOL_API std::optional<RMember> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RFactory& factory) override;

    // from RStructVarDecl
    NSYMBOL_API RType* GetDeclType(RTypeArguments& typeArgs, RFactory& factory) override;
    bool IsStatic() override { return bStatic; }
};

}