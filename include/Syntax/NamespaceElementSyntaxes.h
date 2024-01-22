#pragma once
#include "SyntaxConfig.h"
#include <variant>
#include "GlobalFuncDeclSyntax.h"
#include "NamespaceDeclSyntax.h"
#include "TypeDeclSyntax.h"

#include "SyntaxMacros.h"

namespace Citron {

class GlobalFuncDeclNamespaceElementSyntax
{
    GlobalFuncDeclSyntax funcDecl;

public:
    GlobalFuncDeclNamespaceElementSyntax(GlobalFuncDeclSyntax funcDecl)
        : funcDecl(std::move(funcDecl)) { }

    GlobalFuncDeclSyntax& GetFuncDecl() { return funcDecl; }
};

class NamespaceDeclNamespaceElementSyntax
{
    NamespaceDeclSyntax namespaceDecl;

public:
    NamespaceDeclNamespaceElementSyntax(NamespaceDeclSyntax namespaceDecl)
        : namespaceDecl(std::move(namespaceDecl)) { }

    NamespaceDeclSyntax& GetNamespaceDecl() { return namespaceDecl; }
};

class TypeDeclNamespaceElementSyntax
{
    TypeDeclSyntax typeDecl;

public:
    TypeDeclNamespaceElementSyntax(TypeDeclSyntax typeDecl)
        : typeDecl(std::move(typeDecl)) { }

    TypeDeclSyntax& GetTypeDecl() { return typeDecl; }
};

using NamespaceElementSyntax = std::variant<GlobalFuncDeclNamespaceElementSyntax, NamespaceDeclNamespaceElementSyntax, TypeDeclNamespaceElementSyntax>;

} // namespace Citron
