#pragma once
#include "RSymbolConfig.h"
#include <variant>

namespace Citron {

class RType;
class RFactory;

struct RFuncReturn_None {}; // for ctor, dtor
struct RFuncReturn_Normal { RType* type; };
struct RFuncReturn_NotSet {}; // need inference

class RFuncReturn
{
    using Variant = std::variant<RFuncReturn_None, RFuncReturn_Normal, RFuncReturn_NotSet>;
    Variant v;

public:
    RFuncReturn() : v{RFuncReturn_None{}} {}

    template<typename T> requires (!std::same_as<std::remove_cvref_t<T>, RFuncReturn>) && std::constructible_from<Variant, T&&>
    RFuncReturn(T&& t) : v{std::forward<T>(t)} {}

    RSYMBOL_API RType* GetType(RFactory* rFactory);

    template<typename... TArgs>
    auto Visit(TArgs&&... args) { return std::visit(std::forward<TArgs>(args)..., v); }

    template<typename... TArgs>
    static auto Visit(RFuncReturn& x, RFuncReturn& y, TArgs&&... args) { return std::visit(std::forward<TArgs>(args)..., x.v, y.v); }

    bool IsNotSet() { return std::holds_alternative<RFuncReturn_NotSet>(v); }
    RFuncReturn_Normal* TryGetNormal() { return std::get_if<RFuncReturn_Normal>(&v); }

    
};

}

