#pragma once
#include "SyntaxConfig.h"
#include <vector>
#include <string>
#include <optional>

#include "TypeExps.h"
#include "Exps.h"

namespace Citron::Syntax {

// var a = ref i; �� �־ refVarDecl, VarDecl������ ���� �ϳ����� �� ó���Ѵ�
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