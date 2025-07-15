module Citron.Compiler;

namespace Citron {

Compiler::Compiler(std::unique_ptr<IFileSystem>&& fileSystem)
    : fileSystem(std::move(fileSystem))
{
}



} // namespace Citron