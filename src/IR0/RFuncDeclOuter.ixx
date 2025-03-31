export module Citron.RDecls:RFuncDeclOuter;

import <memory>;

import Citron.MDecls;

namespace Citron {

export class RDecl;
export class RNamespaceDecl;
export class RGlobalFuncDecl;
export class RClassDecl;
export class RClassCtorDecl;
export class RClassFuncDecl;
export class RStructDecl;
export class RStructCtorDecl;
export class RStructFuncDecl;
export class RLambdaDecl;

class RFuncDeclOuterVisitor;

export class RFuncDeclOuter
{
public:
    virtual ~RFuncDeclOuter() {}

    virtual RDecl* GetRDecl() = 0;
    virtual void Accept(RFuncDeclOuterVisitor& visitor) = 0;
};

export class RFuncDeclOuterVisitor
{
public:
    virtual ~RFuncDeclOuterVisitor() {}
    virtual void Visit(RNamespaceDecl& outer) = 0;
    virtual void Visit(RGlobalFuncDecl& outer) = 0;
    virtual void Visit(RClassDecl& outer) = 0;
    virtual void Visit(RClassCtorDecl& outer) = 0;
    virtual void Visit(RClassFuncDecl& outer) = 0;
    virtual void Visit(RStructDecl& outer) = 0;
    virtual void Visit(RStructCtorDecl& outer) = 0;
    virtual void Visit(RStructFuncDecl& outer) = 0;
    virtual void Visit(RLambdaDecl& outer) = 0;
};

export class RMFuncDeclOuter : public RFuncDeclOuter
{
    std::shared_ptr<MFuncDeclOuter> outer;
};


} // namespace Citron
