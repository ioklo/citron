#pragma once
#include "RSymbolConfig.h"

#include "RDecl.h"
#include "RFuncDecl.h"
#include "RFuncDeclOuter.h"

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
    , public RFuncDeclOuter
{
public:
    virtual RStructDecl* GetStructDecl() = 0;
    virtual RStructCtorKind GetKind() = 0;

    void Accept(RDeclVisitor& visitor) final { visitor.Visit(this); }
    RSYMBOL_API void Accept(RFuncDeclVisitor& visitor) final;
    void Accept(RFuncDeclOuterVisitor& visitor) final { visitor.Visit(this); }
};

class REStructCtorDecl : public RStructCtorDecl
{
    EStructCtorDecl* decl;
};

} // namespace Citron
