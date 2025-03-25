export module Citron.MSymbol:MFuncDecl;

import <memory>;

namespace Citron
{

export class MGlobalFuncDecl; // top-level decl space
export class MClassCtorDecl;  // construct decl space
export class MClassFuncDecl;   // construct decl space
export class MStructCtorDecl; // struct decl space
export class MStructFuncDecl;  // struct decl space

export class MFuncDeclVisitor
{
public:
    virtual ~MFuncDeclVisitor() {}
    virtual void Visit(MGlobalFuncDecl& func) = 0;
    virtual void Visit(MClassCtorDecl& func) = 0;
    virtual void Visit(MClassFuncDecl& func) = 0;
    virtual void Visit(MStructCtorDecl& func) = 0;
    virtual void Visit(MStructFuncDecl& func) = 0;
};

export class MFuncDecl
{
public:
    virtual ~MFuncDecl() {}
    virtual void Accept(MFuncDeclVisitor& visitor) = 0;
};

export using MFuncDeclPtr = std::shared_ptr<MFuncDecl>;

}
