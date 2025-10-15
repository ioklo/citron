#pragma once

#include "MDecl.h"

namespace Citron {

// 같은 unit내에서 forward declaration
class MTypeDeclVisitor;

class MTypeDecl
{
public:
    virtual ~MTypeDecl() {}
    virtual void Accept(MTypeDeclVisitor& visitor) = 0;
};

class MTypeDeclVisitor
{
public:
    virtual ~MTypeDeclVisitor() {}
    virtual void Visit(MClassDecl* typeDecl) = 0;
    virtual void Visit(MStructDecl* typeDecl) = 0;
    virtual void Visit(MEnumDecl* typeDecl) = 0;
    virtual void Visit(MEnumElemDecl* typeDecl) = 0;
    virtual void Visit(MInterfaceDecl* typeDecl) = 0;
};

}