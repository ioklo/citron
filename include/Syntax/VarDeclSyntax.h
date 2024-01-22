#pragma once
#include "SyntaxConfig.h"
#include <vector>
#include <string>
#include <optional>

#include "TypeExpSyntaxes.h"
#include "ExpSyntaxes.h"

namespace Citron {

// var a = ref i; 도 있어서 refVarDecl, VarDecl나누지 말고 하나에서 다 처리한다
struct VarDeclElementSyntax
{
    std::u32string VarName;
    std::optional<ExpSyntax> InitExp;
};

struct VarDeclSyntax
{
    TypeExpSyntax Type;
    std::vector<VarDeclElementSyntax> Elems;
};

} // namespace Citron