#pragma once

#include <memory>
#include "RDecl.h"

namespace Citron {

class MTypeDecl;
class RTypeDeclVisitor;
class RClassDecl;
class RStructDecl;
class REnumDecl;
class REnumElemDecl;
class RInterfaceDecl;
class RLambdaDecl;

class RTypeDecl
{
public:
    virtual ~RTypeDecl() { }
    virtual void Accept(RTypeDeclVisitor& visitor) = 0;
};

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

class RMTypeDecl : public RTypeDecl
{
    std::shared_ptr<MTypeDecl> typeDecl;
};


} // namespace Citron