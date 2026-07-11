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
class RTypeParamDecl;

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
class RLambdaVarDecl;

// RDeclSpaceResolvedResult
struct RDeclRes_Namespace { RNamespaceDecl* decl; };
struct RDeclRes_GlobalFuncs 
{ 
    std::vector<TDeclWithOuterTypeArgs<RGlobalFuncDecl>> items;

    RSYMBOL_API RDeclRes_GlobalFuncs(std::vector<TDeclWithOuterTypeArgs<RGlobalFuncDecl>>&& items);
    RSYMBOL_API RDeclRes_GlobalFuncs(const RDeclRes_GlobalFuncs&);
    RSYMBOL_API ~RDeclRes_GlobalFuncs();
};
struct RDeclRes_Class { RTypeArguments* outerTypeArgs; RClassDecl* decl; };
struct RDeclRes_ClassFuncs 
{
    std::vector<TDeclWithOuterTypeArgs<RClassFuncDecl>> items;

    RSYMBOL_API RDeclRes_ClassFuncs(std::vector<TDeclWithOuterTypeArgs<RClassFuncDecl>>&& items);
    RSYMBOL_API RDeclRes_ClassFuncs(const RDeclRes_ClassFuncs&);
    RSYMBOL_API ~RDeclRes_ClassFuncs();
};

struct RDeclRes_ClassVar { RClassVarDecl* decl; RTypeArguments* typeArgs; };
struct RDeclRes_Struct { RTypeArguments* outerTypeArgs; RStructDecl* decl; };

struct RDeclRes_StructFuncs
{
    std::vector<TDeclWithOuterTypeArgs<RStructFuncDecl>> items;

    RSYMBOL_API RDeclRes_StructFuncs(std::vector<TDeclWithOuterTypeArgs<RStructFuncDecl>>&& items);
    RSYMBOL_API RDeclRes_StructFuncs(const RDeclRes_StructFuncs&);
    RSYMBOL_API ~RDeclRes_StructFuncs();
};

struct RDeclRes_StructVar { RStructVarDecl* decl; RTypeArguments* typeArgs; };
struct RDeclRes_Enum { RTypeArguments* outerTypeArgs; REnumDecl* decl; };
struct RDeclRes_EnumElem { RTypeArguments* outerTypeArgs; REnumElemDecl* decl; };
struct RDeclRes_EnumElemVar { RTypeArguments* outerTypeArgs; REnumElemVarDecl* decl; };
struct RDeclRes_LambdaVar { RTypeArguments* outerTypeArgs; RLambdaVarDecl* decl; };
struct RDeclRes_TupleVar {}; // 어떻게 쓰일지 몰라서, 실제로 만들때 채워넣는다
struct RDeclRes_TypeVar { RTypeParamDecl* decl; };
struct RDeclRes_FuncParam { RFuncParameter funcParam; };

class RDeclRes
{
    using Variant = std::variant<
        struct RDeclRes_Namespace,
        struct RDeclRes_GlobalFuncs,
        struct RDeclRes_Class,
        struct RDeclRes_ClassFuncs,
        struct RDeclRes_ClassVar,
        struct RDeclRes_Struct,
        struct RDeclRes_StructFuncs,
        struct RDeclRes_StructVar,
        struct RDeclRes_Enum,
        struct RDeclRes_EnumElem,
        struct RDeclRes_EnumElemVar,
        struct RDeclRes_LambdaVar,
        struct RDeclRes_TupleVar,
        struct RDeclRes_TypeVar,
        struct RDeclRes_FuncParam
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



} // namespace Citron

