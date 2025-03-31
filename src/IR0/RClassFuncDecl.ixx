export module Citron.RDecls:RClassFuncDecl;

import <memory>;

import Citron.MDecls;

import :RDecl;
import :RFuncDecl;
import :RFuncDeclOuter;

namespace Citron {

export class RType;
export using RTypePtr = std::shared_ptr<RType>;

export class RTypeFactory;

export class RClassFuncDecl
    : public RDecl
    , public RFuncDecl
    , public RFuncDeclOuter
{
public:
    virtual RTypePtr GetReturnType(RTypeArguments& typeArgs, RTypeFactory& factory) = 0;
    virtual bool IsStatic() = 0;

    void Accept(RDeclVisitor& visitor) final { visitor.Visit(*this); }
    void Accept(RFuncDeclVisitor& visitor) final { visitor.Visit(*this); }
    void Accept(RFuncDeclOuterVisitor& visitor) final { visitor.Visit(*this); }
};

export class RMClassFuncDecl : public RClassFuncDecl
{
    std::shared_ptr<MClassFuncDecl> decl;
};


} // namespace Citron
