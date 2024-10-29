#pragma once

#include <memory>

#include "RDecl.h"
#include "RTypeDecl.h"

namespace Citron {

class MEnumElemDecl;

class REnumElemDecl
    : public RDecl
    , public RTypeDecl
{
public:
    void Accept(RDeclVisitor& visitor) final { visitor.Visit(*this); }
    void Accept(RTypeDeclVisitor& visitor) final { visitor.Visit(*this); }
};

class RMEnumElemDecl : public REnumElemDecl
{
    std::shared_ptr<MEnumElemDecl> decl;
};


} // namespace Citron
