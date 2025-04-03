export module Citron.SyntaxIR0Translator:ImCallableAndSArgsToNExpTranslation;

import <memory>;
import <vector>;

import Citron.Syntax;
import Citron.RDecls;
import Citron.NDecls;

namespace Citron::SyntaxIR0Translator {

export class ImExp;
export class TranslationContext;

export NExpPtr TranslateImCallableAndSArgsToNExp(ImExp& imCallable, const SExpPtr& sCallable, const SArgumentsPtr& sArgs, TranslationContext& context);

} // namespace Citron::SyntaxIR0Translator