export module Citron.RDecls:RTypeDeclOuter;

import <memory>;

import Citron.MDecls;

namespace Citron {

export class RDecl;
export class RNamespaceDecl;
export class RClassDecl;
export class RStructDecl;

class RTypeDeclOuterVisitor;

export class RTypeDeclOuter
{
public:
    virtual ~RTypeDeclOuter() {}

    virtual RDecl* GetRDecl() = 0;
    virtual void Accept(RTypeDeclOuterVisitor& visitor) = 0;
};

export class RTypeDeclOuterVisitor
{
public:
    virtual ~RTypeDeclOuterVisitor() {}
    virtual void Visit(RNamespaceDecl& outer) = 0;
    virtual void Visit(RClassDecl& outer) = 0;
    virtual void Visit(RStructDecl& outer) = 0;
};

export class RMTypeDeclOuter : public RTypeDeclOuter
{
    std::shared_ptr<MTypeDeclOuter> outer;
};


} // namespace Citron