#pragma once

#include <vector>
#include <tuple>
#include <functional>

#include "Syntax/Syntax.h"

namespace Citron {

struct RFuncParameter;
class RType;

class NDecl;

namespace SyntaxIR0Translator {

class BodyPhaseContext;

class MemberDeclPhaseContext
{
public:
    RType* MakeType(const STypeExpPtr& sTypeExp, NDecl* decl);
    std::tuple<std::vector<RFuncParameter>, bool> MakeParameters(NDecl* decl, std::vector<SFuncParam>& sParams);

    void AddBodyPhaseTask(std::function<void(BodyPhaseContext&)> task);
    void AddTrivialCtorPhaseTask(std::function<void()> task);
};

} // namespace SyntaxIR0Translator
} // namespace Citron