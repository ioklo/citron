#pragma once

#include <vector>
#include <memory>

namespace Citron {

class IModuleDecl;

namespace SyntaxIR0Translator {

struct ModuleDecls
{
    std::vector<IModuleDecl> moduleDecls;
};

using ModuleDeclsPtr = std::shared_ptr<ModuleDecls>;

} // namespace SyntaxIR0Translator

} // namespace Citron
