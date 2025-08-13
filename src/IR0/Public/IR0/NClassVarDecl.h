#pragma once

#include "IR0Config.h"

#include "RClassVarDecl.h"
#include "RAccessor.h"
#include "RTypes.h"

#include "NDecl.h"

namespace Citron
{

class NClassDecl;

class NClassVarDecl
    : public NDecl
    , public RClassVarDecl
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
    NDecl* GetNOuter() override;
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(this); }

    // from RDecl
    IR0_API RDecl* GetROuter() override;
    RAccessor GetAccessor() override { return accessor; }
    IR0_API RIdentifier GetIdentifier() override;
    IR0_API std::optional<RMember> GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
    IR0_API std::optional<RMember> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, IR0Factory& factory) override;

    // from RClassVarDecl
    IR0_API RType* GetDeclType(RTypeArguments& typeArgs, IR0Factory& factory) override;
};

}