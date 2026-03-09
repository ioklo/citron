#pragma once

#include <memory>
#include <string>
#include "RSymbol/RGlobalFuncDecl.h"
#include "MIR/MRead.h"

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
    using FuncComp::partialTypeArgsExceptOuter;

    ImExp_GlobalFuncs(const std::vector<DeclWithOuterTypeArgs<RGlobalFuncDecl>>& items, RTypeArguments* partialTypeArgsExceptOuter)
        : FuncsWithPartialTypeArgsComponent<RGlobalFuncDecl>{items, partialTypeArgsExceptOuter}
    { }

    using FuncComp::GetCount;
    using FuncComp::GetDecl;
    using FuncComp::GetOuterTypeArgs;
    using FuncComp::GetPartialTypeArgsExceptOuter;

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
    // HasExplicitInstance: x.F 처럼 x가 명시적으로 있는 경우 true, F 처럼 this.F 나 C.F 를 암시적으로 나타낸 경우라면 false, C.F는 명시적이지만 인스턴스가 아니므로 false
    // ExplicitInstance: HasExplicitInstance가 true일때만 의미가 있다

    // C.F => HasExplicitInstance: true, null
    // x.F => HasExplicitInstance: true, "x"
    // F   => HasExplicitInstance: false, null
    using FuncsWithPartialTypeArgsComponent::items;
    using FuncsWithPartialTypeArgsComponent::partialTypeArgsExceptOuter;
    bool hasExplicitInstance;
    MLoc* explicitInstance;

    using FuncComp = FuncsWithPartialTypeArgsComponent<RClassFuncDecl>;

    ImExp_ClassFuncs(const std::vector<DeclWithOuterTypeArgs<RClassFuncDecl>>& items, RTypeArguments* partialTypeArgsExceptOuter, bool hasExplicitInstance, MLoc* explicitInstance)
        : FuncsWithPartialTypeArgsComponent<RClassFuncDecl>{items, partialTypeArgsExceptOuter}, hasExplicitInstance{hasExplicitInstance}, explicitInstance{explicitInstance}
    { }

    using FuncComp::GetCount;
    using FuncComp::GetDecl;
    using FuncComp::GetOuterTypeArgs;
    using FuncComp::GetPartialTypeArgsExceptOuter;

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
    using FuncComp::partialTypeArgsExceptOuter;

    bool hasExplicitInstance;
    MLoc* explicitInstance;

    ImExp_StructFuncs(const std::vector<DeclWithOuterTypeArgs<RStructFuncDecl>>& items, RTypeArguments* partialTypeArgsExceptOuter, bool hasExplicitInstance, MLoc* explicitInstance)
        : FuncsWithPartialTypeArgsComponent<RStructFuncDecl>{items, partialTypeArgsExceptOuter}, hasExplicitInstance{hasExplicitInstance}, explicitInstance{explicitInstance}
    { }

    using FuncComp::GetCount;
    using FuncComp::GetDecl;
    using FuncComp::GetOuterTypeArgs;
    using FuncComp::GetPartialTypeArgsExceptOuter;

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

// exp로 사용할 수 있는
struct ImExp_ThisVar : ImExp
{
    RType* type;

    ImExp_ThisVar(RType* type) : type{type}
    { }
    void Accept(ImExpVisitor& visitor) override;
};

struct ImExp_LocalVar : ImExp
{
    RType* type;
    RName name;

    ImExp_LocalVar(RType* type, const RName& name)
        : type{type}, name{name}
    { }
    void Accept(ImExpVisitor& visitor) override;
};

struct ImExp_LocalRef : ImExp
{
    RType* type;
    RName name;

    ImExp_LocalRef(RType* type, const RName& name)
        : type{type}, name{name}
    { }
    void Accept(ImExpVisitor& visitor) override;
};

struct ImExp_LambdaVar : ImExp
{
    NLambdaVarDecl* decl;
    RTypeArguments* typeArgs;

    ImExp_LambdaVar(NLambdaVarDecl* decl, RTypeArguments* typeArgs)
        : decl{decl}, typeArgs{typeArgs}
    { }
    void Accept(ImExpVisitor& visitor) override;
};

struct ImExp_ClassVar : ImExp
{
    RClassVarDecl* decl;
    RTypeArguments* typeArgs;
    
    bool hasExplicitInstance;
    MLoc* explicitInstance;

    ImExp_ClassVar(RClassVarDecl* decl, RTypeArguments* typeArgs, bool hasExplicitInstance, MLoc* explicitInstance)
        : decl{decl}, typeArgs{typeArgs}, hasExplicitInstance{hasExplicitInstance}, explicitInstance{explicitInstance}
    { }

    void Accept(ImExpVisitor& visitor) override;
};

struct ImExp_StructVar : ImExp
{
    RStructVarDecl* decl;
    RTypeArguments* typeArgs;
    
    bool hasExplicitInstance;
    MLoc* explicitInstance;

    ImExp_StructVar(RStructVarDecl* decl, RTypeArguments* typeArgs, bool hasExplicitInstance, MLoc* explicitInstance)
        : decl{decl}, typeArgs{typeArgs}, hasExplicitInstance{hasExplicitInstance}, explicitInstance{explicitInstance}
    { }
    void Accept(ImExpVisitor& visitor) override;
};

struct ImExp_EnumElemVar : ImExp
{
    REnumElemVarDecl* decl;
    RTypeArguments* typeArgs;
    MLoc* instance;

    ImExp_EnumElemVar(REnumElemVarDecl* decl, RTypeArguments* typeArgs, MLoc* instance)
        : decl(decl), typeArgs(typeArgs), instance(instance)
    { }
    void Accept(ImExpVisitor& visitor) override;
};

struct ImExp_ListIndexer : ImExp
{
    MRead_Location instance;
    MRead_Value index;
    RType* itemType;

    ImExp_ListIndexer(MRead_Location&& instance, MRead_Value&& index, RType* itemType)
        : instance{std::move(instance)}, index{std::move(index)}, itemType{itemType}
    { }
    void Accept(ImExpVisitor& visitor) override;
};

struct ImExp_PtrDeref : ImExp
{
    MLoc* target;

    ImExp_PtrDeref(MLoc* target)
        : target{target}
    { }
    void Accept(ImExpVisitor& visitor) override;
};

struct ImExp_SharedDeref : ImExp
{
    MLoc* target;

    ImExp_SharedDeref(MLoc* target)
        : target{target}
    { }
    void Accept(ImExpVisitor& visitor) override;
};

// 기타의 경우
struct ImExp_Exp : ImExp
{
    MExp* exp;

    ImExp_Exp(MExp* exp)
        : exp{exp}
    { }
    void Accept(ImExpVisitor& visitor) override;
};

} // namespace Citron

#include "ImExpVisitor.g.h"