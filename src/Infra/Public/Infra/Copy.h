#pragma once

#include <optional>
#include <vector>

namespace Citron {

template<typename T>
concept Copyable = requires(T t)
{
    t.Copy();
};

template<typename T>
T Copy(const T& t) requires Copyable<T>
{
    return t.Copy();
}

template<typename T>
std::optional<T> Copy(const std::optional<T>& v)
{
    if (v)
        return Copy(*v);
    else
        return std::nullopt;
}

template<typename T>
std::vector<T> Copy(const std::vector<T>& v)
{
    std::vector<T> copied;
    copied.reserve(v.size());

    for (auto& e : v)
        copied.push_back(Copy(e));

    return copied;
}

}