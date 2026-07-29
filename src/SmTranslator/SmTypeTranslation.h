#pragma once
#include <expected>
#include <span>
#include <memory>
#include <variant>
#include <optional>
#include "Infra/Ref.h"
#include "RSymbol/RAppliedDecl.h"

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;

class STypeExp;
class RType;
class RTypeArguments;
class RFactory;
class RTypeParam;
class RName;
class RTraitDecl;
class SmTypeRes;
class SmFuncContext;
class SmDeclContext;

struct SmTypeResolveScope_DeclHeader { SmDeclContext* outerDeclContext; std::span<RTypeParam*> typeParams; };
struct SmTypeResolveScope_FuncContext { SmFuncContext* funcContext; };
struct SmTypeResolveScope_DeclContext { SmDeclContext* declContext; };

class SmTypeResolveScope
{
    using Variant = std::variant<
        SmTypeResolveScope_DeclHeader,
        SmTypeResolveScope_FuncContext,
        SmTypeResolveScope_DeclContext>;

    Variant v;
public:
    template<typename T> requires (!std::same_as<std::remove_cvref_t<T>, SmTypeResolveScope>) && std::constructible_from<Variant, T&&>
    SmTypeResolveScope(T&& res) : v{std::forward<T>(res)} {}

    template<typename... TArgs>
    auto Visit(TArgs&&... args) & { return std::visit(std::forward<TArgs>(args)..., v); }

    template<typename... TArgs>
    auto Visit(TArgs&&... args) && { return std::visit(std::forward<TArgs>(args)..., std::move(v)); }

    std::optional<SmTypeRes> ResolveTypeIdentifier(InRef<RName> name, RFactory* rFactory);
};

std::expected<RType*, DiagPtr> TranslateSTypeExpToRType(STypeExp* sTypeExp, SmTypeResolveScope scope, RFactory* rFactory);
std::expected<RAppliedDecl<RTraitDecl>, DiagPtr> TranslateSTypeExpToRTrait(STypeExp* sTypeExp, SmTypeResolveScope scope, RFactory* rFactory);
std::expected<RType*, DiagPtr> MakeType(SmTypeRes& typeRes, std::span<STypeExp*> sMemberTypeArgs, SmTypeResolveScope scope, RFactory* rFactory);
std::expected<RTypeArguments*, DiagPtr> MakeRTypeArgs(std::span<STypeExp*> typeArgs, SmTypeResolveScope scope, RFactory* rFactory);


} // namespace Citron
