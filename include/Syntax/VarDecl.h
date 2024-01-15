#pragma once
#include "SyntaxConfig.h"
#include <vector>
#include <string>
#include <optional>

#include "TypeExps.h"
#include "Exps.h"

namespace Citron::Syntax {

// var a = ref i; 도 있어서 refVarDecl, VarDecl나누지 말고 하나에서 다 처리한다
struct VarDeclElement
{
    std::string VarName;
    std::optional<Exp> InitExp;
};

struct VarDecl
{
    TypeExp Type;
    std::vector<VarDeclElement> Elems;
};

} // namespace Citron::Syntax