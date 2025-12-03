#pragma once



namespace Citron {

class ETypeDeclOuter;

class RDecl;
class RNamespaceDecl;
class RClassDecl;
class RStructDecl;

class RTypeDeclOuterVisitor;

class RTypeDeclOuter
{
public:
    virtual ~RTypeDeclOuter() {}

    virtual RDecl* GetRDecl() = 0;
    virtual void Accept(RTypeDeclOuterVisitor& visitor) = 0;
};

class RTypeDeclOuterVisitor
{
public:
    virtual ~RTypeDeclOuterVisitor() {}
    virtual void Visit(RNamespaceDecl* outer) = 0;
    virtual void Visit(RClassDecl* outer) = 0;
    virtual void Visit(RStructDecl* outer) = 0;
};

class RETypeDeclOuter : public RTypeDeclOuter
{
    ETypeDeclOuter* outer;
};


} // namespace Citron