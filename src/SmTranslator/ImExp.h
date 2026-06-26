#pragma once

#include <memory>
#include <string>
#include "RSymbol/RGlobalFuncDecl.h"
#include "RSymbol/RClassFuncDecl.h"
#include "RSymbol/RStructFuncDecl.h"
#include "MIR/MRead.h"
#include "ReExp.h"
#include "FuncsWithPartialTypeArgsComponent.h"

namespace Citron {

class RNamespaceDecl;
class RType_TypeVar;
class RClassDecl;
class RClassFuncDecl;
class RClassVarDecl;
class RStructDecl;
class RStructFuncDecl;
class RStructVarDecl;
class REnumDecl;
class REnumElemDecl;
class REnumElemVarDecl;
class RType;
struct MExp;
class NLambdaVarDecl;
struct ImExpVisitor;

struct ImExpInstanceKind_ExplicitStatic { }; // C.F
struct ImExpInstanceKind_ExplicitInstance { MLoc* mInstLoc; }; // x.F
struct ImExpInstanceKind_Implicit {}; // F
using ImExpInstanceKind = std::variant<ImExpInstanceKind_ExplicitStatic, ImExpInstanceKind_ExplicitInstance, ImExpInstanceKind_Implicit>;

struct ImExp
{
public:
    virtual ~ImExp() { }
    virtual void Accept(ImExpVisitor& visitor) = 0;
};

struct ImExp_Namespace : ImExp
{
    RNamespaceDecl* _namespace; // namespace를 뭘로 저장하고 있어야 하나

public:
    ImExp_Namespace(RNamespaceDecl* _namespace)
        : _namespace{_namespace}
    { }
    void Accept(ImExpVisitor& visitor) override;
};

// 
struct ImExp_GlobalFuncs
    : ImExp
    , private FuncsWithPartialTypeArgsComponent<RGlobalFuncDecl>
{
    using FuncComp = FuncsWithPartialTypeArgsComponent<RGlobalFuncDecl>;

    using FuncComp::items;
    using FuncComp::memberTypeArgs;

    ImExp_GlobalFuncs(const std::vector<TDeclWithOuterTypeArgs<RGlobalFuncDecl>>& items, RTypeArguments* memberTypeArgs)
        : FuncsWithPartialTypeArgsComponent<RGlobalFuncDecl>{items, memberTypeArgs}
    { }

    using FuncComp::GetCount;
    using FuncComp::GetDecl;
    using FuncComp::GetOuterTypeArgs;
    using FuncComp::GetMemberTypeArgs;

    void Accept(ImExpVisitor& visitor) override;
};

struct ImExp_TypeVar : ImExp
{
    RType_TypeVar* type;
    ImExp_TypeVar(RType_TypeVar* type)
        : type{type}
    { }
    void Accept(ImExpVisitor& visitor) override;
};

struct ImExp_Class : ImExp
{
    RClassDecl* classDecl;
    RTypeArguments* typeArgs;

    ImExp_Class(RClassDecl* classDecl, RTypeArguments* typeArgs)
        : classDecl{classDecl}, typeArgs{typeArgs}
    { }
    void Accept(ImExpVisitor& visitor) override;
};

struct ImExp_ClassFuncs 
    : ImExp
    , private FuncsWithPartialTypeArgsComponent<RClassFuncDecl>
{   
    using FuncsWithPartialTypeArgsComponent::items;
    using FuncsWithPartialTypeArgsComponent::memberTypeArgs;
    ImExpInstanceKind instanceKind;

    using FuncComp = FuncsWithPartialTypeArgsComponent<RClassFuncDecl>;

    ImExp_ClassFuncs(const std::vector<TDeclWithOuterTypeArgs<RClassFuncDecl>>& items, RTypeArguments* memberTypeArgs, ImExpInstanceKind&& instanceKind)
        : FuncsWithPartialTypeArgsComponent<RClassFuncDecl>{items, memberTypeArgs}, instanceKind{std::move(instanceKind)}
    { }

    using FuncComp::GetCount;
    using FuncComp::GetDecl;
    using FuncComp::GetOuterTypeArgs;
    using FuncComp::GetMemberTypeArgs;

    void Accept(ImExpVisitor& visitor) override;
};

struct ImExp_Struct : ImExp
{
    RStructDecl* structDecl;
    RTypeArguments* typeArgs;

    ImExp_Struct(RStructDecl* structDecl, RTypeArguments* typeArgs)
        : structDecl{structDecl}, typeArgs{typeArgs}
    { }

    void Accept(ImExpVisitor& visitor) override;
};

struct ImExp_StructFuncs 
    : ImExp
    , private FuncsWithPartialTypeArgsComponent<RStructFuncDecl>
{
    using FuncComp = FuncsWithPartialTypeArgsComponent<RStructFuncDecl>;

    using FuncComp::items;
    using FuncComp::memberTypeArgs;
    ImExpInstanceKind instanceKind;

    ImExp_StructFuncs(const std::vector<TDeclWithOuterTypeArgs<RStructFuncDecl>>& items, RTypeArguments* memberTypeArgs, ImExpInstanceKind&& instanceKind)
        : FuncsWithPartialTypeArgsComponent<RStructFuncDecl>{items, memberTypeArgs}, instanceKind{std::move(instanceKind)}
    { }

    using FuncComp::GetCount;
    using FuncComp::GetDecl;
    using FuncComp::GetOuterTypeArgs;
    using FuncComp::GetMemberTypeArgs;

    void Accept(ImExpVisitor& visitor) override;
};

struct ImExp_Enum : ImExp
{
    REnumDecl* decl;
    RTypeArguments* typeArgs;

    ImExp_Enum(REnumDecl* decl, RTypeArguments* typeArgs)
        : decl{decl}, typeArgs{typeArgs}
    { }

    void Accept(ImExpVisitor& visitor) override;
};

struct ImExp_EnumElem : ImExp
{
    REnumElemDecl* decl;
    RTypeArguments* typeArgs;

    ImExp_EnumElem(REnumElemDecl* decl, RTypeArguments* typeArgs)
        : decl{decl}, typeArgs{typeArgs}
    { }

    void Accept(ImExpVisitor& visitor) override;
};

struct ImExp_ClassVar : ImExp
{
    RClassVarDecl* decl;
    RTypeArguments* typeArgs;
    ImExpInstanceKind instanceKind;

    ImExp_ClassVar(RClassVarDecl* decl, RTypeArguments* typeArgs, ImExpInstanceKind&& instanceKind)
        : decl{decl}, typeArgs{typeArgs}, instanceKind{std::move(instanceKind)}
    { }

    void Accept(ImExpVisitor& visitor) override;
};

struct ImExp_StructVar : ImExp
{
    RStructVarDecl* decl;
    RTypeArguments* typeArgs;
    ImExpInstanceKind instanceKind;

    ImExp_StructVar(RStructVarDecl* decl, RTypeArguments* typeArgs, ImExpInstanceKind&& instanceKind)
        : decl{decl}, typeArgs{typeArgs}, instanceKind{std::move(instanceKind)}
    { }
    void Accept(ImExpVisitor& visitor) override;
};

struct ImExp_ReExp : ImExp
{
    ReExp reExp;

    ImExp_ReExp(ReExp&& reExp)
        : reExp{std::move(reExp)}
    { }
    void Accept(ImExpVisitor& visitor) override;
};

} // namespace Citron

#include "ImExpVisitor.g.h"