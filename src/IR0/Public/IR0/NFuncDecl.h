#pragma once

#include <memory>

#include "NDecl.h"
#include "RFuncReturn.h"
#include "RFuncDecl.h"

namespace Citron
{

class NGlobalFuncDecl;        // top-level decl space
class NClassCtorDecl;  // construct decl space
class NClassFuncDecl;   // construct decl space
class NStructCtorDecl; // struct decl space
class NStructFuncDecl;  // struct decl space
class NLambdaDecl;            // body space

class NFuncDeclVisitor
{
public:
    virtual ~NFuncDeclVisitor() { }
    virtual void Visit(NGlobalFuncDecl& func) = 0;
    virtual void Visit(NClassCtorDecl& func) = 0;
    virtual void Visit(NClassFuncDecl& func) = 0;
    virtual void Visit(NStructCtorDecl& func) = 0;
    virtual void Visit(NStructFuncDecl& func) = 0;
    virtual void Visit(NLambdaDecl& func) = 0;
};

class NFuncDecl
{
public:
    virtual ~NFuncDecl() { }
    virtual NDecl* GetNDecl() = 0;
    virtual RFuncReturn GetUnboundFuncReturn() = 0;
    virtual bool IsSeqFunc() = 0;
    virtual void Accept(NFuncDeclVisitor& visitor) = 0;
};

using NFuncDeclPtr = std::shared_ptr<NFuncDecl>;

}