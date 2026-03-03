#pragma once
#include "RSymbolConfig.h"

#include "RDecl.h"
#include "RTypeDecl.h"

namespace Citron {

class EEnumDecl;

class REnumDecl
    : public RDecl
    , public RTypeDecl
{
public:
    void Accept(RDeclVisitor& visitor) final { visitor.Visit(this); }
    RSYMBOL_API void Accept(RTypeDeclVisitor& visitor) final;
};

class REEnumDecl : public REnumDecl
{
    EEnumDecl* decl;
};


} // namespace Citron
