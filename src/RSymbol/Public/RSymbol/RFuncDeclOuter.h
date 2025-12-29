#pragma once


namespace Citron {

class EFuncDeclOuter;

class RDecl;
class RNamespaceDecl;
class RGlobalFuncDecl;
class RClassDecl;
class RClassCtorDecl;
class RClassFuncDecl;
class RStructDecl;
class RStructCtorDecl;
class RStructDtorDecl;
class RStructFuncDecl;
class RLambdaDecl;

class RFuncDeclOuterVisitor;

class RFuncDeclOuter
{
public:
    virtual ~RFuncDeclOuter() {}

    virtual RDecl* GetRDecl() = 0;
    virtual void Accept(RFuncDeclOuterVisitor& visitor) = 0;
};

class RFuncDeclOuterVisitor
{
public:
    virtual ~RFuncDeclOuterVisitor() {}
    virtual void Visit(RNamespaceDecl* outer) = 0;
    virtual void Visit(RGlobalFuncDecl* outer) = 0;
    virtual void Visit(RClassDecl* outer) = 0;
    virtual void Visit(RClassCtorDecl* outer) = 0;
    virtual void Visit(RClassFuncDecl* outer) = 0;
    virtual void Visit(RStructDecl* outer) = 0;
    virtual void Visit(RStructCtorDecl* outer) = 0;
    virtual void Visit(RStructDtorDecl* outer) = 0;
    virtual void Visit(RStructFuncDecl* outer) = 0;
    virtual void Visit(RLambdaDecl* outer) = 0;
};

class REFuncDeclOuter : public RFuncDeclOuter
{
    EFuncDeclOuter* outer;
};


} // namespace Citron
