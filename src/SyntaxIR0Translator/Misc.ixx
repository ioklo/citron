export module Citron.SyntaxIR0Translator:Misc;

import <memory>;
import <vector>;
import <expected>;

import Citron.Diag;
import Citron.Syntax;
import Citron.RDecls;
import Citron.NDecls;

namespace Citron::SyntaxIR0Translator {

export class TranslationContext;

export RTypeArgumentsPtr MakeTypeArgs(std::vector<STypeExpPtr>& typeArgs, TranslationContext& context);

export std::expected<NExpPtr, DiagPtr> CastNExp(NExpPtr&& exp, const RTypePtr& expectedType, TranslationContext& context);
export std::expected<NExpPtr, DiagPtr> CastNExp(const NExpPtr& exp, const RTypePtr& expectedType, TranslationContext& context);

export bool IsVarType(STypeExp& typeExp);

export RName_CtorParam MakeBaseCtorParamName(size_t index, RName baseParamName);

} // namespace Citron::SyntaxIR0Translator
