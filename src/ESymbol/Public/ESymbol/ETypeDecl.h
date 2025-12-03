#pragma once

#include "EDecl.h"

namespace Citron {

// 같은 unit내에서 forward declaration
class ETypeDeclVisitor;

class ETypeDecl
{
public:
    virtual ~ETypeDecl() {}
    virtual void Accept(ETypeDeclVisitor& visitor) = 0;
};

class ETypeDeclVisitor
{
public:
    virtual ~ETypeDeclVisitor() {}
    virtual void Visit(EClassDecl* typeDecl) = 0;
    virtual void Visit(EStructDecl* typeDecl) = 0;
    virtual void Visit(EEnumDecl* typeDecl) = 0;
    virtual void Visit(EEnumElemDecl* typeDecl) = 0;
    virtual void Visit(EInterfaceDecl* typeDecl) = 0;
};

}