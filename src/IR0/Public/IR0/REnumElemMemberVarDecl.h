#pragma once

#include <memory>

#include "RDecl.h"

namespace Citron {

class MEnumElemMemberVarDecl;

class REnumElemMemberVarDecl
    : public RDecl
{
public:
    void Accept(RDeclVisitor& visitor) final { visitor.Visit(*this); }
};

class RMEnumElemMemberVarDecl : public REnumElemMemberVarDecl
{
    std::shared_ptr<MEnumElemMemberVarDecl> decl;
};


} // namespace Citron
