#pragma once

#include <variant>

#include "RSymbol/RNames.h"
#include "SmDeclRes.h"

namespace Citron {

class RType;

class SmBodyRes;

struct SmBodyRes_DeclRes { SmDeclRes declRes; };
struct SmBodyRes_LocalVar { RType* type; RName name; };
struct SmBodyRes_LocalRef { RType* type; RName name; };
struct SmBodyRes_NeedCapture { RName name; std::unique_ptr<SmBodyRes> member; };
struct SmBodyRes_ThisVar { RType* type; };

// Body-Space Resolved Result
class SmBodyRes
{
    using Variant = std::variant<
        SmBodyRes_DeclRes,
        SmBodyRes_LocalVar,
        SmBodyRes_LocalRef,
        SmBodyRes_NeedCapture, // 람다에서 캡쳐가 필요할때
        SmBodyRes_ThisVar
    >;

    Variant v;

public:
    template<typename T> requires (!std::same_as<std::remove_cvref_t<T>, SmBodyRes>) && std::constructible_from<Variant, T&&>
    SmBodyRes(T&& res) : v{std::forward<T>(res)} {}

    template<typename... TArgs>
    auto Visit(TArgs&&... args) & { return std::visit(std::forward<TArgs>(args)..., v); }

    template<typename... TArgs>
    auto Visit(TArgs&&... args) && { return std::visit(std::forward<TArgs>(args)..., std::move(v)); }
};

} // namespace Citron
