#pragma once

#include <memory>
#include "NDecl.h"
#include "RTypeDecl.h"
#include "RMember.h"

namespace Citron {

class NClassDecl;
class NStructDecl;
class NEnumDecl;
class NEnumElemDecl;
class NInterfaceDecl;
class NLambdaDecl;

class NTypeDeclVisitor
{
public:
    virtual ~NTypeDeclVisitor() { }
    virtual void Visit(NClassDecl& typeDecl) = 0;
    virtual void Visit(NStructDecl& typeDecl) = 0;
    virtual void Visit(NEnumDecl& typeDecl) = 0;
    virtual void Visit(NEnumElemDecl& typeDecl) = 0;
    virtual void Visit(NInterfaceDecl& typeDecl) = 0;
    virtual void Visit(NLambdaDecl& typeDecl) = 0;
};

class NTypeDecl
{
public:
    virtual ~NTypeDecl() { }
    virtual NDecl* GetDecl() = 0;
    virtual RMember ToRMember(const std::shared_ptr<NTypeDecl>& sharedThis, const RTypeArgumentsPtr& typeArgs) = 0;
    virtual void Accept(NTypeDeclVisitor& visitor) = 0;
};

using NTypeDeclPtr = std::shared_ptr<NTypeDecl>;

}