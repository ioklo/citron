#pragma once
#include "RFuncDeclBase.h"

namespace Citron {

class EStructFuncDecl;

class RType;
class RFactory;

class RStructFuncDecl : public RFuncDeclBase
{
public:
    void Accept(RDeclVisitor& visitor) final { visitor.Visit(this); }
};

class REStructFuncDecl : public RStructFuncDecl
{
    EStructFuncDecl* decl;
};


} // namespace Citron
