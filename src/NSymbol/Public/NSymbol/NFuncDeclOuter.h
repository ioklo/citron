#pragma once

#include <variant>

namespace Citron {

class NDecl;
class NNamespaceDecl;
class NGlobalFuncDecl;
class NClassDecl;
class NClassCtorDecl;
class NClassFuncDecl;
class NStructDecl;
class NStructCtorDecl;
class NStructDtorDecl;
class NStructFuncDecl;
class NLambdaDecl;

class NFuncDeclOuterVisitor
{
public:
    virtual ~NFuncDeclOuterVisitor() {}
    virtual void Visit(NNamespaceDecl* outer) = 0;
    virtual void Visit(NGlobalFuncDecl* outer) = 0;
    virtual void Visit(NClassDecl* outer) = 0;
    virtual void Visit(NClassCtorDecl* outer) = 0;
    virtual void Visit(NClassFuncDecl* outer) = 0;
    virtual void Visit(NStructDecl* outer) = 0;
    virtual void Visit(NStructCtorDecl* outer) = 0;
    virtual void Visit(NStructDtorDecl* outer) = 0;
    virtual void Visit(NStructFuncDecl* outer) = 0;
    virtual void Visit(NLambdaDecl* outer) = 0;
};

class NFuncDeclOuter
{
public:
    virtual ~NFuncDeclOuter() {}
    virtual NDecl* GetNDecl() = 0;
    virtual void Accept(NFuncDeclOuterVisitor& visitor) = 0;
};

}