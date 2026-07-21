#pragma once

#include <variant>

namespace Citron {

struct NImplTraitFunc; 

class NImplTraitMember
{
    using Variant = std::variant<NImplTraitFunc*>; // id로 갖고 있어야 해서 포인터로 관리한다
    Variant v;

public:
    template<typename T> requires (!std::same_as<T, NImplTraitMember>) && std::constructible_from<Variant, T&&>
    NImplTraitMember(T&& value) : v{std::forward<T>(value)} {}
};


} // namespace Citron 