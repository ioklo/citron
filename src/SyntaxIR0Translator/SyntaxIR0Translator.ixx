export module Citron.SyntaxIR0Translator:SyntaxIR0Translator;

import <optional>;
import <memory>;
import <vector>;

import Citron.MDecls;
import Citron.NDecls;

import Citron.Syntax;

import "SyntaxIR0TranslatorConfig.h";

namespace Citron {

export SYNTAXIR0TRANSLATOR_API
std::shared_ptr<NModule> Translate(
    MName moduleName,
    std::vector<SScript> scripts,
    std::vector<std::shared_ptr<MModule>> referenceModules,
    RTypeFactory& factory);

} // namespace Citron