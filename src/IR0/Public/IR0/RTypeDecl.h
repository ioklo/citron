#pragma once


#include "RDecl.h"

namespace Citron {

class MTypeDecl;

class RClassDecl;
class RStructDecl;
class REnumDecl;
class REnumElemDecl;
class RInterfaceDecl;
class RLambdaDecl;

class RTypeDeclVisitor;

class RTypeDecl
{
public:
    virtual ~RTypeDecl() {}

    virtual RDecl* GetRDecl() = 0;
    virtual void Accept(RTypeDeclVisitor& visitor) = 0;
};

class RTypeDeclVisitor
{
public:
    virtual ~RTypeDeclVisitor() {}
    virtual void Visit(RClassDecl* typeDecl) = 0;
    virtual void Visit(RStructDecl* typeDecl) = 0;
    virtual void Visit(REnumDecl* typeDecl) = 0;
    virtual void Visit(REnumElemDecl* typeDecl) = 0;
    virtual void Visit(RInterfaceDecl* typeDecl) = 0;
    virtual void Visit(RLambdaDecl* typeDecl) = 0;
};

class RMTypeDecl : public RTypeDecl
{
    MTypeDecl* typeDecl;
};


} // namespace Citron