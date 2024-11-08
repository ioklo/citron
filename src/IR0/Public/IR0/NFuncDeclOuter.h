#pragma once
#include <variant>
#include <memory>

#include "RFuncDeclOuter.h"

namespace Citron
{

class NDecl;
class NModule;
class NNamespaceDecl;
class NGlobalFuncDecl;
class NClassDecl;
class NClassConstructorDecl;
class NClassMemberFuncDecl;
class NStructDecl;
class NStructConstructorDecl;
class NStructMemberFuncDecl;
class NLambdaDecl;

class NFuncDeclOuterVisitor
{
public:
    virtual ~NFuncDeclOuterVisitor() { }
    virtual void Visit(NNamespaceDecl& outer) = 0;
    virtual void Visit(NGlobalFuncDecl& outer) = 0;
    virtual void Visit(NClassDecl& outer) = 0;
    virtual void Visit(NClassConstructorDecl& outer) = 0;
    virtual void Visit(NClassMemberFuncDecl& outer) = 0;
    virtual void Visit(NStructDecl& outer) = 0;
    virtual void Visit(NStructConstructorDecl& outer) = 0;
    virtual void Visit(NStructMemberFuncDecl& outer) = 0;
    virtual void Visit(NLambdaDecl& outer) = 0;
};

// 이것은 weak_ptr로 선언하도록 한다
class NFuncDeclOuter
{
public:
    virtual ~NFuncDeclOuter() { }
    virtual NDecl* GetNDecl() = 0;
    virtual void Accept(NFuncDeclOuterVisitor& visitor) = 0;
};

using NFuncDeclOuterWPtr = std::weak_ptr<NFuncDeclOuter>;

}