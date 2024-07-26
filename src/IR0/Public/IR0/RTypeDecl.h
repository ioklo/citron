#pragma once

#include <memory>

namespace Citron {

class RClassDecl;
class RStructDecl;
class REnumDecl;
class REnumElemDecl;
class RInterfaceDecl;
class RLambdaDecl;

class RTypeDeclVisitor
{
public:
    virtual ~RTypeDeclVisitor() { }
    virtual void Visit(RClassDecl& typeDecl) = 0;
    virtual void Visit(RStructDecl& typeDecl) = 0;
    virtual void Visit(REnumDecl& typeDecl) = 0;
    virtual void Visit(REnumElemDecl& typeDecl) = 0;
    virtual void Visit(RInterfaceDecl& typeDecl) = 0;
    virtual void Visit(RLambdaDecl& typeDecl) = 0;
};

class RTypeDecl
{
public:
    virtual ~RTypeDecl() { }
    virtual void Accept(RTypeDeclVisitor& visitor) = 0;
};

using RTypeDeclPtr = std::shared_ptr<RTypeDecl>;

}