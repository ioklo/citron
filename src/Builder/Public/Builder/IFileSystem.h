#pragma once

#include <vector>
#include <cstddef>
#include <optional>
#include <filesystem>

namespace Citron {

class IFileSystem 
{
public:
    virtual ~IFileSystem() { }
    virtual std::optional<std::vector<std::byte>> GetFileContents(const std::filesystem::path& path) = 0;
};

} // namespace Citron