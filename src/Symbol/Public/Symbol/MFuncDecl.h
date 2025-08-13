#pragma once

namespace Citron
{

class MGlobalFuncDecl;
class MClassCtorDecl;
class MClassFuncDecl;
class MStructCtorDecl;
class MStructFuncDecl;

class MFuncDeclVisitor;

class MFuncDecl
{
public:
    virtual ~MFuncDecl() {}
    virtual void Accept(MFuncDeclVisitor& visitor) = 0;
};

class MFuncDeclVisitor
{
public:
    virtual ~MFuncDeclVisitor() {}
    virtual void Visit(MGlobalFuncDecl* func) = 0;
    virtual void Visit(MClassCtorDecl* func) = 0;
    virtual void Visit(MClassFuncDecl* func) = 0;
    virtual void Visit(MStructCtorDecl* func) = 0;
    virtual void Visit(MStructFuncDecl* func) = 0;
};

class MFuncDeclOuter
{
public:
    ~MFuncDeclOuter() = default;
};

}
