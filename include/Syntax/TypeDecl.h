#pragma once
#include "SyntaxConfig.h"
#include <variant>

#include "StructDecl.h"
#include "ClassDecl.h"
#include "EnumDecl.h"

namespace Citron::Syntax {

using TypeDecl = std::variant<ClassDecl, StructDecl, EnumDecl>;

}