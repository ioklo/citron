#pragma once
#include "RSymbolConfig.h"

#include <vector>
#include <variant>
#include <string>
#include <memory>
#include "RNames.h"
#include "RFuncParameter.h"
#include "RFuncDecl.h"
#include "DeclWithOuterTypeArgs.h"

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
struct RDeclRes_GlobalFuncs { RTypeArguments* outerTypeArgs; std::vector<RGlobalFuncDecl*> items; };
struct RDeclRes_Class { RTypeArguments* outerTypeArgs; RClassDecl* decl; };
struct RDeclRes_ClassFuncs { RTypeArguments* outerTypeArgs; std::vector<RClassFuncDecl*> items; };
struct RDeclRes_ClassVar { RClassVarDecl* decl; RTypeArguments* typeArgs; };
struct RDeclRes_Struct { RTypeArguments* outerTypeArgs; RStructDecl* decl; };
struct RDeclRes_StructFuncs { RTypeArguments* outerTypeArgs; std::vector<RStructFuncDecl*> items; };
struct RDeclRes_StructVar { RStructVarDecl* decl; RTypeArguments* typeArgs; };
struct RDeclRes_Enum { RTypeArguments* outerTypeArgs; REnumDecl* decl; };
struct RDeclRes_EnumElem { RTypeArguments* outerTypeArgs; REnumElemDecl* decl; };
struct RDeclRes_EnumElemVar { RTypeArguments* outerTypeArgs; REnumElemVarDecl* decl; };
struct RDeclRes_Lambda { RTypeArguments* outerTypeArgs; RLambdaDecl* decl; };
struct RDeclRes_LambdaVar { RTypeArguments* outerTypeArgs; RLambdaVarDecl* decl; };
struct RDeclRes_Interface { RTypeArguments* outerTypeArgs; RInterfaceDecl* decl; };
struct RDeclRes_TupleVar {}; // 어떻게 쓰일지 몰라서, 실제로 만들때 채워넣는다
struct RDeclRes_TypeVar { RTypeParam* decl; };
struct RDeclRes_FuncParam { RFuncParameter funcParam; };
struct RDeclRes_Trait { RTypeArguments* outerTypeArgs; RTraitDecl* decl; };
struct RDeclRes_TraitFuncs { RTypeArguments* outerTypeArgs; std::vector<RTraitFuncDecl*> items; };

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
        RDeclRes_FuncParam,
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

