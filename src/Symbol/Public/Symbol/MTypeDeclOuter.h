#pragma once

namespace Citron
{

class MNamespaceDecl;
class MClassDecl;
class MStructDecl;

// 같은 unit내에서의 forward declaration
class MTypeDeclOuterVisitor;

// 보통 타입의 Outer
class MTypeDeclOuter
{
public:
    virtual ~MTypeDeclOuter() {}
    virtual void Accept(MTypeDeclOuterVisitor& visitor) = 0;
};

class MTypeDeclOuterVisitor
{
public:
    virtual ~MTypeDeclOuterVisitor() {}
    virtual void Visit(MNamespaceDecl* outer) = 0;
    virtual void Visit(MClassDecl* outer) = 0;
    virtual void Visit(MStructDecl* outer) = 0;
};

}