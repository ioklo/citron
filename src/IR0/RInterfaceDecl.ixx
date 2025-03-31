export module Citron.RDecls:RInterfaceDecl;

import <memory>;

import Citron.MDecls;
import :RDecl;
import :RTypeDecl;

namespace Citron {

export class RInterfaceDecl
    : public RDecl
    , public RTypeDecl
{
public:
    void Accept(RDeclVisitor& visitor) final { visitor.Visit(*this); }
    void Accept(RTypeDeclVisitor& visitor) final { visitor.Visit(*this); }
};

export class RMInterfaceDecl : public RInterfaceDecl
{
    std::shared_ptr<MInterfaceDecl> decl;
};


} // namespace Citron

