#pragma once

#include <memory>
#include "RDecl.h"
#include "RFuncReturn.h"

namespace Citron {

class MFuncDecl;
class RFuncDeclVisitor;
class RGlobalFuncDecl;
class RClassConstructorDecl;
class RClassMemberFuncDecl;
class RStructConstructorDecl;
class RStructMemberFuncDecl;
class RLambdaDecl;

class RFuncDecl
{
public:
    virtual ~RFuncDecl() { }

    virtual bool IsStatic() = 0;
    virtual int GetTypeParamCount() = 0;
    virtual int GetParamCount() = 0;
    virtual RFuncReturn GetReturn() = 0;

    virtual void Accept(RFuncDeclVisitor& visitor) = 0;
};

class RFuncDeclVisitor
{
public:
    virtual ~RFuncDeclVisitor() { }
    virtual void Visit(RGlobalFuncDecl& func) = 0;
    virtual void Visit(RClassConstructorDecl& func) = 0;
    virtual void Visit(RClassMemberFuncDecl& func) = 0;
    virtual void Visit(RStructConstructorDecl& func) = 0;
    virtual void Visit(RStructMemberFuncDecl& func) = 0;
    virtual void Visit(RLambdaDecl& func) = 0;
};


class RMFuncDecl : public RFuncDecl
{
    std::shared_ptr<MFuncDecl> funcDecl;
};


} // namespace Citron
