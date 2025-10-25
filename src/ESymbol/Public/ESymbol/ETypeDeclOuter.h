#pragma once

namespace Citron
{

class ENamespaceDecl;
class EClassDecl;
class EStructDecl;

// 같은 unit내에서의 forward declaration
class ETypeDeclOuterVisitor;

// 보통 타입의 Outer
class ETypeDeclOuter
{
public:
    virtual ~ETypeDeclOuter() {}
    virtual void Accept(ETypeDeclOuterVisitor& visitor) = 0;
};

class ETypeDeclOuterVisitor
{
public:
    virtual ~ETypeDeclOuterVisitor() {}
    virtual void Visit(ENamespaceDecl* outer) = 0;
    virtual void Visit(EClassDecl* outer) = 0;
    virtual void Visit(EStructDecl* outer) = 0;
};

}