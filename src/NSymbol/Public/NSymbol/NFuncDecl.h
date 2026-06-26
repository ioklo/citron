#pragma once
#include "NSymbolConfig.h"

#include "RSymbol/RFuncReturn.h"
#include "RSymbol/RFuncDecl.h"
#include "NDecl.h"
#include "NFuncDeclOuter.h"

namespace Citron
{
struct RFuncParameter;

class NFuncDecl
{
    using Variant = std::variant<
        NGlobalFuncDecl*,
        NClassCtorDecl*,
        NClassFuncDecl*,
        NStructCtorDecl*,
        NStructDtorDecl*,
        NStructFuncDecl*,
        NLambdaDecl*>;

    Variant v;

public:
    template<typename T> requires (!std::same_as<std::remove_cvref_t<T>, NFuncDecl>) && std::constructible_from<Variant, T&&>
    NFuncDecl(T&& funcDecl) : v{std::forward<T>(funcDecl)} {}

    // RFuncDecl과 겹치는게 있으면 지우자
    NSYMBOL_API NDecl* GetNDecl();
    NSYMBOL_API RFuncDecl GetRFuncDecl();
    NSYMBOL_API bool IsSeqFunc();
    NSYMBOL_API NFuncDeclOuter GetNFuncDeclOuter();

    bool operator==(const NFuncDecl& other) const { return v == other.v; }

    template<typename... TArgs>
    auto Visit(TArgs&&... args) { return std::visit(std::forward<TArgs>(args)..., v); }

    template<typename T>
    T* GetIf() { return std::get_if<T>(&v); }
};

}