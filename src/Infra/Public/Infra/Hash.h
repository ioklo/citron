#pragma once

#include <type_traits>
#include <vector>
#include <string>
#include <string_view>

namespace std {

template<typename T>
struct hash<std::vector<T>>
{
    std::size_t operator()(const std::vector<T>& vec) const noexcept {
        std::hash<T> hasher;
        std::size_t seed = vec.size();
        for (auto& v : vec) {
            seed ^= hasher(v) + 0x9e3779b9 + (seed << 6) + (seed >> 2);
        }
        return seed;
    }
};

}

namespace Citron {

// from https://stackoverflow.com/a/2595226/25053202
template <class T, class Hasher = std::hash<T>>
void hash_combine(std::size_t& seed, const T& v)
{
    Hasher hasher;
    seed ^= hasher(v) + 0x9e3779b9 + (seed << 6) + (seed >> 2);
}

struct transparent_string_hash : std::hash<std::string>, std::hash<std::string_view>
{
    using is_transparent = void;
    using std::hash<std::string>::operator();
    using std::hash<std::string_view>::operator();
};

} // namespace Citron