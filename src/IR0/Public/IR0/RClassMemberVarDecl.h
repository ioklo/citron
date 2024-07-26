#pragma once

#include "RDecl.h"
#include "RAccessor.h"
#include "RType.h"
#include "RNames.h"

namespace Citron
{

class RClassDecl;

class RClassMemberVarDecl
    : public RDecl
{
    std::weak_ptr<RClassDecl> _class;

    RAccessor accessor;
    bool bStatic;
    RTypePtr declType;
    RName name;

public:
    void Accept(RDeclVisitor& visitor) override { visitor.Visit(*this); }
};

}