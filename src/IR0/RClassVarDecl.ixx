export module Citron.RDecls:RClassVarDecl;

import <memory>;

import Citron.MDecls;

import :RDecl;

namespace Citron {

export class RType;
export using RTypePtr = std::shared_ptr<RType>;

export class RTypeFactory;

export class RClassVarDecl
    : public RDecl
{
public:
    virtual RTypePtr GetDeclType(RTypeArguments& typeArgs, RTypeFactory& factory) = 0;
    virtual bool IsStatic() = 0;

    void Accept(RDeclVisitor& visitor) final { visitor.Visit(*this); }
};

export class RMClassVarDecl : public RClassVarDecl
{
    std::shared_ptr<MClassVarDecl> decl;
};


} // namespace Citron
