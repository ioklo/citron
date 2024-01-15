#pragma once
#include "SyntaxConfig.h"
#include <variant>
#include "GlobalFuncDecl.h"
#include "NamespaceDecl.h"
#include "TypeDecl.h"

#include "SyntaxMacros.h"

namespace Citron::Syntax {

class GlobalFuncDeclNamespaceElement
{
    GlobalFuncDecl funcDecl;

public:
    GlobalFuncDeclNamespaceElement(GlobalFuncDecl funcDecl)
        : funcDecl(std::move(funcDecl)) { }

    GlobalFuncDecl& GetFuncDecl() { return funcDecl; }
};

class NamespaceDeclNamespaceElement
{
    NamespaceDecl namespaceDecl;

public:
    NamespaceDeclNamespaceElement(NamespaceDecl namespaceDecl)
        : namespaceDecl(std::move(namespaceDecl)) { }

    NamespaceDecl& GetNamespaceDecl() { return namespaceDecl; }
};

class TypeDeclNamespaceElement
{
    TypeDecl typeDecl;

public:
    TypeDeclNamespaceElement(TypeDecl typeDecl)
        : typeDecl(std::move(typeDecl)) { }

    TypeDecl& GetTypeDecl() { return typeDecl; }
};

using NamespaceElement = std::variant<GlobalFuncDeclNamespaceElement, NamespaceDeclNamespaceElement, TypeDeclNamespaceElement>;

} // namespace Citron::Syntax
