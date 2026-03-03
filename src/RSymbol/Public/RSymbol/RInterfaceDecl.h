#pragma once
#include "RSymbolConfig.h"

#include "RDecl.h"
#include "RTypeDecl.h"

namespace Citron {

class EInterfaceDecl;

class RInterfaceDecl
    : public RDecl
    , public RTypeDecl
{
public:
    void Accept(RDeclVisitor& visitor) final { visitor.Visit(this); }
    RSYMBOL_API void Accept(RTypeDeclVisitor& visitor) final;
};

class REInterfaceDecl : public RInterfaceDecl
{
    EInterfaceDecl* decl;
};


} // namespace Citron

