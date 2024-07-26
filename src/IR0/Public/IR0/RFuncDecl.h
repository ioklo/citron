#pragma once

#include <memory>

namespace Citron
{

class RGlobalFuncDecl;        // top-level decl space
class RClassConstructorDecl;  // construct decl space
class RClassMemberFuncDecl;   // construct decl space
class RStructConstructorDecl; // struct decl space
class RStructMemberFuncDecl;  // struct decl space
class RLambdaDecl;            // body space

class RFuncDeclVisitor
{
public:
    virtual ~RFuncDeclVisitor() { }
    virtual void Visit(RGlobalFuncDecl& func) = 0;
    virtual void Visit(RClassConstructorDecl& func) = 0;
    virtual void Visit(RClassMemberFuncDecl& func) = 0;
    virtual void Visit(RStructConstructorDecl& func) = 0;
    virtual void Visit(RStructMemberFuncDecl& func) = 0;
    virtual void Visit(RLambdaDecl& func) = 0;
};

class RFuncDecl
{
public:
    virtual ~RFuncDecl() { }
    virtual void Accept(RFuncDeclVisitor& visitor) = 0;
};

using RFuncDeclPtr = std::shared_ptr<RFuncDecl>;

}