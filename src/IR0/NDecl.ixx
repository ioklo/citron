export module Citron.NDecls:NDecl;

import <memory>;
import <optional>;

import Citron.RDecls;

namespace Citron
{

export class NNamespaceDecl;
export class NGlobalFuncDecl;
export class NStructDecl;
export class NStructCtorDecl;
export class NStructFuncDecl;
export class NStructVarDecl;
export class NClassDecl;
export class NClassCtorDecl;
export class NClassFuncDecl;
export class NClassVarDecl;
export class NEnumDecl;
export class NEnumElemDecl;
export class NEnumElemVarDecl;
export class NLambdaDecl;
export class NLambdaVarDecl;
export class NInterfaceDecl;

export class NDeclVisitor
{
public:
    virtual ~NDeclVisitor() {}
    virtual void Visit(NNamespaceDecl& decl) = 0;
    virtual void Visit(NGlobalFuncDecl& decl) = 0;
    virtual void Visit(NStructDecl& decl) = 0;
    virtual void Visit(NStructCtorDecl& decl) = 0;
    virtual void Visit(NStructFuncDecl& decl) = 0;
    virtual void Visit(NStructVarDecl& decl) = 0;
    virtual void Visit(NClassDecl& decl) = 0;
    virtual void Visit(NClassCtorDecl& decl) = 0;
    virtual void Visit(NClassFuncDecl& decl) = 0;
    virtual void Visit(NClassVarDecl& decl) = 0;
    virtual void Visit(NEnumDecl& decl) = 0;
    virtual void Visit(NEnumElemDecl& decl) = 0;
    virtual void Visit(NEnumElemVarDecl& decl) = 0;
    virtual void Visit(NLambdaDecl& decl) = 0;
    virtual void Visit(NLambdaVarDecl& decl) = 0;
    virtual void Visit(NInterfaceDecl& decl) = 0;
};

export class NDecl
{
public:
    virtual ~NDecl() {}

    virtual RDecl* GetRDecl() = 0;
    virtual NDecl* GetNOuter() = 0;
    virtual void Accept(NDeclVisitor& visitor) = 0;
};

export using NDeclPtr = std::shared_ptr<NDecl>;

}