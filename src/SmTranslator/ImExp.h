#pragma once

#include <memory>
#include <string>
#include "RSymbol/RGlobalFuncDecl.h"
#include "RSymbol/RClassFuncDecl.h"
#include "RSymbol/RStructFuncDecl.h"
#include "MIR/MRead.h"
#include "ReExp.h"
#include "SmPartiallyAppliedFuncDeclGroup.h"
#include "RSymbol/RNamespaceGroup.h"

namespace Citron {

class RNamespace;
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

struct ImExp_Namespaces : ImExp
{
    RNamespaceGroup namespaces; // namespace를 뭘로 저장하고 있어야 하나

public:
    ImExp_Namespaces(RNamespaceGroup&& namespaces)
        : namespaces{std::move(namespaces)}
    { }
    void Accept(ImExpVisitor& visitor) override;
};

// 
struct ImExp_GlobalFuncs : ImExp
{
    SmPartiallyAppliedFuncDeclGroup<RGlobalFuncDecl> funcDeclGroup;

public:
    ImExp_GlobalFuncs(ROuterAppliedFuncDeclGroup<RGlobalFuncDecl>& funcDeclGroup, RTypeArguments* memberTypeArgs)
        : funcDeclGroup{funcDeclGroup.outerTypeArgs, funcDeclGroup.decls, memberTypeArgs}
    {
    }

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
    RAppliedDecl<RClassDecl> appliedDecl;

    ImExp_Class(RAppliedDecl<RClassDecl>&& appliedDecl)
        : appliedDecl{std::move(appliedDecl)}
    { }
    void Accept(ImExpVisitor& visitor) override;
};

struct ImExp_ClassFuncs : ImExp
{   
    SmPartiallyAppliedFuncDeclGroup<RClassFuncDecl> funcDeclGroup;
    ImExpInstanceKind instanceKind;

public:
    ImExp_ClassFuncs(ROuterAppliedFuncDeclGroup<RClassFuncDecl>& funcDeclGroup, RTypeArguments* memberTypeArgs, ImExpInstanceKind&& instanceKind)
        : funcDeclGroup{funcDeclGroup.outerTypeArgs, funcDeclGroup.decls, memberTypeArgs}, instanceKind{std::move(instanceKind)}
    {
    }

    void Accept(ImExpVisitor& visitor) override;
};

struct ImExp_Struct : ImExp
{
    RAppliedDecl<RStructDecl> appliedDecl;

    ImExp_Struct(RAppliedDecl<RStructDecl>&& appliedDecl)
        : appliedDecl{std::move(appliedDecl)}
    { }

    void Accept(ImExpVisitor& visitor) override;
};

struct ImExp_StructFuncs : ImExp
{
    SmPartiallyAppliedFuncDeclGroup<RStructFuncDecl> funcDeclGroup;
    ImExpInstanceKind instanceKind;

    ImExp_StructFuncs(ROuterAppliedFuncDeclGroup<RStructFuncDecl>& funcDeclGroup, RTypeArguments* memberTypeArgs, ImExpInstanceKind&& instanceKind)
        : funcDeclGroup{funcDeclGroup.outerTypeArgs, funcDeclGroup.decls, memberTypeArgs}, instanceKind{std::move(instanceKind)}
    { }

    void Accept(ImExpVisitor& visitor) override;
};

struct ImExp_Enum : ImExp
{
    RAppliedDecl<REnumDecl> appliedDecl;

    ImExp_Enum(RAppliedDecl<REnumDecl>&& appliedDecl)
        : appliedDecl{std::move(appliedDecl)}
    { }

    void Accept(ImExpVisitor& visitor) override;
};

struct ImExp_EnumElem : ImExp
{
    RAppliedDecl<REnumElemDecl> appliedDecl;

    ImExp_EnumElem(RAppliedDecl<REnumElemDecl>&& appliedDecl)
        : appliedDecl{std::move(appliedDecl)}
    { }

    void Accept(ImExpVisitor& visitor) override;
};

struct ImExp_ClassVar : ImExp
{
    RAppliedDecl<RClassVarDecl> appliedDecl;
    ImExpInstanceKind instanceKind;

    ImExp_ClassVar(RAppliedDecl<RClassVarDecl>&& appliedDecl, ImExpInstanceKind&& instanceKind)
        : appliedDecl{std::move(appliedDecl)}, instanceKind{std::move(instanceKind)}
    { }

    void Accept(ImExpVisitor& visitor) override;
};

struct ImExp_StructVar : ImExp
{
    RAppliedDecl<RStructVarDecl> appliedDecl;
    ImExpInstanceKind instanceKind;

    ImExp_StructVar(RAppliedDecl<RStructVarDecl>&& appliedDecl, ImExpInstanceKind&& instanceKind)
        : appliedDecl{std::move(appliedDecl)}, instanceKind{std::move(instanceKind)}
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