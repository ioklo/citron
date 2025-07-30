#pragma once
#include "BuilderConfig.h"

#include <memory>

namespace Citron {
class IFileSystem;

class Builder
{
    std::unique_ptr<IFileSystem> fileSystem;

public:
    BUILDER_API Builder(std::unique_ptr<IFileSystem>&& fileSystem);
    BUILDER_API ~Builder();
};

}