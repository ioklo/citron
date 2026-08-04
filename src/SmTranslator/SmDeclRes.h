#pragma once

#include <vector>
#include <variant>
#include <string>
#include <memory>
#include "RSymbol/RNames.h"
#include "RSymbol/ROuterAppliedDecl.h"
#include "RSymbol/RAppliedDecl.h"
#include "RSymbol/RNamespaceGroup.h"

namespace Citron {

class RTypeArguments;
class RType;
class RTypeParam;

class RNamespace;
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
class RMember;

struct SmDeclRes_Namespaces { RNamespaceGroup namespaces; };
struct SmDeclRes_GlobalFuncs { ROuterAppliedFuncDeclGroup<RGlobalFuncDecl> outerAppliedFuncDecls; };
struct SmDeclRes_Class { ROuterAppliedDecl<RClassDecl> outerAppliedDecl; };
struct SmDeclRes_ClassFuncs { ROuterAppliedFuncDeclGroup<RClassFuncDecl> outerAppliedFuncDecls; };
struct SmDeclRes_ClassVar { RAppliedDecl<RClassVarDecl> appliedDecl; };
struct SmDeclRes_Struct { ROuterAppliedDecl<RStructDecl> outerAppliedDecl; };
struct SmDeclRes_StructFuncs { ROuterAppliedFuncDeclGroup<RStructFuncDecl> outerAppliedFuncDecls; };
struct SmDeclRes_StructVar { RAppliedDecl<RStructVarDecl> appliedDecl; };
struct SmDeclRes_Enum { ROuterAppliedDecl<REnumDecl> outerAppliedDecl; };
struct SmDeclRes_EnumElem { RAppliedDecl<REnumElemDecl> appliedDecl; };
struct SmDeclRes_EnumElemVar { RAppliedDecl<REnumElemVarDecl> appliedDecl; };
struct SmDeclRes_Lambda { RAppliedDecl<RLambdaDecl> appliedDecl; };
struct SmDeclRes_LambdaVar { RAppliedDecl<RLambdaVarDecl> appliedDecl; };
struct SmDeclRes_Interface { ROuterAppliedDecl<RInterfaceDecl> outerAppliedDecl; };
struct SmDeclRes_TupleVar {}; // 어떻게 쓰일지 몰라서, 실제로 만들때 채워넣는다
struct SmDeclRes_TypeVar { RTypeParam* decl; };
struct SmDeclRes_Trait { ROuterAppliedDecl<RTraitDecl> outerAppliedDecl; };
struct SmDeclRes_TraitFuncs { ROuterAppliedFuncDeclGroup<RTraitFuncDecl> outerAppliedFuncDecls; };

// Declspace Resolution Result
class SmDeclRes
{
    using Variant = std::variant<
        SmDeclRes_Namespaces,
        SmDeclRes_GlobalFuncs,
        SmDeclRes_Class,
        SmDeclRes_ClassFuncs,
        SmDeclRes_ClassVar,
        SmDeclRes_Struct,
        SmDeclRes_StructFuncs,
        SmDeclRes_StructVar,
        SmDeclRes_Enum,
        SmDeclRes_EnumElem,
        SmDeclRes_EnumElemVar,
        SmDeclRes_Lambda,
        SmDeclRes_LambdaVar,
        SmDeclRes_Interface,
        SmDeclRes_TupleVar,
        SmDeclRes_TypeVar,
        SmDeclRes_Trait,
        SmDeclRes_TraitFuncs
    >;

    Variant v;

public:
    template<typename T> requires (!std::same_as<std::remove_cvref_t<T>, SmDeclRes>) && std::constructible_from<Variant, T&&>
    SmDeclRes(T&& res) : v{std::forward<T>(res)} {}

    template<typename... TArgs>
    auto Visit(TArgs&&... args) & { return std::visit(std::forward<TArgs>(args)..., v); }

    template<typename... TArgs>
    auto Visit(TArgs&&... args) && { return std::visit(std::forward<TArgs>(args)..., std::move(v)); }

    template<typename T>
    T* GetIf() { return std::get_if<T>(&v); }
};

SmDeclRes ToSmDeclRes(RTypeArguments* outerTypeArgs, RMember member);

} // namespace Citron