#pragma once
#include "SyntaxConfig.h"
#include <variant>

#include "StructDeclSyntax.h"
#include "ClassDeclSyntax.h"
#include "EnumDeclSyntax.h"

namespace Citron {

using TypeDeclSyntax = std::variant<ClassDeclSyntax, StructDeclSyntax, EnumDeclSyntax>;

}