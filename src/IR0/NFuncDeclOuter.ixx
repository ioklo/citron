export module Citron.NDecls:NFuncDeclOuter;

import <variant>;
import <memory>;

import Citron.RDecls;

namespace Citron
{

export class NDecl;
export class NNamespaceDecl;
export class NGlobalFuncDecl;
export class NClassDecl;
export class NClassCtorDecl;
export class NClassFuncDecl;
export class NStructDecl;
export class NStructCtorDecl;
export class NStructFuncDecl;
export class NLambdaDecl;

export class NFuncDeclOuterVisitor
{
public:
    virtual ~NFuncDeclOuterVisitor() {}
    virtual void Visit(NNamespaceDecl& outer) = 0;
    virtual void Visit(NGlobalFuncDecl& outer) = 0;
    virtual void Visit(NClassDecl& outer) = 0;
    virtual void Visit(NClassCtorDecl& outer) = 0;
    virtual void Visit(NClassFuncDecl& outer) = 0;
    virtual void Visit(NStructDecl& outer) = 0;
    virtual void Visit(NStructCtorDecl& outer) = 0;
    virtual void Visit(NStructFuncDecl& outer) = 0;
    virtual void Visit(NLambdaDecl& outer) = 0;
};

// 이것은 weak_ptr로 선언하도록 한다
export class NFuncDeclOuter
{
public:
    virtual ~NFuncDeclOuter() {}
    virtual NDecl* GetNDecl() = 0;
    virtual void Accept(NFuncDeclOuterVisitor& visitor) = 0;
};

export using NFuncDeclOuterWPtr = std::weak_ptr<NFuncDeclOuter>;

}