export module Citron.SyntaxIR0Translator:MemberDeclPhaseContext;

import <vector>;
import <tuple>;
import <functional>;

import Citron.Syntax;

import Citron.RDecls;
import Citron.NDecls;

namespace Citron::SyntaxIR0Translator {

export class BodyPhaseContext;

export class MemberDeclPhaseContext
{
public:
    RTypePtr MakeType(const STypeExpPtr& sTypeExp, NDeclPtr decl);
    std::tuple<std::vector<RFuncParameter>, bool> MakeParameters(NDeclPtr decl, std::vector<SFuncParam>& sParams);

    void AddBodyPhaseTask(std::function<void(BodyPhaseContext&)> task);
    void AddTrivialCtorPhaseTask(std::function<void()> task);
};

} // namespace Citron::SyntaxIR0Translator