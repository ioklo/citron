#pragma once
#include "SyntaxConfig.h"
#include <Infra/Json.h>
#include <string>

namespace Citron {

// ISyntaxNode로 지정하기 위해서 클래스로
struct TypeParamSyntax
{
    std::u32string Name;
};

BEGIN_IMPLEMENT_JSON_STRUCT_INLINE(TypeParamSyntax, syntax)
    IMPLEMENT_JSON_MEMBER_DIRECT(syntax, Name)
END_IMPLEMENT_JSON_STRUCT_INLINE()

} // namespace Citron
