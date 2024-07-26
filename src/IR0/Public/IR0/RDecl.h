#pragma once

#include <memory>

namespace Citron
{

class RModuleDecl;
class RNamespaceDecl;
class RGlobalFuncDecl;
class RStructDecl;
class RStructConstructorDecl;
class RStructMemberFuncDecl;
class RStructMemberVarDecl;
class RClassDecl;
class RClassConstructorDecl;
class RClassMemberFuncDecl;
class RClassMemberVarDecl;
class REnumDecl;
class REnumElemDecl;
class REnumElemMemberVarDecl;
class RLambdaDecl;
class RLambdaMemberVarDecl;
class RInterfaceDecl;

class RDeclVisitor
{
public:
    virtual ~RDeclVisitor() { }
    virtual void Visit(RModuleDecl& decl) = 0;
    virtual void Visit(RNamespaceDecl& decl) = 0;
    virtual void Visit(RGlobalFuncDecl& decl) = 0;
    virtual void Visit(RStructDecl& decl) = 0;
    virtual void Visit(RStructConstructorDecl& decl) = 0;
    virtual void Visit(RStructMemberFuncDecl& decl) = 0;
    virtual void Visit(RStructMemberVarDecl& decl) = 0;
    virtual void Visit(RClassDecl& decl) = 0;
    virtual void Visit(RClassConstructorDecl& decl) = 0;
    virtual void Visit(RClassMemberFuncDecl& decl) = 0;
    virtual void Visit(RClassMemberVarDecl& decl) = 0;
    virtual void Visit(REnumDecl& decl) = 0;
    virtual void Visit(REnumElemDecl& decl) = 0;
    virtual void Visit(REnumElemMemberVarDecl& decl) = 0;
    virtual void Visit(RLambdaDecl& decl) = 0;
    virtual void Visit(RLambdaMemberVarDecl& decl) = 0;
    virtual void Visit(RInterfaceDecl& decl) = 0;
};

class RDecl
{
public:
    virtual ~RDecl() { }
    virtual void Accept(RDeclVisitor& visitor) = 0;
};

using RDeclPtr = std::shared_ptr<RDecl>;

}