#pragma once

#include "RFuncReturn.h"
#include "NDecl.h"

namespace Citron
{

class NFuncDeclVisitor
{
public:
    virtual ~NFuncDeclVisitor() {}
    virtual void Visit(NGlobalFuncDecl* func) = 0;
    virtual void Visit(NClassCtorDecl* func) = 0;
    virtual void Visit(NClassFuncDecl* func) = 0;
    virtual void Visit(NStructCtorDecl* func) = 0;
    virtual void Visit(NStructFuncDecl* func) = 0;
    virtual void Visit(NLambdaDecl* func) = 0;
};

class NFuncDecl
{
public:
    virtual ~NFuncDecl() {}
    virtual NDecl* GetNDecl() = 0;
    virtual RFuncReturn GetUnboundFuncReturn() = 0;
    virtual bool IsSeqFunc() = 0;
    virtual void Accept(NFuncDeclVisitor& visitor) = 0;
};

}