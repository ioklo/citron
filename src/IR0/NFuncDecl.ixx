export module Citron.NDecls:NFuncDecl;

import <memory>;

import Citron.RDecls;
import :NDecl;

namespace Citron
{

export class NFuncDeclVisitor
{
public:
    virtual ~NFuncDeclVisitor() {}
    virtual void Visit(NGlobalFuncDecl& func) = 0;
    virtual void Visit(NClassCtorDecl& func) = 0;
    virtual void Visit(NClassFuncDecl& func) = 0;
    virtual void Visit(NStructCtorDecl& func) = 0;
    virtual void Visit(NStructFuncDecl& func) = 0;
    virtual void Visit(NLambdaDecl& func) = 0;
};

export class NFuncDecl
{
public:
    virtual ~NFuncDecl() {}
    virtual NDecl* GetNDecl() = 0;
    virtual RFuncReturn GetUnboundFuncReturn() = 0;
    virtual bool IsSeqFunc() = 0;
    virtual void Accept(NFuncDeclVisitor& visitor) = 0;
};

export using NFuncDeclPtr = std::shared_ptr<NFuncDecl>;

}