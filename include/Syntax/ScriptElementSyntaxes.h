#pragma once
#include "SyntaxConfig.h"
#include <variant>
#include "NamespaceDeclSyntax.h"
#include "GlobalFuncDeclSyntax.h"
#include "TypeDeclSyntax.h"

namespace Citron {

class NamespaceDeclScriptElementSyntax
{
    NamespaceDeclSyntax namespaceDecl;

public:
    NamespaceDeclScriptElementSyntax(NamespaceDeclSyntax namespaceDecl)
        : namespaceDecl(std::move(namespaceDecl)) { }

    NamespaceDeclSyntax& GetNamespaceDecl() { return namespaceDecl; }
};

class GlobalFuncDeclScriptElementSyntax
{
    GlobalFuncDeclSyntax funcDecl;

public:
    GlobalFuncDeclScriptElementSyntax(GlobalFuncDeclSyntax funcDecl)
        : funcDecl(std::move(funcDecl)) { }

    GlobalFuncDeclSyntax& GetFuncDecl() { return funcDecl; }
};

class TypeDeclScriptElementSyntax
{
    TypeDeclSyntax typeDecl;

public:
    TypeDeclScriptElementSyntax(TypeDeclSyntax typeDecl)
        : typeDecl(std::move(typeDecl)) { }

    TypeDeclSyntax& GetTypeDecl() { return typeDecl; }
};

using ScriptElementSyntax = std::variant<NamespaceDeclScriptElementSyntax, GlobalFuncDeclScriptElementSyntax, TypeDeclScriptElementSyntax>;

} // namespace Citron


