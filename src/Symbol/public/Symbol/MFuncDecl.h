#pragma once

namespace Citron
{

class MGlobalFuncDecl;        // top-level decl space
class MClassCtorDecl;  // construct decl space
class MClassFuncDecl;   // construct decl space
class MStructCtorDecl; // struct decl space
class MStructFuncDecl;  // struct decl space

class MFuncDeclVisitor
{
public:
    virtual ~MFuncDeclVisitor() { }
    virtual void Visit(MGlobalFuncDecl& func) = 0;
    virtual void Visit(MClassCtorDecl& func) = 0;
    virtual void Visit(MClassFuncDecl& func) = 0;
    virtual void Visit(MStructCtorDecl& func) = 0;
    virtual void Visit(MStructFuncDecl& func) = 0;
};

class MFuncDecl
{
public:
    virtual ~MFuncDecl() { }
    virtual void Accept(MFuncDeclVisitor& visitor) = 0;
};

using MFuncDeclPtr = std::shared_ptr<MFuncDecl>;

}