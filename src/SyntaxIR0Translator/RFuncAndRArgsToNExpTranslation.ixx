export module Citron.SyntaxIR0Translator:RFuncAndRArgsToNExpTranslation;

import <memory>;
import <vector>;

import Citron.RDecls;
import Citron.NDecls;

namespace Citron::SyntaxIR0Translator {

export NExpPtr TranslateRFuncAndNArgsToNExp(const std::shared_ptr<RFuncDecl>& decl, const RTypeArgumentsPtr& typeArgs, NLocPtr&& instance, std::vector<NArgument>&& args);

} // namespace Citron::SyntaxIR0Translator