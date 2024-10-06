#pragma once

#include <memory>

#include "RDecl.h"
#include "RAccessor.h"
#include "RType.h"
#include "RNames.h"

namespace Citron
{

class RStructDecl;

class RStructMemberVarDecl
    : public RDecl
{
public:
    std::weak_ptr<RStructDecl> _struct;

    RAccessor accessor;
    bool bStatic;
    RTypePtr declType; // lazy-init
    std::string name;

public:
    IR0_API RStructMemberVarDecl(std::weak_ptr<RStructDecl> _struct, RAccessor accessor, bool bStatic, std::string name);
    IR0_API void InitDeclType(const RTypePtr& declType);

    IR0_API RTypePtr GetDeclType(RTypeArguments& typeArgs, RTypeFactory& factory);

public:
    IR0_API RDecl* GetOuter() override;
    IR0_API RIdentifier GetIdentifier() override;

    void Accept(RDeclVisitor& visitor) override { visitor.Visit(*this); }
};

}