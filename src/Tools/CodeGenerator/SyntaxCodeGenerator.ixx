module;
#include <filesystem>

export module Citron.SyntaxCodeGenerator;

namespace Citron {

export void GenerateSyntax(std::filesystem::path srcPath);

}