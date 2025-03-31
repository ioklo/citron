export module Citron.RDecls:RNamespaceDecl;

import <memory>;

import Citron.MDecls;
import :RDecl;
import :RFuncDeclOuter;
import :RTypeDeclOuter;

namespace Citron {

export class RNamespaceDecl
    : public RDecl
    , public RTypeDeclOuter
    , public RFuncDeclOuter
{
public:
    virtual void Accept(RDeclVisitor& visitor) final { visitor.Visit(*this); }
    virtual void Accept(RFuncDeclOuterVisitor& visitor) final { visitor.Visit(*this); }
    virtual void Accept(RTypeDeclOuterVisitor& visitor) final { visitor.Visit(*this); }
};

export class RMNamespaceDecl : public RNamespaceDecl
{
    std::shared_ptr<MNamespaceDecl> decl;
    // std::optional<RMember> GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount) override;
};

} // namespace Citron
