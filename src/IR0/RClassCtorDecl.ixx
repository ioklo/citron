export module Citron.RDecls:RClassCtorDecl;

import <memory>;

import Citron.MDecls;
import :RDecl;
import :RFuncDecl;
import :RFuncDeclOuter;

namespace Citron {

export class RClassCtorDecl
    : public RDecl
    , public RFuncDecl
    , public RFuncDeclOuter
{
public:
    virtual std::shared_ptr<RClassDecl> GetClassDecl() = 0;

    void Accept(RDeclVisitor& visitor) final { visitor.Visit(*this); }
    void Accept(RFuncDeclVisitor& visitor) final { visitor.Visit(*this); }
    void Accept(RFuncDeclOuterVisitor& visitor) final { visitor.Visit(*this); }
};

export class RMClassCtorDecl : public RClassCtorDecl
{
    std::shared_ptr<MClassCtorDecl> decl;
};


} // namespace Citron
