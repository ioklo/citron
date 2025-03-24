#pragma once

#include <memory>
#include "RDecl.h"
#include "RFuncReturn.h"
#include "RFuncParameter.h"

namespace Citron {

class MFuncDecl;
class RFuncDeclVisitor;
class RGlobalFuncDecl;
class RClassCtorDecl;
class RClassFuncDecl;
class RStructCtorDecl;
class RStructFuncDecl;
class RLambdaDecl;

class RTypeArguments;
class RTypeFactory;

class RFuncDecl
{
public:
    virtual ~RFuncDecl() { }

    virtual RDecl* GetRDecl() = 0;

    virtual bool IsStatic() = 0;
    virtual size_t GetTypeParamCount() = 0;
    virtual size_t GetParamCount() = 0;
    virtual RTypePtr GetReturnType(RTypeArguments& typeArgs, RTypeFactory& factory) = 0;
    virtual RFuncParameter GetFuncParameter(RTypeArguments& typeArgs, size_t index, RTypeFactory& factory) = 0;

    virtual void Accept(RFuncDeclVisitor& visitor) = 0;
};

class RFuncDeclVisitor
{
public:
    virtual ~RFuncDeclVisitor() { }
    virtual void Visit(RGlobalFuncDecl& func) = 0;
    virtual void Visit(RClassCtorDecl& func) = 0;
    virtual void Visit(RClassFuncDecl& func) = 0;
    virtual void Visit(RStructCtorDecl& func) = 0;
    virtual void Visit(RStructFuncDecl& func) = 0;
    virtual void Visit(RLambdaDecl& func) = 0;
};


class RMFuncDecl : public RFuncDecl
{
    std::shared_ptr<MFuncDecl> funcDecl;
};


} // namespace Citron
