#pragma once
#include "SyntaxConfig.h"
#include <variant>
#include <Infra/Json.h>

#include "NamespaceDeclSyntax.h"
#include "GlobalFuncDeclSyntax.h"
#include "TypeDeclSyntax.h"

namespace Citron {

class NamespaceDeclScriptSyntaxElement
{
    NamespaceDeclSyntax namespaceDecl;

public:
    NamespaceDeclScriptSyntaxElement(NamespaceDeclSyntax namespaceDecl)
        : namespaceDecl(std::move(namespaceDecl)) { }

    NamespaceDeclSyntax& GetNamespaceDecl() { return namespaceDecl; }

    SYNTAX_API JsonItem ToJson();
};

class GlobalFuncDeclScriptSyntaxElement
{
    GlobalFuncDeclSyntax funcDecl;

public:
    GlobalFuncDeclScriptSyntaxElement(GlobalFuncDeclSyntax funcDecl)
        : funcDecl(std::move(funcDecl)) { }

    GlobalFuncDeclSyntax& GetFuncDecl() { return funcDecl; }

    SYNTAX_API JsonItem ToJson();
};

class TypeDeclScriptSyntaxElement
{
    TypeDeclSyntax typeDecl;

public:
    TypeDeclScriptSyntaxElement(TypeDeclSyntax typeDecl)
        : typeDecl(std::move(typeDecl)) { }

    TypeDeclSyntax& GetTypeDecl() { return typeDecl; }

    SYNTAX_API JsonItem ToJson();
};

using ScriptSyntaxElement = std::variant<NamespaceDeclScriptSyntaxElement, GlobalFuncDeclScriptSyntaxElement, TypeDeclScriptSyntaxElement>;
SYNTAX_API JsonItem ToJson(ScriptSyntaxElement& syntax);

} // namespace Citron


