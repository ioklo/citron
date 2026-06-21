#pragma once
#include "RSymbolConfig.h"

#include "RDecl.h"
#include "RFuncDecl.h"

namespace Citron {

class EStructCtorDecl;

enum class RStructCtorKind
{
    Normal,
    Memberwise,
    Copy,
    Move,
};

class RStructCtorDecl
    : public RDecl
    , public RFuncDecl
{
public:
    virtual RStructDecl* GetStructDecl() = 0;
    virtual RStructCtorKind GetKind() = 0;

    void Accept(RDeclVisitor& visitor) final { visitor.Visit(this); }
    RSYMBOL_API void Accept(RFuncDeclVisitor& visitor) final;
};

class REStructCtorDecl : public RStructCtorDecl
{
    EStructCtorDecl* decl;
};

} // namespace Citron
