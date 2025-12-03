#pragma once

namespace Citron {

class ENamespaceDecl;
class EGlobalFuncDecl;
class EStructDecl;
class EStructCtorDecl;
class EStructFuncDecl;
class EStructVarDecl;
class EClassDecl;
class EClassCtorDecl;
class EClassFuncDecl;
class EClassVarDecl;
class EEnumDecl;
class EEnumElemDecl;
class EEnumElemVarDecl;
class EInterfaceDecl;

class EDeclVisitor;

class EDecl
{
public:
    virtual ~EDecl() {}
    virtual void Accept(EDeclVisitor& visitor) = 0;
};

class EDeclVisitor
{
public:
    virtual ~EDeclVisitor() {}
    virtual void Visit(ENamespaceDecl* decl) = 0;
    virtual void Visit(EGlobalFuncDecl* decl) = 0;
    virtual void Visit(EStructDecl* decl) = 0;
    virtual void Visit(EStructCtorDecl* decl) = 0;
    virtual void Visit(EStructFuncDecl* decl) = 0;
    virtual void Visit(EStructVarDecl* decl) = 0;
    virtual void Visit(EClassDecl* decl) = 0;
    virtual void Visit(EClassCtorDecl* decl) = 0;
    virtual void Visit(EClassFuncDecl* decl) = 0;
    virtual void Visit(EClassVarDecl* decl) = 0;
    virtual void Visit(EEnumDecl* decl) = 0;
    virtual void Visit(EEnumElemDecl* decl) = 0;
    virtual void Visit(EEnumElemVarDecl* decl) = 0;
    virtual void Visit(EInterfaceDecl* decl) = 0;
};

}