#pragma once
#include "RSymbolConfig.h"

#include <vector>
#include <variant>
#include <string>
#include <memory>
#include "RNames.h"
#include "RFuncParameter.h"
#include "RFuncDecl.h"
#include "ROuterAppliedDecl.h"
#include "RAppliedDecl.h"

namespace Citron {

class RTypeArguments;
class RType;
class RTypeParam;

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
class RMember;

// RDeclSpaceResolvedResult
struct RDeclRes_Namespace { RNamespaceDecl* decl; };
struct RDeclRes_GlobalFuncs : ROuterAppliedFuncDeclGroup<RGlobalFuncDecl> { };
struct RDeclRes_Class : ROuterAppliedDecl<RClassDecl> { };
struct RDeclRes_ClassFuncs : ROuterAppliedFuncDeclGroup<RClassFuncDecl> { };
struct RDeclRes_ClassVar : RAppliedDecl<RClassVarDecl> { };
struct RDeclRes_Struct : ROuterAppliedDecl<RStructDecl> { };
struct RDeclRes_StructFuncs : ROuterAppliedFuncDeclGroup<RStructFuncDecl> { };
struct RDeclRes_StructVar : RAppliedDecl<RStructVarDecl> { };
struct RDeclRes_Enum : ROuterAppliedDecl<REnumDecl> { };
struct RDeclRes_EnumElem : ROuterAppliedDecl<REnumElemDecl> { };
struct RDeclRes_EnumElemVar : ROuterAppliedDecl<REnumElemVarDecl> { };
struct RDeclRes_Lambda : ROuterAppliedDecl<RLambdaDecl> { };
struct RDeclRes_LambdaVar : ROuterAppliedDecl<RLambdaVarDecl> { };
struct RDeclRes_Interface : ROuterAppliedDecl<RInterfaceDecl> { };
struct RDeclRes_TupleVar {}; // 어떻게 쓰일지 몰라서, 실제로 만들때 채워넣는다
struct RDeclRes_TypeVar { RTypeParam* decl; };
struct RDeclRes_Trait : ROuterAppliedDecl<RTraitDecl> { };
struct RDeclRes_TraitFuncs : ROuterAppliedFuncDeclGroup<RTraitFuncDecl> { };

class RDeclRes
{
    using Variant = std::variant<
        RDeclRes_Namespace,
        RDeclRes_GlobalFuncs,
        RDeclRes_Class,
        RDeclRes_ClassFuncs,
        RDeclRes_ClassVar,
        RDeclRes_Struct,
        RDeclRes_StructFuncs,
        RDeclRes_StructVar,
        RDeclRes_Enum,
        RDeclRes_EnumElem,
        RDeclRes_EnumElemVar,
        RDeclRes_Lambda,
        RDeclRes_LambdaVar,
        RDeclRes_Interface,
        RDeclRes_TupleVar,
        RDeclRes_TypeVar,
        RDeclRes_Trait,
        RDeclRes_TraitFuncs
    >;

    Variant v;

public:
    template<typename T> requires (!std::same_as<std::remove_cvref_t<T>, RDeclRes>) && std::constructible_from<Variant, T&&>
    RDeclRes(T&& res) : v{std::forward<T>(res)} {}

    template<typename... TArgs>
    auto Visit(TArgs&&... args) { return std::visit(std::forward<TArgs>(args)..., v); }

    template<typename T>
    T* GetIf() { return std::get_if<T>(&v); }
};

RSYMBOL_API RDeclRes ToRDeclRes(RTypeArguments* outerTypeArgs, RMember member);

} // namespace Citron

