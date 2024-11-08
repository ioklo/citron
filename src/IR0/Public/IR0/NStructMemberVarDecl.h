#pragma once
#include "IR0Config.h"

#include <memory>

#include "NDecl.h"
#include "RAccessor.h"
#include "RType.h"
#include "RNames.h"
#include "RStructMemberVarDecl.h"

namespace Citron
{

class NStructDecl;

class NStructMemberVarDecl
    : public NDecl
    , public RStructMemberVarDecl
{
public:
    std::weak_ptr<NStructDecl> _struct;

    RAccessor accessor;
    bool bStatic;
    RTypePtr declType; // lazy-init
    std::string name;

public:
    IR0_API NStructMemberVarDecl(std::weak_ptr<NStructDecl> _struct, RAccessor accessor, bool bStatic, std::string name);
    IR0_API void InitDeclType(const RTypePtr& declType);

public:
    // from NDecl
    RDecl* GetRDecl() override { return this; }
    NDecl* GetNOuter() override;
    void Accept(NDeclVisitor& visitor) override { visitor.Visit(*this); }

    // from RDecl
    IR0_API RDecl* GetROuter() override;
    RAccessor GetAccessor() override { return accessor; }
    IR0_API RIdentifier GetIdentifier() override;
    IR0_API std::optional<RMember> GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
    IR0_API std::optional<RMember> ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory) override;

    // from RStructMemberVarDecl
    IR0_API RTypePtr GetDeclType(RTypeArguments& typeArgs, RTypeFactory& factory) override;
};

}