export module Citron.RDecls:RStructVarDecl;

import <memory>;

import Citron.MDecls;
import :RDecl;

namespace Citron {

export class RType;
export using RTypePtr = std::shared_ptr<RType>;

export class RTypeFactory;

export class RStructVarDecl
    : public RDecl
{
public:
    virtual RTypePtr GetDeclType(RTypeArguments& typeArgs, RTypeFactory& factory) = 0;
    virtual bool IsStatic() = 0;
    void Accept(RDeclVisitor& visitor) final { visitor.Visit(*this); }
};

export class RMStructVarDecl : public RStructVarDecl
{
    std::shared_ptr<MStructVarDecl> decl;
};


} // namespace Citron
