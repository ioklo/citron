#pragma once

namespace Citron
{

class EGlobalFuncDecl;
class EClassCtorDecl;
class EClassFuncDecl;
class EStructCtorDecl;
class EStructFuncDecl;

class EFuncDeclVisitor;

class EFuncDecl
{
public:
    virtual ~EFuncDecl() {}
    virtual void Accept(EFuncDeclVisitor& visitor) = 0;
};

class EFuncDeclVisitor
{
public:
    virtual ~EFuncDeclVisitor() {}
    virtual void Visit(EGlobalFuncDecl* func) = 0;
    virtual void Visit(EClassCtorDecl* func) = 0;
    virtual void Visit(EClassFuncDecl* func) = 0;
    virtual void Visit(EStructCtorDecl* func) = 0;
    virtual void Visit(EStructFuncDecl* func) = 0;
};

class EFuncDeclOuter
{
public:
    ~EFuncDeclOuter() = default;
};

}
