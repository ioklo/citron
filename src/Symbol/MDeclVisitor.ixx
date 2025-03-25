export module Citron.MSymbol:MDeclVisitor;

namespace Citron
{

export class MNamespaceDecl;
export class MGlobalFuncDecl;
export class MStructDecl;
export class MStructCtorDecl;
export class MStructFuncDecl;
export class MStructVarDecl;
export class MClassDecl;
export class MClassCtorDecl;
export class MClassFuncDecl;
export class MClassVarDecl;
export class MEnumDecl;
export class MEnumElemDecl;
export class MEnumElemVarDecl;
export class MInterfaceDecl;

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
