export module Citron.RDecls:REnumElemVarDecl;

import <memory>;

import Citron.MDecls;
import :RDecl;

namespace Citron {

export class RType;
export using RTypePtr = std::shared_ptr<RType>;

export class RTypeFactory;

export class REnumElemVarDecl
    : public RDecl
{
public:
    virtual RTypePtr GetDeclType(RTypeArguments& typeArgs, RTypeFactory& factory) = 0;
    void Accept(RDeclVisitor& visitor) final { visitor.Visit(*this); }
};

export class RMEnumElemVarDecl : public REnumElemVarDecl
{
    std::shared_ptr<MEnumElemVarDecl> decl;
};


} // namespace Citron
