#include "Builder.h"

#include "IFileSystem.h"

namespace Citron {

Builder::Builder(std::unique_ptr<IFileSystem>&& fileSystem)
    : fileSystem(std::move(fileSystem))
{
}

Builder::~Builder() = default;

} // namespace Citron