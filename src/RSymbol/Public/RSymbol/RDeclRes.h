#pragma once

#include "RSymbolConfig.h"

#include <vector>
#include <variant>
#include <string>
#include <memory>
#include "RNames.h"
#include "RFuncParameter.h"
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
class RFuncDecl;

// RDeclSpaceResolvedResult
using RDeclRes = std::variant<
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

struct RDeclRes_Namespace { RNamespaceDecl* decl; };
struct RDeclRes_GlobalFuncs 
{ 
    std::vector<DeclWithOuterTypeArgs<RGlobalFuncDecl>> items;

    RSYMBOL_API RDeclRes_GlobalFuncs(std::vector<DeclWithOuterTypeArgs<RGlobalFuncDecl>>&& items);
    RSYMBOL_API RDeclRes_GlobalFuncs(const RDeclRes_GlobalFuncs&);
    RSYMBOL_API ~RDeclRes_GlobalFuncs();
};
struct RDeclRes_Class { RTypeArguments* outerTypeArgs; RClassDecl* decl; };
struct RDeclRes_ClassFuncs 
{
    std::vector<DeclWithOuterTypeArgs<RClassFuncDecl>> items;

    RSYMBOL_API RDeclRes_ClassFuncs(std::vector<DeclWithOuterTypeArgs<RClassFuncDecl>>&& items);
    RSYMBOL_API RDeclRes_ClassFuncs(const RDeclRes_ClassFuncs&);
    RSYMBOL_API ~RDeclRes_ClassFuncs();
};

struct RDeclRes_ClassVar { RClassVarDecl* decl; RTypeArguments* typeArgs; };
struct RDeclRes_Struct { RTypeArguments* outerTypeArgs; RStructDecl* decl; };

struct RDeclRes_StructFuncs
{
    std::vector<DeclWithOuterTypeArgs<RStructFuncDecl>> items;

    RSYMBOL_API RDeclRes_StructFuncs(std::vector<DeclWithOuterTypeArgs<RStructFuncDecl>>&& items);
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

RSYMBOL_API std::vector<DeclWithOuterTypeArgs<RFuncDecl>> GetFuncDeclWithOuterTypeArgs(RDeclRes& member);

} // namespace Citron

