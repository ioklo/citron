#pragma once

#include <optional>

namespace Citron
{
class RDecl;

class NNamespaceDecl;
class NGlobalFuncDecl;
class NStructDecl;
class NStructCtorDecl;
class NStructDtorDecl;
class NStructFuncDecl;
class NStructVarDecl;
class NClassDecl;
class NClassCtorDecl;
class NClassFuncDecl;
class NClassVarDecl;
class NEnumDecl;
class NEnumElemDecl;
class NEnumElemVarDecl;
class NLambdaDecl;
class NLambdaVarDecl;
class NInterfaceDecl;
class NTypeParamDecl;

class NDeclVisitor
{
public:
    virtual ~NDeclVisitor() {}
    virtual void Visit(NNamespaceDecl* decl) = 0;
    virtual void Visit(NGlobalFuncDecl* decl) = 0;
    virtual void Visit(NStructDecl* decl) = 0;
    virtual void Visit(NStructCtorDecl* decl) = 0;
    virtual void Visit(NStructDtorDecl* decl) = 0;
    virtual void Visit(NStructFuncDecl* decl) = 0;
    virtual void Visit(NStructVarDecl* decl) = 0;
    virtual void Visit(NClassDecl* decl) = 0;
    virtual void Visit(NClassCtorDecl* decl) = 0;
    virtual void Visit(NClassFuncDecl* decl) = 0;
    virtual void Visit(NClassVarDecl* decl) = 0;
    virtual void Visit(NEnumDecl* decl) = 0;
    virtual void Visit(NEnumElemDecl* decl) = 0;
    virtual void Visit(NEnumElemVarDecl* decl) = 0;
    virtual void Visit(NLambdaDecl* decl) = 0;
    virtual void Visit(NLambdaVarDecl* decl) = 0;
    virtual void Visit(NInterfaceDecl* decl) = 0;
    virtual void Visit(NTypeParamDecl* decl) = 0;
};

class NDecl
{
public:
    virtual ~NDecl() {}

    virtual RDecl* GetRDecl() = 0;
    virtual NDecl* GetNOuter() = 0;
    virtual void Accept(NDeclVisitor& visitor) = 0;
};

}