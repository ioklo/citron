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
    RTypePtr declType;
    RName name;

public:
    void Accept(RDeclVisitor& visitor) override { visitor.Visit(*this); }
};



}