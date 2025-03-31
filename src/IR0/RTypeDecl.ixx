export module Citron.RDecls:RTypeDecl;

import <memory>;

import Citron.MDecls;

import :RDecl;

namespace Citron {

export class RClassDecl;
export class RStructDecl;
export class REnumDecl;
export class REnumElemDecl;
export class RInterfaceDecl;
export class RLambdaDecl;

class RTypeDeclVisitor;

export class RTypeDecl
{
public:
    virtual ~RTypeDecl() {}

    virtual RDecl* GetRDecl() = 0;
    virtual void Accept(RTypeDeclVisitor& visitor) = 0;
};

export class RTypeDeclVisitor
{
public:
    virtual ~RTypeDeclVisitor() {}
    virtual void Visit(RClassDecl& typeDecl) = 0;
    virtual void Visit(RStructDecl& typeDecl) = 0;
    virtual void Visit(REnumDecl& typeDecl) = 0;
    virtual void Visit(REnumElemDecl& typeDecl) = 0;
    virtual void Visit(RInterfaceDecl& typeDecl) = 0;
    virtual void Visit(RLambdaDecl& typeDecl) = 0;
};

export class RMTypeDecl : public RTypeDecl
{
    std::shared_ptr<MTypeDecl> typeDecl;
};


} // namespace Citron