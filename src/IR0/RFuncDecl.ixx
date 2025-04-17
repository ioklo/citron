export module Citron.RDecls:RFuncDecl;

import "IR0Config.h";

import <memory>;

import :RDecl;
import :RFuncReturn;
import :RFuncParameter;

namespace Citron {

export class RGlobalFuncDecl;
export class RClassCtorDecl;
export class RClassFuncDecl;
export class RStructCtorDecl;
export class RStructFuncDecl;

export class RType;
export using RTypePtr = std::shared_ptr<RType>;

export class RTypeFactory;

class RFuncDeclVisitor;

export class RFuncDecl
{
public:
    virtual ~RFuncDecl() {}

    virtual RDecl* GetRDecl() = 0;
    virtual bool IsStatic() = 0;
    virtual size_t GetTypeParamCount() = 0;
    virtual size_t GetParamCount() = 0;
    virtual RTypePtr GetReturnType(RTypeArguments& typeArgs, RTypeFactory& factory) = 0;
    virtual RFuncReturn GetFuncReturn(RTypeArguments& typeArgs, RTypeFactory& factory) = 0;
    virtual RFuncParameter GetFuncParam(RTypeArguments& typeArgs, size_t index, RTypeFactory& factory) = 0;
    virtual void Accept(RFuncDeclVisitor& visitor) = 0;
};

export using RFuncDeclPtr = std::shared_ptr<RFuncDecl>;

export class RFuncDeclVisitor
{
public:
    IR0_API virtual ~RFuncDeclVisitor() { }
    virtual void Visit(RGlobalFuncDecl& func) = 0;
    virtual void Visit(RClassCtorDecl& func) = 0;
    virtual void Visit(RClassFuncDecl& func) = 0;
    virtual void Visit(RStructCtorDecl& func) = 0;
    virtual void Visit(RStructFuncDecl& func) = 0;
    virtual void Visit(RLambdaDecl& func) = 0;
};

export class RMFuncDecl : public RFuncDecl
{
    std::shared_ptr<MFuncDecl> funcDecl;
};


} // namespace Citron
