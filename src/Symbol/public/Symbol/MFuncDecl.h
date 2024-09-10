#pragma once

namespace Citron
{

class MGlobalFuncDecl;        // top-level decl space
class MClassConstructorDecl;  // construct decl space
class MClassMemberFuncDecl;   // construct decl space
class MStructConstructorDecl; // struct decl space
class MStructMemberFuncDecl;  // struct decl space

class MFuncDeclVisitor
{
public:
    virtual ~MFuncDeclVisitor() { }
    virtual void Visit(MGlobalFuncDecl& func) = 0;
    virtual void Visit(MClassConstructorDecl& func) = 0;
    virtual void Visit(MClassMemberFuncDecl& func) = 0;
    virtual void Visit(MStructConstructorDecl& func) = 0;
    virtual void Visit(MStructMemberFuncDecl& func) = 0;
};

class MFuncDecl
{
public:
    virtual ~MFuncDecl() { }
    virtual void Accept(MFuncDeclVisitor& visitor) = 0;
};

using MFuncDeclPtr = std::shared_ptr<MFuncDecl>;

}