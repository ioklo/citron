#pragma once

#include "IR0Config.h"


#include "RStructVarDecl.h"
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
    IR0_API NStructVarDecl(NStructDecl* _struct, RAccessor accessor, bool bStatic, std::string name);
    IR0_API void InitDeclType(RType* declType);

    IR0_API RType* GetUnboundDeclType();

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
    IR0_API std::optional<RMember> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RFactory& factory) override;

    // from RStructVarDecl
    IR0_API RType* GetDeclType(RTypeArguments& typeArgs, RFactory& factory) override;
    bool IsStatic() override { return bStatic; }
};

}