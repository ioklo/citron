#pragma once

#include <variant>

namespace Citron {

class MGlobalFuncDecl;
class MClassCtorDecl;
class MClassFuncDecl;
class MStructCtorDecl;
class MStructFuncDecl;

class MBodyDeclOuterVisitor
{
public:
    virtual ~MBodyDeclOuterVisitor() = default;
    virtual void Visit(MGlobalFuncDecl& outer) = 0;
    virtual void Visit(MClassCtorDecl& outer) = 0;
    virtual void Visit(MClassFuncDecl& outer) = 0;
    virtual void Visit(MStructCtorDecl& outer) = 0;
    virtual void Visit(MStructFuncDecl& outer) = 0;
};

class MBodyDeclOuter
{
public:
    virtual ~MBodyDeclOuter() = default;
    virtual void Accept(MBodyDeclOuterVisitor& visitor) = 0;
};

}