#pragma once

#include <vector>
#include <tuple>
#include <functional>

#include <Syntax/Syntax.h>

#include <IR0/RType.h>
#include <IR0/RDecl.h>
#include <IR0/RFuncParameter.h>

namespace Citron {

namespace SyntaxIR0Translator {

class BodyPhaseContext;

class MemberDeclPhaseContext
{
public:
    RTypePtr MakeType(const STypeExpPtr& sTypeExp, RDeclPtr decl);
    std::tuple<std::vector<RFuncParameter>, bool> MakeParameters(RDeclPtr decl, std::vector<SFuncParam>& sParams);

    void AddBodyPhaseTask(std::function<void(BodyPhaseContext&)> task);
    void AddTrivialConstructorPhaseTask(std::function<void()> task);
};

} // namespace SyntaxIR0Translator

} // namespace Citron