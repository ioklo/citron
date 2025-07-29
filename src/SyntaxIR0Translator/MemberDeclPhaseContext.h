#pragma once

#include <vector>
#include <tuple>
#include <functional>

#include "Syntax/Syntax.h"

namespace Citron {

struct RFuncParameter;
class RType;
using RTypePtr = std::shared_ptr<RType>;

class NDecl;
using NDeclPtr = std::shared_ptr<NDecl>;

namespace SyntaxIR0Translator {

class BodyPhaseContext;

class MemberDeclPhaseContext
{
public:
    RTypePtr MakeType(const STypeExpPtr& sTypeExp, NDeclPtr decl);
    std::tuple<std::vector<RFuncParameter>, bool> MakeParameters(NDeclPtr decl, std::vector<SFuncParam>& sParams);

    void AddBodyPhaseTask(std::function<void(BodyPhaseContext&)> task);
    void AddTrivialCtorPhaseTask(std::function<void()> task);
};

} // namespace SyntaxIR0Translator
} // namespace Citron