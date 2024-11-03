#pragma once

#include <memory>
#include "RDecl.h"

namespace Citron {

class MClassMemberVarDecl;
using RTypePtr = std::shared_ptr<class RType>;

class RClassMemberVarDecl
    : public RDecl
{
public:
    virtual RTypePtr GetDeclType(RTypeArguments& typeArgs, RTypeFactory& factory) = 0;
    virtual bool IsStatic() = 0;

    void Accept(RDeclVisitor& visitor) final { visitor.Visit(*this); }
};

class RMClassMemberVarDecl : public RClassMemberVarDecl
{
    std::shared_ptr<MClassMemberVarDecl> decl;
};


} // namespace Citron
