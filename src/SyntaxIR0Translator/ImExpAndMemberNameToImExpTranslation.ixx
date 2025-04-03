export module Citron.SyntaxIR0Translator:ImExpAndMemberNameToImExpTranslation;

import <memory>;
import <string>;

import Citron.RDecls;

namespace Citron::SyntaxIR0Translator {

export class ImExp;
export using ImExpPtr = std::shared_ptr<ImExp>;

export class TranslationContext;

export ImExpPtr TranslateImExpAndMemberNameToImExp(ImExp& imExp, const std::string& name, const RTypeArgumentsPtr& typeArgs, TranslationContext& context);

} // namespace Citron::SyntaxIR0Translator