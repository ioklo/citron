#pragma once
#include "SyntaxConfig.h"
#include <variant>
#include <Infra/Json.h>

#include "GlobalFuncDeclSyntax.h"
#include "NamespaceDeclSyntax.h"
#include "TypeDeclSyntax.h"

#include "SyntaxMacros.h"

namespace Citron {

class GlobalFuncDeclNamespaceDeclSyntaxElement
{
    GlobalFuncDeclSyntax funcDecl;

public:
    GlobalFuncDeclNamespaceDeclSyntaxElement(GlobalFuncDeclSyntax funcDecl)
        : funcDecl(std::move(funcDecl)) { }

    GlobalFuncDeclSyntax& GetFuncDecl() { return funcDecl; }

    SYNTAX_API JsonItem ToJson();
};

class NamespaceDeclNamespaceDeclSyntaxElement
{
    NamespaceDeclSyntax namespaceDecl;

public:
    NamespaceDeclNamespaceDeclSyntaxElement(NamespaceDeclSyntax namespaceDecl)
        : namespaceDecl(std::move(namespaceDecl)) { }

    NamespaceDeclSyntax& GetNamespaceDecl() { return namespaceDecl; }

    SYNTAX_API JsonItem ToJson();
};

class TypeDeclNamespaceDeclSyntaxElement
{
    TypeDeclSyntax typeDecl;

public:
    TypeDeclNamespaceDeclSyntaxElement(TypeDeclSyntax typeDecl)
        : typeDecl(std::move(typeDecl)) { }

    TypeDeclSyntax& GetTypeDecl() { return typeDecl; }

    SYNTAX_API JsonItem ToJson();
};

using NamespaceDeclSyntaxElement = std::variant<GlobalFuncDeclNamespaceDeclSyntaxElement, NamespaceDeclNamespaceDeclSyntaxElement, TypeDeclNamespaceDeclSyntaxElement>;
SYNTAX_API JsonItem ToJson(NamespaceDeclSyntaxElement& syntax);

} // namespace Citron
