#pragma once

#include <vector>
#include <tuple>
#include <functional>

#include <Syntax/Syntax.h>

#include <IR0/RType.h>
#include <IR0/NDecl.h>
#include <IR0/RFuncParameter.h>

namespace Citron {

namespace SyntaxIR0Translator {

class BodyPhaseContext;

class MemberDeclPhaseContext
{
public:
    RTypePtr MakeType(const STypeExpPtr& sTypeExp, NDeclPtr decl);
    std::tuple<std::vector<RFuncParameter>, bool> MakeParameters(NDeclPtr decl, std::vector<SFuncParam>& sParams);

    void AddBodyPhaseTask(std::function<void(BodyPhaseContext&)> task);
    void AddTrivialConstructorPhaseTask(std::function<void()> task);
};

} // namespace SyntaxIR0Translator

} // namespace Citron