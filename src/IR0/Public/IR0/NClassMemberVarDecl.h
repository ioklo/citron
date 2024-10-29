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
    IR0_API RTypePtr GetDeclType(RTypeArguments& typeArgs, RTypeFactory& factory);

public:
    RAccessor GetAccessor() override { return accessor; }
    IR0_API NDecl* GetOuter() override;
    IR0_API RIdentifier GetIdentifier() override;

    void Accept(NDeclVisitor& visitor) override { visitor.Visit(*this); }
};

}