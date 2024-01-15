#pragma once
#include "SyntaxConfig.h"
#include <vector>
#include <string>
#include <variant>

#include "SyntaxMacros.h"

namespace Citron::Syntax {

// forward declarations
using NamespaceElement = std::variant<class GlobalFuncDeclNamespaceElement, class NamespaceDeclNamespaceElement, class TypeDeclNamespaceElement>;

class NamespaceDecl
{
    std::vector<std::string> names; // dot seperated names, NS1.NS2
    std::vector<NamespaceElement> elements;

public:
    SYNTAX_API NamespaceDecl(std::vector<std::string> names, std::vector<NamespaceElement> elements);
    DECLARE_DEFAULTS(NamespaceDecl)

    std::vector<std::string>& GetNames() { return names; }
    std::vector<NamespaceElement>& GetElements() { return elements; }
};

} // namespace Citron::Syntax

