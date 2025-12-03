#pragma once

#include <variant>

namespace Citron {

class EGlobalFuncDecl;
class EClassCtorDecl;
class EClassFuncDecl;
class EStructCtorDecl;
class EStructFuncDecl;

class EBodyDeclOuterVisitor
{
public:
    virtual ~EBodyDeclOuterVisitor() = default;
    virtual void Visit(EGlobalFuncDecl& outer) = 0;
    virtual void Visit(EClassCtorDecl& outer) = 0;
    virtual void Visit(EClassFuncDecl& outer) = 0;
    virtual void Visit(EStructCtorDecl& outer) = 0;
    virtual void Visit(EStructFuncDecl& outer) = 0;
};

class EBodyDeclOuter
{
public:
    virtual ~EBodyDeclOuter() = default;
    virtual void Accept(EBodyDeclOuterVisitor& visitor) = 0;
};

}