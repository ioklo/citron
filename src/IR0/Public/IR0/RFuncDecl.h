#pragma once

#include "IR0Config.h"


#include "Symbol/MFuncDecl.h"

#include "RDecl.h"
#include "RFuncReturn.h"
#include "RFuncParameter.h"

namespace Citron {

class RGlobalFuncDecl;
class RClassCtorDecl;
class RClassFuncDecl;
class RStructCtorDecl;
class RStructFuncDecl;

class RType;
class RFactory;

class RFuncDeclVisitor;

class RFuncDecl
{
public:
    virtual ~RFuncDecl() {}

    virtual RDecl* GetRDecl() = 0;
    virtual bool IsStatic() = 0;
    virtual size_t GetTypeParamCount() = 0;
    virtual size_t GetParamCount() = 0;
    virtual RType* GetReturnType(RTypeArguments& typeArgs, RFactory& factory) = 0;
    virtual RFuncReturn GetFuncReturn(RTypeArguments& typeArgs, RFactory& factory) = 0;
    virtual RFuncParameter GetFuncParam(RTypeArguments& typeArgs, size_t index, RFactory& factory) = 0;
    virtual void Accept(RFuncDeclVisitor& visitor) = 0;
};

class RFuncDeclVisitor
{
public:
    IR0_API virtual ~RFuncDeclVisitor() { }
    virtual void Visit(RGlobalFuncDecl* func) = 0;
    virtual void Visit(RClassCtorDecl* func) = 0;
    virtual void Visit(RClassFuncDecl* func) = 0;
    virtual void Visit(RStructCtorDecl* func) = 0;
    virtual void Visit(RStructFuncDecl* func) = 0;
    virtual void Visit(RLambdaDecl* func) = 0;
};

class RMFuncDecl : public RFuncDecl
{
    MFuncDecl* funcDecl;
};


} // namespace Citron
