#pragma once

#include <variant>

#include "RSymbol/RDeclRes.h"
#include "RSymbol/RNames.h"

namespace Citron {

class RType;

class BodyRes;

struct BodyRes_RDeclRes { RDeclRes declRes; };
struct BodyRes_LocalVar { RType* type; RName name; };
struct BodyRes_LocalRef { RType* type; RName name; };
struct BodyRes_NeedCapture { RName name; std::unique_ptr<BodyRes> member; };
struct BodyRes_ThisVar { RType* type; };

// Body-Space Resolved Result
class BodyRes
{
    using Variant = std::variant<
        BodyRes_RDeclRes,
        BodyRes_LocalVar,
        BodyRes_LocalRef,
        BodyRes_NeedCapture, // 람다에서 캡쳐가 필요할때
        BodyRes_ThisVar
    >;

    Variant v;

public:
    template<typename T> requires (!std::same_as<std::remove_cvref_t<T>, BodyRes>) && std::constructible_from<Variant, T&&>
    BodyRes(T&& res) : v{std::forward<T>(res)} {}

    template<typename... TArgs>
    auto Visit(TArgs&&... args) { return std::visit(std::forward<TArgs>(args)..., v); }
};

} // namespace Citron
