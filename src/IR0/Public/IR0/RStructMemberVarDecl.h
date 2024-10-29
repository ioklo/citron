#pragma once

#include <memory>

#include "RDecl.h"

namespace Citron {

class MStructMemberVarDecl;

class RStructMemberVarDecl
    : public RDecl
{
public:
    void Accept(RDeclVisitor& visitor) final { visitor.Visit(*this); }
};

class RMStructMemberVarDecl : public RStructMemberVarDecl
{
    std::shared_ptr<MStructMemberVarDecl> decl;
};


} // namespace Citron
