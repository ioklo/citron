#pragma once

#include <type_traits>
#include <vector>

namespace Citron {

// from https://stackoverflow.com/a/2595226/25053202
template <class T>
inline void hash_combine(std::size_t& seed, const T& v)
{
    std::hash<T> hasher;
    seed ^= hasher(v) + 0x9e3779b9 + (seed << 6) + (seed >> 2);
}

}

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