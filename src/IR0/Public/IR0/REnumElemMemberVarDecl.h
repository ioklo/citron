#pragma once
#include "IR0Config.h"

#include <memory>

#include "RDecl.h"

namespace Citron {

class MEnumElemMemberVarDecl;

class REnumElemMemberVarDecl
    : public RDecl
{
public:
    virtual RTypePtr GetDeclType(RTypeArguments& typeArgs, RTypeFactory& factory) = 0;
    void Accept(RDeclVisitor& visitor) final { visitor.Visit(*this); }
};

class RMEnumElemMemberVarDecl : public REnumElemMemberVarDecl
{
    std::shared_ptr<MEnumElemMemberVarDecl> decl;
};


} // namespace Citron
