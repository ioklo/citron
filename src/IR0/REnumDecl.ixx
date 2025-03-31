export module Citron.RDecls:REnumDecl;

import <memory>;

import Citron.MDecls;
import :RDecl;
import :RTypeDecl;

namespace Citron {

export class REnumDecl
    : public RDecl
    , public RTypeDecl
{
public:
    void Accept(RDeclVisitor& visitor) final { visitor.Visit(*this); }
    void Accept(RTypeDeclVisitor& visitor) final { visitor.Visit(*this); }
};

export class RMEnumDecl : public REnumDecl
{
    std::shared_ptr<MEnumDecl> decl;
};


} // namespace Citron
