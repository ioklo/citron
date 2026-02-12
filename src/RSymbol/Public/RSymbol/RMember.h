#pragma once

#include "RSymbolConfig.h"

#include <vector>
#include <variant>
#include <string>
#include <memory>

#include "RNames.h"

namespace Citron {

class RTypeArguments;
class RType;
class RTypeParamDecl;

template<typename TDecl>
struct DeclWithOuterTypeArgs;

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

using RMember = std::variant<
    class RMember_Namespace,
    class RMember_GlobalFuncs,
    class RMember_Class,
    class RMember_ClassFuncs,
    class RMember_ClassVar,
    class RMember_Struct,
    class RMember_StructFuncs,
    class RMember_StructVar,
    class RMember_Enum,
    class RMember_EnumElem,
    class RMember_EnumElemVar,
    class RMember_LambdaVar,
    class RMember_TupleVar,
    class RMember_TypeVar,
    class RMember_LocalVar,
    class RMember_LocalRef,
    class RMember_NeedCapture, // 람다에서 캡쳐가 필요할때
    class RMember_ThisVar
>;

class RMember_Namespace
{
public:
    RNamespaceDecl* decl;

public:
    RSYMBOL_API RMember_Namespace(RNamespaceDecl* decl);
};

class RMember_GlobalFuncs
{
public:
    std::vector<DeclWithOuterTypeArgs<RGlobalFuncDecl>> items;

public:
    RSYMBOL_API RMember_GlobalFuncs(std::vector<DeclWithOuterTypeArgs<RGlobalFuncDecl>>&& items);
    RSYMBOL_API RMember_GlobalFuncs(const RMember_GlobalFuncs&);
    RSYMBOL_API ~RMember_GlobalFuncs();
};

class RMember_Class
{
public:
    RTypeArguments* outerTypeArgs;
    RClassDecl* decl;
public:
    RSYMBOL_API RMember_Class(RTypeArguments* outerTypeArgs, RClassDecl* decl);
};

class RMember_ClassFuncs
{
public:
    std::vector<DeclWithOuterTypeArgs<RClassFuncDecl>> items;
public:
    RSYMBOL_API RMember_ClassFuncs(std::vector<DeclWithOuterTypeArgs<RClassFuncDecl>>&& items);
    RSYMBOL_API RMember_ClassFuncs(const RMember_ClassFuncs&);
    RSYMBOL_API ~RMember_ClassFuncs();
};

class RMember_ClassVar
{
public:
    RClassVarDecl* decl;
    RTypeArguments* typeArgs;

public:
    RSYMBOL_API RMember_ClassVar(RClassVarDecl* decl, RTypeArguments* typeArgs);
};

class RMember_Struct
{
public:
    RTypeArguments* outerTypeArgs;
    RStructDecl* decl;

public:
    RSYMBOL_API RMember_Struct(RTypeArguments* outerTypeArgs, RStructDecl* decl);
};

class RMember_StructFuncs
{
public:
    std::vector<DeclWithOuterTypeArgs<RStructFuncDecl>> items;

public:
    RSYMBOL_API RMember_StructFuncs(std::vector<DeclWithOuterTypeArgs<RStructFuncDecl>>&& items);
    RSYMBOL_API RMember_StructFuncs(const RMember_StructFuncs&);
    RSYMBOL_API ~RMember_StructFuncs();
};

class RMember_StructVar
{
public:
    RStructVarDecl* decl;
    RTypeArguments* typeArgs;

public:
    RSYMBOL_API RMember_StructVar(RStructVarDecl* decl, RTypeArguments* typeArgs);
};

class RMember_Enum
{
public:
    RTypeArguments* outerTypeArgs;
    REnumDecl* decl;

public:
    RSYMBOL_API RMember_Enum(RTypeArguments* outerTypeArgs, REnumDecl* decl);
};

class RMember_EnumElem
{
public:
    RTypeArguments* outerTypeArgs;
    REnumElemDecl* decl;

public:
    RSYMBOL_API RMember_EnumElem(RTypeArguments* outerTypeArgs, REnumElemDecl* decl);
};

class RMember_EnumElemVar
{
public:
    RTypeArguments* outerTypeArgs;
    REnumElemVarDecl* decl;

public:
    RSYMBOL_API RMember_EnumElemVar(RTypeArguments* outerTypeArgs, REnumElemVarDecl* decl);
};

class RMember_LambdaVar
{
public:
    RTypeArguments* outerTypeArgs;
    RLambdaVarDecl* decl;

public:
    RSYMBOL_API RMember_LambdaVar(RTypeArguments* outerTypeArgs, RLambdaVarDecl* decl);
};

// 어떻게 쓰일지 몰라서, 실제로 만들때 채워넣는다
class RMember_TupleVar
{
public:
    RSYMBOL_API RMember_TupleVar();
};

class RMember_TypeVar
{
public:
    RTypeParamDecl* decl;

public:
    RSYMBOL_API RMember_TypeVar(RTypeParamDecl* decl);
};

class RMember_LocalVar
{
public:
    RType* type;
    RName name;
public:
    RSYMBOL_API RMember_LocalVar(RType* type, const RName& name);
};

class RMember_LocalRef
{
public:
    RType* type;
    RName name;
public:
    RSYMBOL_API RMember_LocalRef(RType* type, const RName& name);
};

class RMember_NeedCapture
{
public:
    RName name;
    std::unique_ptr<RMember> member;
};

class RMember_ThisVar
{
public:
    RType* type;

public:
    RSYMBOL_API RMember_ThisVar(RType* type);
};

RSYMBOL_API std::vector<DeclWithOuterTypeArgs<RFuncDecl>> GetFuncDeclWithOuterTypeArgs(RMember& member);

} // namespace Citron

