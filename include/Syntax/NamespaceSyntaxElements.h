#pragma once
#include "SyntaxConfig.h"
#include <variant>
#include <Infra/Json.h>

#include "GlobalFuncDeclSyntax.h"
#include "NamespaceDeclSyntax.h"
#include "TypeDeclSyntax.h"

#include "SyntaxMacros.h"

namespace Citron {

class GlobalFuncDeclNamespaceSyntaxElement
{
    GlobalFuncDeclSyntax funcDecl;

public:
    GlobalFuncDeclNamespaceSyntaxElement(GlobalFuncDeclSyntax funcDecl)
        : funcDecl(std::move(funcDecl)) { }

    GlobalFuncDeclSyntax& GetFuncDecl() { return funcDecl; }

    SYNTAX_API JsonItem ToJson();
};

class NamespaceDeclNamespaceSyntaxElement
{
    NamespaceDeclSyntax namespaceDecl;

public:
    NamespaceDeclNamespaceSyntaxElement(NamespaceDeclSyntax namespaceDecl)
        : namespaceDecl(std::move(namespaceDecl)) { }

    NamespaceDeclSyntax& GetNamespaceDecl() { return namespaceDecl; }

    SYNTAX_API JsonItem ToJson();
};

class TypeDeclNamespaceSyntaxElement
{
    TypeDeclSyntax typeDecl;

public:
    TypeDeclNamespaceSyntaxElement(TypeDeclSyntax typeDecl)
        : typeDecl(std::move(typeDecl)) { }

    TypeDeclSyntax& GetTypeDecl() { return typeDecl; }

    SYNTAX_API JsonItem ToJson();
};

using NamespaceSyntaxElement = std::variant<GlobalFuncDeclNamespaceSyntaxElement, NamespaceDeclNamespaceSyntaxElement, TypeDeclNamespaceSyntaxElement>;
SYNTAX_API JsonItem ToJson(NamespaceSyntaxElement& syntax);

} // namespace Citron
