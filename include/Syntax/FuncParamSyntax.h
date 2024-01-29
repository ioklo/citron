#pragma once
#include "SyntaxConfig.h"
#include <string>
#include <Infra/Json.h>
#include "TypeExpSyntaxes.h"

namespace Citron {

struct FuncParamSyntax
{
    bool hasOut;
    bool hasParams;
    TypeExpSyntax type; 
    std::u32string name;
};

BEGIN_IMPLEMENT_JSON_STRUCT_INLINE(FuncParamSyntax, syntax)
    IMPLEMENT_JSON_MEMBER_DIRECT(syntax, hasOut)
    IMPLEMENT_JSON_MEMBER_DIRECT(syntax, hasParams)
    IMPLEMENT_JSON_MEMBER_DIRECT(syntax, type)
    IMPLEMENT_JSON_MEMBER_DIRECT(syntax, name)
END_IMPLEMENT_JSON_STRUCT_INLINE()

} // namespace Citron

