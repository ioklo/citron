#pragma once
#include "SyntaxConfig.h"
#include <string>

namespace Citron {

// ISyntaxNode로 지정하기 위해서 클래스로
struct TypeParamSyntax
{
    std::u32string Name;
};

} // namespace Citron
