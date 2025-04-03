export module Citron.SyntaxIR0Translator:Misc;

import <memory>;
import <vector>;

import Citron.Syntax;
import Citron.RDecls;
import Citron.NDecls;

namespace Citron::SyntaxIR0Translator {

export class TranslationContext;

export RTypeArgumentsPtr MakeTypeArgs(std::vector<STypeExpPtr>& typeArgs, TranslationContext& context);

export NExpPtr TryCastRExp(NExpPtr&& exp, const RTypePtr& expectedType, TranslationContext& context); // nothrow
export NExpPtr CastNExp(NExpPtr&& exp, const RTypePtr& expectedType, TranslationContext& context);

export bool IsVarType(STypeExp& typeExp);

export RName_CtorParam MakeBaseCtorParamName(size_t index, RName baseParamName);

} // namespace Citron::SyntaxIR0Translator
