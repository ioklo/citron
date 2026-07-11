#pragma once
#include <variant>

namespace Citron {

class RType;

struct RThisKind_Static {};
struct RThisKind_Handle { RType* type; }; // handle 자체의 타입. 즉, class C라면 C
struct RThisKind_Ref { RType* type; }; // type은 원래 타입. 즉 struct S라면 S*가 아니라 S

class RThisKind
{
    using Variant = std::variant<RThisKind_Static, RThisKind_Handle, RThisKind_Ref>;
    Variant v;

public:
    template<typename T> 
        requires (!std::same_as<std::remove_cvref_t<T>, RThisKind>) && std::constructible_from<Variant, T&&>
    RThisKind(T&& t) : v{std::forward<T>(t)} {}

    RType* GetThisType()
    {
        return std::visit([](auto&& arg) -> RType* {
            using T = std::remove_cvref_t<decltype(arg)>;
            if constexpr (std::same_as<T, RThisKind_Static>)
                return nullptr;
            else if constexpr (std::same_as<T, RThisKind_Handle>)
                return arg.type;
            else if constexpr (std::same_as<T, RThisKind_Ref>)
                return arg.type;
        }, v);
    }

    bool IsStatic()
    {
        return std::holds_alternative<RThisKind_Static>(v);
    }
};

} // namespace Citron
