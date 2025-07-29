#pragma once

#include <memory>

#include "RDecl.h"
#include "RTypeDecl.h"

namespace Citron {

class MEnumDecl;

class REnumDecl
    : public RDecl
    , public RTypeDecl
{
public:
    void Accept(RDeclVisitor& visitor) final { visitor.Visit(*this); }
    void Accept(RTypeDeclVisitor& visitor) final { visitor.Visit(*this); }
};

class RMEnumDecl : public REnumDecl
{
    std::shared_ptr<MEnumDecl> decl;
};


} // namespace Citron
