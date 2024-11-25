#pragma once
#include <variant>
#include <memory>

namespace Citron
{

class MGlobalFuncDecl;
class MClassCtorDecl;
class MClassMemberFuncDecl;
class MStructCtorDecl;
class MStructMemberFuncDecl;

class MBodyDeclOuterVisitor
{
public:
    virtual ~MBodyDeclOuterVisitor() { }
    virtual void Visit(MGlobalFuncDecl& outer) = 0;
    virtual void Visit(MClassCtorDecl& outer) = 0;
    virtual void Visit(MClassMemberFuncDecl& outer) = 0;
    virtual void Visit(MStructCtorDecl& outer) = 0;
    virtual void Visit(MStructMemberFuncDecl& outer) = 0;
};

// 이것은 weak_ptr로 선언하도록 한다
class MBodyDeclOuter
{
public:
    virtual ~MBodyDeclOuter() { }
    virtual void Accept(MBodyDeclOuterVisitor& visitor) = 0;
};

using MBodyDeclOuterWPtr = std::weak_ptr<MBodyDeclOuter>;

}