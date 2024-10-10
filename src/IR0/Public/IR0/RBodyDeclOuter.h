#pragma once
#include <variant>
#include <memory>

namespace Citron
{

class RDecl;
class RGlobalFuncDecl;
class RClassConstructorDecl;
class RClassMemberFuncDecl;
class RStructConstructorDecl;
class RStructMemberFuncDecl;
class RLambdaDecl;

class RBodyDeclOuterVisitor
{
public:
    virtual ~RBodyDeclOuterVisitor() { }
    virtual void Visit(RGlobalFuncDecl& outer) = 0;
    virtual void Visit(RClassConstructorDecl& outer) = 0;
    virtual void Visit(RClassMemberFuncDecl& outer) = 0;
    virtual void Visit(RStructConstructorDecl& outer) = 0;
    virtual void Visit(RStructMemberFuncDecl& outer) = 0;
    virtual void Visit(RLambdaDecl& outer) = 0;
};

// 이것은 weak_ptr로 선언하도록 한다
class RBodyDeclOuter
{
public:
    virtual ~RBodyDeclOuter() { }
    virtual RDecl* GetDecl() = 0;
    virtual void Accept(RBodyDeclOuterVisitor& visitor) = 0;
};

using RBodyDeclOuterPtr = std::weak_ptr<RBodyDeclOuter>;

}