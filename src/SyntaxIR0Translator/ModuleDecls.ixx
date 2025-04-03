export module Citron.SyntaxIR0Translator:ModuleDecls;

import <vector>;
import <memory>;

import Citron.RDecls;

namespace Citron::SyntaxIR0Translator {

export struct RModules
{
    std::vector<RModule> modules;
};

export using RModulesPtr = std::shared_ptr<RModules>;

} // namespace Citron::SyntaxIR0Translator
