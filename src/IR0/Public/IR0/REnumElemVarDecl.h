#pragma once
#include "IR0Config.h"

#include <memory>

#include "RDecl.h"

namespace Citron {

class MEnumElemVarDecl;

class REnumElemVarDecl
    : public RDecl
{
public:
    virtual RTypePtr GetDeclType(RTypeArguments& typeArgs, RTypeFactory& factory) = 0;
    void Accept(RDeclVisitor& visitor) final { visitor.Visit(*this); }
};

class RMEnumElemVarDecl : public REnumElemVarDecl
{
    std::shared_ptr<MEnumElemVarDecl> decl;
};


} // namespace Citron
