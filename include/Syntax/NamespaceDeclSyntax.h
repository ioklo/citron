#pragma once
#include "SyntaxConfig.h"
#include <vector>
#include <string>
#include <variant>

#include "SyntaxMacros.h"

namespace Citron {

// forward declarations
using NamespaceElementSyntax = std::variant<class GlobalFuncDeclNamespaceElementSyntax, class NamespaceDeclNamespaceElementSyntax, class TypeDeclNamespaceElementSyntax>;

class NamespaceDeclSyntax
{
    std::vector<std::u32string> names; // dot seperated names, NS1.NS2
    std::vector<NamespaceElementSyntax> elements;

public:
    SYNTAX_API NamespaceDeclSyntax(std::vector<std::u32string> names, std::vector<NamespaceElementSyntax> elements);
    DECLARE_DEFAULTS(NamespaceDeclSyntax)

    std::vector<std::u32string>& GetNames() { return names; }
    std::vector<NamespaceElementSyntax>& GetElements() { return elements; }
};

} // namespace Citron

