#pragma once


#include "RMember.h"
#include "NDecl.h"

namespace Citron {

class NTypeDeclVisitor
{
public:
    virtual ~NTypeDeclVisitor() {}
    virtual void Visit(NClassDecl* typeDecl) = 0;
    virtual void Visit(NStructDecl* typeDecl) = 0;
    virtual void Visit(NEnumDecl* typeDecl) = 0;
    virtual void Visit(NEnumElemDecl* typeDecl) = 0;
    virtual void Visit(NInterfaceDecl* typeDecl) = 0;
    virtual void Visit(NLambdaDecl* typeDecl) = 0;
};

class NTypeDecl
{
public:
    virtual ~NTypeDecl() {}
    virtual NDecl* GetNDecl() = 0;
    virtual RMember ToRMember(RTypeArguments* typeArgs) = 0;
    virtual void Accept(NTypeDeclVisitor& visitor) = 0;
};

}