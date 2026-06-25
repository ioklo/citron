#pragma once
#include "RSymbolConfig.h"

#include "RFuncDeclBase.h"

namespace Citron {

class EStructCtorDecl;

enum class RStructCtorKind
{
    Normal,
    Memberwise,
    Copy,
    Move,
};

class RStructCtorDecl : public RFuncDeclBase
{
public:
    virtual RStructDecl* GetStructDecl() = 0;
    virtual RStructCtorKind GetKind() = 0;
    
    void Accept(RDeclVisitor& visitor) final { visitor.Visit(this); }
};

class REStructCtorDecl : public RStructCtorDecl
{
    EStructCtorDecl* decl;
};

} // namespace Citron
