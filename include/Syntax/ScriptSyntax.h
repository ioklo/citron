#pragma once
#include "SyntaxConfig.h"
#include <vector>

#include "ScriptElementSyntaxes.h"

namespace Citron {

// 가장 외곽
class ScriptSyntax
{
    std::vector<ScriptElementSyntax> elems;

public:
    ScriptSyntax(std::vector<ScriptElementSyntax> elems)
        : elems(elems) { }

    std::vector<ScriptElementSyntax>& GetElements() { return elems; }
};

} // namespace Citron
