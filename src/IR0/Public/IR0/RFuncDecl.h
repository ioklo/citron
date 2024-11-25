#pragma once

#include <memory>
#include "RDecl.h"
#include "RFuncReturn.h"

namespace Citron {

class MFuncDecl;
class RFuncDeclVisitor;
class RGlobalFuncDecl;
class RClassCtorDecl;
class RClassMemberFuncDecl;
class RStructCtorDecl;
class RStructMemberFuncDecl;
class RLambdaDecl;

class RFuncDecl
{
public:
    virtual ~RFuncDecl() { }

    virtual RDecl* GetRDecl() = 0;

    virtual bool IsStatic() = 0;
    virtual size_t GetTypeParamCount() = 0;
    virtual size_t GetParamCount() = 0;
    virtual RFuncReturn GetReturn() = 0;

    virtual void Accept(RFuncDeclVisitor& visitor) = 0;
};

class RFuncDeclVisitor
{
public:
    virtual ~RFuncDeclVisitor() { }
    virtual void Visit(RGlobalFuncDecl& func) = 0;
    virtual void Visit(RClassCtorDecl& func) = 0;
    virtual void Visit(RClassMemberFuncDecl& func) = 0;
    virtual void Visit(RStructCtorDecl& func) = 0;
    virtual void Visit(RStructMemberFuncDecl& func) = 0;
    virtual void Visit(RLambdaDecl& func) = 0;
};


class RMFuncDecl : public RFuncDecl
{
    std::shared_ptr<MFuncDecl> funcDecl;
};


} // namespace Citron
