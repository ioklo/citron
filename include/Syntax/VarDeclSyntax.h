#pragma once
#include "SyntaxConfig.h"
#include <vector>
#include <string>
#include <optional>

#include "TypeExpSyntaxes.h"
#include "ExpSyntaxes.h"

namespace Citron {

// var a = ref i; 도 있어서 refVarDecl, VarDecl나누지 말고 하나에서 다 처리한다
struct VarDeclSyntaxElement
{
    std::u32string varName;
    std::optional<ExpSyntax> initExp;
};

BEGIN_IMPLEMENT_JSON_STRUCT_INLINE(VarDeclSyntaxElement, syntax)
    IMPLEMENT_JSON_MEMBER_DIRECT(syntax, varName)
    IMPLEMENT_JSON_MEMBER_DIRECT(syntax, initExp)
END_IMPLEMENT_JSON_STRUCT_INLINE()

struct VarDeclSyntax
{
    TypeExpSyntax type;
    std::vector<VarDeclSyntaxElement> elems;
};

BEGIN_IMPLEMENT_JSON_STRUCT_INLINE(VarDeclSyntax, syntax)
    IMPLEMENT_JSON_MEMBER_DIRECT(syntax, type)
    IMPLEMENT_JSON_MEMBER_DIRECT(syntax, elems)
END_IMPLEMENT_JSON_STRUCT_INLINE()


} // namespace Citron