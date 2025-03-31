export module Citron.RDecls:RClassDecl;

import <memory>;

import Citron.MDecls;

import :RDecl;
import :RFuncDeclOuter;
import :RTypeDecl;
import :RTypeDeclOuter;
import :RNames;

namespace Citron {

export class RTypeArguments;
export using RTypeArgumentsPtr = std::shared_ptr<RTypeArguments>;

export class RClassDecl
    : public RDecl
    , public RFuncDeclOuter
    , public RTypeDecl
    , public RTypeDeclOuter
{
public:
    virtual std::optional<RMember_ClassVar> GetVar(const RTypeArgumentsPtr& typeArgs, const RName& name) = 0;

    void Accept(RDeclVisitor& visitor) final { visitor.Visit(*this); }
    void Accept(RFuncDeclOuterVisitor& visitor) final { visitor.Visit(*this); }
    void Accept(RTypeDeclVisitor& visitor) final { visitor.Visit(*this); }
    void Accept(RTypeDeclOuterVisitor& visitor) final { visitor.Visit(*this); }
};

export class RMClassDecl : public RClassDecl
{
    std::shared_ptr<MClassDecl> decl;
};


} // namespace Citron