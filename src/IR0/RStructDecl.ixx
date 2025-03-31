export module Citron.RDecls:RStructDecl;

import <memory>;

import Citron.MDecls;
import :RDecl;
import :RFuncDeclOuter;
import :RTypeDecl;
import :RTypeDeclOuter;

namespace Citron {

export class RTypeArguments;
export using RTypeArgumentsPtr = std::shared_ptr<RTypeArguments>;

export class RStructDecl
    : public RDecl
    , public RFuncDeclOuter
    , public RTypeDecl
    , public RTypeDeclOuter
{
public:
    virtual std::optional<RMember_StructVar> GetVar(const RTypeArgumentsPtr& typeArgs, const RName& name) = 0;
    virtual std::vector<std::shared_ptr<RStructCtorDecl>> GetUnboundCtors() = 0;
    virtual std::shared_ptr<RStructCtorDecl> GetUnboundTrivialCtor_RStructCtorDecl() = 0;

    void Accept(RDeclVisitor& visitor) final { visitor.Visit(*this); }
    void Accept(RFuncDeclOuterVisitor& visitor) final { visitor.Visit(*this); }
    void Accept(RTypeDeclVisitor& visitor) final { visitor.Visit(*this); }
    void Accept(RTypeDeclOuterVisitor& visitor) final { visitor.Visit(*this); }
};

export class RMStructDecl : public RStructDecl
{
    std::shared_ptr<MStructDecl> decl;
};


} // namespace Citron

