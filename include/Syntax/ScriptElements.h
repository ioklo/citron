#pragma once
#include "SyntaxConfig.h"
#include <variant>
#include "NamespaceDecl.h"
#include "GlobalFuncDecl.h"
#include "TypeDecl.h"

namespace Citron::Syntax {

class NamespaceDeclScriptElement
{
    NamespaceDecl namespaceDecl;

public:
    NamespaceDeclScriptElement(NamespaceDecl namespaceDecl)
        : namespaceDecl(std::move(namespaceDecl)) { }

    NamespaceDecl& GetNamespaceDecl() { return namespaceDecl; }
};

class GlobalFuncDeclScriptElement
{
    GlobalFuncDecl funcDecl;

public:
    GlobalFuncDeclScriptElement(GlobalFuncDecl funcDecl)
        : funcDecl(std::move(funcDecl)) { }

    GlobalFuncDecl& GetFuncDecl() { return funcDecl; }
};

class TypeDeclScriptElement
{
    TypeDecl typeDecl;

public:
    TypeDeclScriptElement(TypeDecl typeDecl)
        : typeDecl(std::move(typeDecl)) { }

    TypeDecl& GetTypeDecl() { return typeDecl; }
};

using ScriptElement = std::variant<NamespaceDeclScriptElement, GlobalFuncDeclScriptElement, TypeDeclScriptElement>;

} // namespace Citron::Syntax


