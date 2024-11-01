#pragma once

#include "NDecl.h"
#include "RAccessor.h"
#include "RType.h"
#include "RNames.h"
#include "RClassMemberVarDecl.h"

namespace Citron
{

class NClassDecl;

class NClassMemberVarDecl
    : public NDecl
    , public RClassMemberVarDecl
{
public:
    std::weak_ptr<NClassDecl> _class;

    RAccessor accessor;
    bool bStatic;
    RTypePtr declType;
    RName name;

public:
    RAccessor GetAccessor() override { return accessor; }
    IR0_API NDecl* GetOuter() override;
    IR0_API RIdentifier GetIdentifier() override;

    // from RClassMemberVarDecl
    IR0_API RTypePtr GetDeclType(RTypeArguments& typeArgs, RTypeFactory& factory) override;

    // from RDecl
    IR0_API std::optional<RMember> GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;

    void Accept(NDeclVisitor& visitor) override { visitor.Visit(*this); }
};

}