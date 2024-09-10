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
    std::weak_ptr<RStructDecl> _struct;

    RAccessor accessor;
    bool bStatic;
    RTypePtr declType; // lazy-init
    std::string name;

public:
    IR0_API RStructMemberVarDecl(std::weak_ptr<RStructDecl> _struct, RAccessor accessor, bool bStatic, std::string name);
    IR0_API void InitDeclType(RTypePtr declType);

    void Accept(RDeclVisitor& visitor) override { visitor.Visit(*this); }
};

}