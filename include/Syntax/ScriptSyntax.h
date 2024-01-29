#pragma once
#include "SyntaxConfig.h"
#include <vector>
#include <Infra/Json.h>

#include "ScriptSyntaxElements.h"

namespace Citron {

// 가장 외곽
class ScriptSyntax
{
    std::vector<ScriptSyntaxElement> elems;

public:
    ScriptSyntax(std::vector<ScriptSyntaxElement> elems)
        : elems(elems) { }

    std::vector<ScriptSyntaxElement>& GetElements() { return elems; }

    SYNTAX_API JsonItem ToJson();
};

} // namespace Citron
