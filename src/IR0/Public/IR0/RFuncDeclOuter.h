#pragma once

#include <memory>

namespace Citron {

class MFuncDeclOuter;
class RFuncDeclOuterVisitor;
class RModule;
class RNamespaceDecl;
class RGlobalFuncDecl;
class RClassDecl;
class RClassCtorDecl;
class RClassMemberFuncDecl;
class RStructDecl;
class RStructCtorDecl;
class RStructMemberFuncDecl;
class RLambdaDecl;

class RFuncDeclOuter
{
public:
    virtual ~RFuncDeclOuter() { }

    virtual RDecl* GetRDecl() = 0;
    virtual void Accept(RFuncDeclOuterVisitor& visitor) = 0;
};

class RFuncDeclOuterVisitor
{
public:
    virtual ~RFuncDeclOuterVisitor() { }
    virtual void Visit(RNamespaceDecl& outer) = 0;
    virtual void Visit(RGlobalFuncDecl& outer) = 0;
    virtual void Visit(RClassDecl& outer) = 0;
    virtual void Visit(RClassCtorDecl& outer) = 0;
    virtual void Visit(RClassMemberFuncDecl& outer) = 0;
    virtual void Visit(RStructDecl& outer) = 0;
    virtual void Visit(RStructCtorDecl& outer) = 0;
    virtual void Visit(RStructMemberFuncDecl& outer) = 0;
    virtual void Visit(RLambdaDecl& outer) = 0;
};

class RMFuncDeclOuter : public RFuncDeclOuter
{
    std::shared_ptr<MFuncDeclOuter> outer;
};


} // namespace Citron
