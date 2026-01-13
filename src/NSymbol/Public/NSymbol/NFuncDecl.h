#pragma once

#include <span>

#include "RSymbol/RFuncReturn.h"
#include "NDecl.h"

namespace Citron
{
struct RFuncParameter;
class NFuncDeclOuter;
class RFuncDecl;

class NFuncDeclVisitor
{
public:
    virtual ~NFuncDeclVisitor() {}
    virtual void Visit(NGlobalFuncDecl* func) = 0;
    virtual void Visit(NClassCtorDecl* func) = 0;
    virtual void Visit(NClassFuncDecl* func) = 0; 
    virtual void Visit(NStructCtorDecl* func) = 0;
    virtual void Visit(NStructDtorDecl* func) = 0;
    virtual void Visit(NStructFuncDecl* func) = 0;
    virtual void Visit(NLambdaDecl* func) = 0;
};

// RFuncDecl과 겹치는게 있으면 지우자
class NFuncDecl
{
public:
    virtual ~NFuncDecl() {}
    virtual NDecl* GetNDecl() = 0;
    virtual NFuncDeclOuter* GetNFuncDeclOuter() = 0;
    virtual RFuncDecl* GetRFuncDecl() = 0;
    virtual bool IsSeqFunc() = 0;
    virtual void Accept(NFuncDeclVisitor& visitor) = 0;
};

}