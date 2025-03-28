export module Citron.MSymbol:MDecl;

import <memory>;

import :ForwardDecls;

namespace Citron
{
export class MDecl
{
public:
    virtual ~MDecl() {}
    virtual void Accept(MDeclVisitor& visitor) = 0;
};

export using MDeclPtr = std::shared_ptr<MDecl>;

export class MDeclVisitor
{
public:
    virtual ~MDeclVisitor() {}
    virtual void Visit(MNamespaceDecl& decl) = 0;
    virtual void Visit(MGlobalFuncDecl& decl) = 0;
    virtual void Visit(MStructDecl& decl) = 0;
    virtual void Visit(MStructCtorDecl& decl) = 0;
    virtual void Visit(MStructFuncDecl& decl) = 0;
    virtual void Visit(MStructVarDecl& decl) = 0;
    virtual void Visit(MClassDecl& decl) = 0;
    virtual void Visit(MClassCtorDecl& decl) = 0;
    virtual void Visit(MClassFuncDecl& decl) = 0;
    virtual void Visit(MClassVarDecl& decl) = 0;
    virtual void Visit(MEnumDecl& decl) = 0;
    virtual void Visit(MEnumElemDecl& decl) = 0;
    virtual void Visit(MEnumElemVarDecl& decl) = 0;
    virtual void Visit(MInterfaceDecl& decl) = 0;
};

}