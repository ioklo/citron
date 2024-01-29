#pragma once
#include "SyntaxConfig.h"

#include <variant>
#include <string>
#include "ExpSyntaxes.h"

namespace Citron {

struct TextStringExpSyntaxElement
{
    std::u32string text;
};

BEGIN_IMPLEMENT_JSON_STRUCT_INLINE(TextStringExpSyntaxElement, syntax)
    IMPLEMENT_JSON_MEMBER_DIRECT(syntax, text)
END_IMPLEMENT_JSON_STRUCT_INLINE()

struct ExpStringExpSyntaxElement
{
    ExpSyntax exp;
};

BEGIN_IMPLEMENT_JSON_STRUCT_INLINE(ExpStringExpSyntaxElement, syntax)
    IMPLEMENT_JSON_MEMBER_DIRECT(syntax, exp)
END_IMPLEMENT_JSON_STRUCT_INLINE()

using StringExpSyntaxElement = std::variant<TextStringExpSyntaxElement, ExpStringExpSyntaxElement>;

inline JsonItem ToJson(StringExpSyntaxElement& syntax)
{
    return std::visit([](auto&& elem) { return ToJson(elem); }, syntax);
}


} // namespace Citron


