export module Citron.IFileSystem;

import <vector>;
import <cstddef>;
import <optional>;
import <filesystem>;

namespace Citron {

export 
class IFileSystem 
{
public:
    virtual std::optional<std::vector<std::byte>> GetFileContents(const std::filesystem::path& path) = 0;
};

} // namespace Citron