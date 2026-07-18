#pragma once
#include <vector>
#include <variant>

namespace Citron {

class RTraitDecl;
class RTraitFuncDecl;
class RTypeArguments;

// ImplementedTraitFunc
struct NImplTraitFunc
{
    RTraitFuncDecl* traitFuncDecl;
};

class NImplTraitMember
{
    using Variant = std::variant<NImplTraitFunc>;
    Variant v;

public:
    template<typename T> requires (!std::same_as<T, NImplTraitMember>) && std::constructible_from<Variant, T&&>
    NImplTraitMember(T&& value) : v{std::forward<T>(value)} {}
};

struct NImplTrait
{
    // 뭘 구현했는가
    RTraitDecl* trait;
    RTypeArguments* traitTypeArgs;

    std::vector<NImplTraitMember> members; // trait 선언 순서를 따른다
};

struct NStructInfo
{
    std::vector<NImplTrait> implTraits; // struct가 구현한 trait들
};

} // namespace Citron
