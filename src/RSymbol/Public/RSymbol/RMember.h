#pragma once
#include "RSymbolConfig.h"
#include <variant>
#include <vector>

namespace Citron {

class RNamespaceDecl;
class RGlobalFuncDecl;
class RClassDecl;
class RClassFuncDecl;
class RClassVarDecl;
class RStructDecl;
class RStructFuncDecl;
class RStructVarDecl;
class REnumDecl;
class REnumElemDecl;
class REnumElemVarDecl;
class RLambdaDecl;
class RLambdaVarDecl;
class RInterfaceDecl;
class RTraitDecl;
class RTraitFuncDecl;
class RTypeDecl;
class RImplTraitDecl;
class RImplTraitFuncDecl;

struct RMember_Namespace { RNamespaceDecl* decl; };
struct RMember_GlobalFuncs { std::vector<RGlobalFuncDecl*> items; };
struct RMember_Class { RClassDecl* decl; };
struct RMember_ClassFuncs { std::vector<RClassFuncDecl*> items; };
struct RMember_ClassVar { RClassVarDecl* decl; };
struct RMember_Struct { RStructDecl* decl; };
struct RMember_StructFuncs { std::vector<RStructFuncDecl*> items; };
struct RMember_StructVar { RStructVarDecl* decl; };
struct RMember_Enum { REnumDecl* decl; };
struct RMember_EnumElem { REnumElemDecl* decl; };
struct RMember_EnumElemVar { REnumElemVarDecl* decl; };
struct RMember_Interface { RInterfaceDecl* decl; };
struct RMember_Lambda { RLambdaDecl* decl; };
struct RMember_LambdaVar { RLambdaVarDecl* decl; };
struct RMember_TupleVar {}; // 어떻게 쓰일지 몰라서, 실제로 만들때 채워넣는다
struct RMember_Trait { RTraitDecl* decl; };
struct RMember_TraitFuncs { std::vector<RTraitFuncDecl*> items; };
struct RMember_ImplTrait { RImplTraitDecl* decl; };
struct RMember_ImplTraitFuncs { std::vector<RImplTraitFuncDecl*> items; };

class RMember
{
    using Variant = std::variant<
        RMember_Namespace,
        RMember_GlobalFuncs,
        RMember_Class,
        RMember_ClassFuncs,
        RMember_ClassVar,
        RMember_Struct,
        RMember_StructFuncs,
        RMember_StructVar,
        RMember_Enum,
        RMember_EnumElem,
        RMember_EnumElemVar,
        RMember_Interface,
        RMember_Lambda,
        RMember_LambdaVar,
        RMember_TupleVar,
        RMember_Trait,
        RMember_TraitFuncs,
        RMember_ImplTrait,
        RMember_ImplTraitFuncs
    >;

    Variant v;

public:
    template<typename T> requires (!std::same_as<std::remove_cvref_t<T>, RMember>) && std::constructible_from<Variant, T&&>
    RMember(T&& t) : v{std::forward<T>(t)} {}

    auto Visit(auto&&... args) { return std::visit(std::forward<decltype(args)>(args)..., v); }
};

RSYMBOL_API RMember ToRMember(RTypeDecl* typeDecl);


} // namespace Citron
