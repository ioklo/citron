#pragma once
#include "SyntaxConfig.h"
#include <variant>

#include "StructDeclSyntax.h"
#include "ClassDeclSyntax.h"
#include "EnumDeclSyntax.h"

namespace Citron {

using TypeDeclSyntax = std::variant<ClassDeclSyntax, StructDeclSyntax, EnumDeclSyntax>;
inline JsonItem ToJson(TypeDeclSyntax& syntax)
{
    return std::visit([](auto&& decl) { return decl.ToJson(); }, syntax);
}

}