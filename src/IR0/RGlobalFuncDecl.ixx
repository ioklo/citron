export module Citron.RDecls:RGlobalFuncDecl;

import <memory>;

import Citron.MDecls;
import :RFuncDecl;
import :RFuncDeclOuter;

namespace Citron {

export class RType;
export using RTypePtr = std::shared_ptr<RType>;

export class RTypeFactory;

// abstract
export class RGlobalFuncDecl
    : public RDecl
    , public RFuncDecl
    , public RFuncDeclOuter
{
public:
    virtual RTypePtr GetReturnType(RTypeArguments& typeArgs, RTypeFactory& factory) = 0;

    void Accept(RDeclVisitor& visitor) final { visitor.Visit(*this); }
    void Accept(RFuncDeclVisitor& visitor) final { visitor.Visit(*this); }
    void Accept(RFuncDeclOuterVisitor& visitor) final { visitor.Visit(*this); }
};

export class RMGlobalFuncDecl : public RGlobalFuncDecl
{
    std::shared_ptr<MGlobalFuncDecl> externalFuncDecl;
};

}