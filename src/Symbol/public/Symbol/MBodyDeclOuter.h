#pragma once
#include <variant>
#include <memory>

namespace Citron
{

class MGlobalFuncDecl;
class MClassConstructorDecl;
class MClassMemberFuncDecl;
class MStructConstructorDecl;
class MStructMemberFuncDecl;

class MBodyDeclOuterVisitor
{
public:
    virtual ~MBodyDeclOuterVisitor() { }
    virtual void Visit(MGlobalFuncDecl& outer) = 0;
    virtual void Visit(MClassConstructorDecl& outer) = 0;
    virtual void Visit(MClassMemberFuncDecl& outer) = 0;
    virtual void Visit(MStructConstructorDecl& outer) = 0;
    virtual void Visit(MStructMemberFuncDecl& outer) = 0;
};

// 이것은 weak_ptr로 선언하도록 한다
class MBodyDeclOuter
{
public:
    virtual ~MBodyDeclOuter() { }
    virtual void Accept(MBodyDeclOuterVisitor& visitor) = 0;
};

using MBodyDeclOuterPtr = std::weak_ptr<MBodyDeclOuter>;

}