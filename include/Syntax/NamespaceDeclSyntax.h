#pragma once
#include "SyntaxConfig.h"
#include <vector>
#include <string>
#include <variant>
#include <Infra/Json.h>

#include "SyntaxMacros.h"

namespace Citron {

// forward declarations
using NamespaceDeclSyntaxElement = std::variant<class GlobalFuncDeclNamespaceDeclSyntaxElement, class NamespaceDeclNamespaceDeclSyntaxElement, class TypeDeclNamespaceDeclSyntaxElement>;

class NamespaceDeclSyntax
{
    std::vector<std::u32string> names; // dot seperated names, NS1.NS2
    std::vector<NamespaceDeclSyntaxElement> elements;

public:
    SYNTAX_API NamespaceDeclSyntax(std::vector<std::u32string> names, std::vector<NamespaceDeclSyntaxElement> elements);
    DECLARE_DEFAULTS(NamespaceDeclSyntax)

    std::vector<std::u32string>& GetNames() { return names; }
    std::vector<NamespaceDeclSyntaxElement>& GetElements() { return elements; }

    SYNTAX_API JsonItem ToJson();
};

} // namespace Citron

