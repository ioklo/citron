#pragma once

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

    IR0_API RTypePtr GetDeclType(RTypeArguments& typeArgs, RTypeFactory& factory);

public:
    RAccessor GetAccessor() override { return accessor; }
    IR0_API NDecl* GetOuter() override;
    IR0_API RIdentifier GetIdentifier() override;

    void Accept(NDeclVisitor& visitor) override { visitor.Visit(*this); }
};

}