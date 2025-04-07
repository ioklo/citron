export module Citron.SyntaxIR0Translator:SyntaxIR0Translator;

import <optional>;
import <memory>;
import <vector>;
import <expected>;

import Citron.Diag;
import Citron.MDecls;
import Citron.NDecls;

import Citron.Syntax;

import "SyntaxIR0TranslatorConfig.h";

namespace Citron {

export SYNTAXIR0TRANSLATOR_API
std::expected<std::shared_ptr<NModule>, DiagPtr> Translate(
    MName moduleName,
    std::vector<SScript> scripts,
    std::vector<std::shared_ptr<MModule>> referenceModules,
    RTypeFactory& factory);

} // namespace Citron