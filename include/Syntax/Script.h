#pragma once
#include "SyntaxConfig.h"
#include <vector>

#include "ScriptElements.h"

namespace Citron::Syntax {

// ���� �ܰ�
class Script
{
    std::vector<ScriptElement> Elements;
};

} // namespace Citron::Syntax
