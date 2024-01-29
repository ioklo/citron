#pragma once
#include "SyntaxConfig.h"
#include <vector>
#include <string>
#include <variant>
#include <Infra/Json.h>

#include "SyntaxMacros.h"

namespace Citron {

// forward declarations
using NamespaceSyntaxElement = std::variant<class GlobalFuncDeclNamespaceSyntaxElement, class NamespaceDeclNamespaceSyntaxElement, class TypeDeclNamespaceSyntaxElement>;

class NamespaceDeclSyntax
{
    std::vector<std::u32string> names; // dot seperated names, NS1.NS2
    std::vector<NamespaceSyntaxElement> elements;

public:
    SYNTAX_API NamespaceDeclSyntax(std::vector<std::u32string> names, std::vector<NamespaceSyntaxElement> elements);
    DECLARE_DEFAULTS(NamespaceDeclSyntax)

    std::vector<std::u32string>& GetNames() { return names; }
    std::vector<NamespaceSyntaxElement>& GetElements() { return elements; }

    SYNTAX_API JsonItem ToJson();
};

} // namespace Citron

