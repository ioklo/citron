#pragma once

#include <memory>

namespace Citron {

class MTypeDeclOuter;
class RTypeDeclOuterVisitor;
class RModuleDecl;
class RNamespaceDecl;
class RClassDecl;
class RStructDecl;

class RTypeDeclOuter
{
public:
    virtual ~RTypeDeclOuter() { }

    virtual RDecl* GetRDecl() = 0;
    virtual void Accept(RTypeDeclOuterVisitor& visitor) = 0;
};

class RTypeDeclOuterVisitor
{
public:
    virtual ~RTypeDeclOuterVisitor() { }
    virtual void Visit(RModuleDecl& outer) = 0;
    virtual void Visit(RNamespaceDecl& outer) = 0;
    virtual void Visit(RClassDecl& outer) = 0;
    virtual void Visit(RStructDecl& outer) = 0;
};

class RMTypeDeclOuter : public RTypeDeclOuter
{
    std::shared_ptr<MTypeDeclOuter> outer;
};


} // namespace Citron