#pragma once
#include "RSymbolConfig.h"

#include <span>

#include "RFuncReturn.h"
#include "RFuncParameter.h"
#include "RThisKind.h"

namespace Citron {

class RDecl;
class RType;
class RTypeArguments;
class RTypeParamDecl;

class RGlobalFuncDecl;
class RClassCtorDecl;
class RClassFuncDecl;
class RStructCtorDecl;
class RStructDtorDecl;
class RStructFuncDecl;
class RLambdaDecl;

class RFuncDecl
{
    using Variant = std::variant<
        RGlobalFuncDecl*,
        RClassCtorDecl*,
        RClassFuncDecl*,
        RStructCtorDecl*,
        RStructDtorDecl*,
        RStructFuncDecl*,
        RLambdaDecl*
    >;

    Variant v;

public:
    template<typename T> requires (!std::same_as<std::remove_cvref_t<T>, RFuncDecl>) && std::constructible_from<Variant, T&&>
    RFuncDecl(T&& funcDecl) : v{std::forward<T>(funcDecl)} {}

    RSYMBOL_API RDecl* GetRDecl();
    RSYMBOL_API RThisKind GetThisKind();
    RSYMBOL_API size_t GetTypeParamCount();
    RSYMBOL_API RTypeParamDecl* GetTypeParam(size_t index);
    RSYMBOL_API size_t GetParamCount();
    RSYMBOL_API RType* GetReturnType(RTypeArguments* typeArgs);
    RSYMBOL_API RFuncReturn GetFuncReturn(RTypeArguments* typeArgs);
    RSYMBOL_API RFuncParameter GetFuncParam(RTypeArguments* typeArgs, size_t index);
    RSYMBOL_API RFuncReturn GetUnboundFuncReturn();
    RSYMBOL_API std::span<RFuncParameter> GetUnboundFuncParams();

    template<typename... TArgs>
    auto Visit(TArgs&&... args) { return std::visit(std::forward<TArgs>(args)..., v); }
};

} // namespace Citron

