#pragma once

#include <memory>
#include <optional>

#include "RDecl.h"
#include "RNames.h"
#include "RType.h"

namespace Citron
{

class REnumElemDecl;

class REnumElemMemberVarDecl
    : public RDecl
{   
    std::weak_ptr<REnumElemDecl> outer;
    RName name;

    RTypePtr declType; // lazy-init

public:
    void Accept(RDeclVisitor& visitor) override { visitor.Visit(*this); }
};


}