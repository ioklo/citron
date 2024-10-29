#pragma once

#include <memory>

#include "NDecl.h"
#include "RFuncReturn.h"

namespace Citron
{

class NGlobalFuncDecl;        // top-level decl space
class NClassConstructorDecl;  // construct decl space
class NClassMemberFuncDecl;   // construct decl space
class NStructConstructorDecl; // struct decl space
class NStructMemberFuncDecl;  // struct decl space
class NLambdaDecl;            // body space

class NFuncDeclVisitor
{
public:
    virtual ~NFuncDeclVisitor() { }
    virtual void Visit(NGlobalFuncDecl& func) = 0;
    virtual void Visit(NClassConstructorDecl& func) = 0;
    virtual void Visit(NClassMemberFuncDecl& func) = 0;
    virtual void Visit(NStructConstructorDecl& func) = 0;
    virtual void Visit(NStructMemberFuncDecl& func) = 0;
    virtual void Visit(NLambdaDecl& func) = 0;
};

class NFuncDecl : public virtual NDecl
{
public:
    virtual ~NFuncDecl() { }
    virtual bool IsStatic() = 0;
    virtual int GetTypeParamCount() = 0;
    virtual int GetParamCount() = 0;
    virtual RFuncReturn GetReturn() = 0;
    virtual void Accept(NFuncDeclVisitor& visitor) = 0;
};

using NFuncDeclPtr = std::shared_ptr<NFuncDecl>;

}