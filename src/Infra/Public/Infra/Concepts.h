#pragma once
#include <concepts>

namespace Citron {

template<class T>
concept UnqualifiedType = std::same_as<T, std::remove_cvref_t<T>>;

} // namespace Citron