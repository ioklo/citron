#pragma once

#include <memory>
#include <string>
#include "RSymbol/RGlobalFuncDecl.h"

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

class ReExp;

struct ImExpVisitor;
class ImExp
{
public:
    virtual ~ImExp() { }
    virtual void Accept(ImExpVisitor& visitor) = 0;
};

class ImExp_Namespace : public ImExp
{
public:
    RNamespaceDecl* _namespace; // namespace를 뭘로 저장하고 있어야 하나

public:
    ImExp_Namespace(RNamespaceDecl* _namespace);
    void Accept(ImExpVisitor& visitor) override;
};

// 
class ImExp_GlobalFuncs
    : public ImExp
    , private FuncsWithPartialTypeArgsComponent<RGlobalFuncDecl>
{
    using FuncComp = FuncsWithPartialTypeArgsComponent<RGlobalFuncDecl>;

public:
    using FuncComp::items;
    using FuncComp::partialTypeArgsExceptOuter;

public:
    ImExp_GlobalFuncs(const std::vector<DeclWithOuterTypeArgs<RGlobalFuncDecl>>& items, RTypeArguments* partialTypeArgsExceptOuter);
    virtual ~ImExp_GlobalFuncs();

public:
    using FuncComp::GetCount;
    using FuncComp::GetDecl;
    using FuncComp::GetOuterTypeArgs;
    using FuncComp::GetPartialTypeArgsExceptOuter;

    void Accept(ImExpVisitor& visitor) override;
};

class ImExp_TypeVar : public ImExp
{
public:
    RType_TypeVar* type;

public:
    ImExp_TypeVar(RType_TypeVar* type);

public:
    void Accept(ImExpVisitor& visitor) override;
};

class ImExp_Class : public ImExp
{
public:
    RClassDecl* classDecl;
    RTypeArguments* typeArgs;

public:
    ImExp_Class(RClassDecl* classDecl, RTypeArguments* typeArgs);
    void Accept(ImExpVisitor& visitor) override;
};

class ImExp_ClassFuncs 
    : public ImExp
    , private FuncsWithPartialTypeArgsComponent<RClassFuncDecl>
{
public:
    // HasExplicitInstance: x.F 처럼 x가 명시적으로 있는 경우 true, F 처럼 this.F 나 C.F 를 암시적으로 나타낸 경우라면 false, C.F는 명시적이지만 인스턴스가 아니므로 false
    // ExplicitInstance: HasExplicitInstance가 true일때만 의미가 있다

    // C.F => HasExplicitInstance: true, null
    // x.F => HasExplicitInstance: true, "x"
    // F   => HasExplicitInstance: false, null
    using FuncsWithPartialTypeArgsComponent::items;
    using FuncsWithPartialTypeArgsComponent::partialTypeArgsExceptOuter;
    bool hasExplicitInstance;
    ReExp* explicitInstance;

private:
    using FuncComp = FuncsWithPartialTypeArgsComponent<RClassFuncDecl>;

public:
    ImExp_ClassFuncs(const std::vector<DeclWithOuterTypeArgs<RClassFuncDecl>>& items, RTypeArguments* partialTypeArgsExceptOuter, bool hasExplicitInstance, ReExp* explicitInstance);
    virtual ~ImExp_ClassFuncs();

public:
    using FuncComp::GetCount;
    using FuncComp::GetDecl;
    using FuncComp::GetOuterTypeArgs;
    using FuncComp::GetPartialTypeArgsExceptOuter;

    void Accept(ImExpVisitor& visitor) override;
};

class ImExp_Struct : public ImExp
{
public:
    RStructDecl* structDecl;
    RTypeArguments* typeArgs;

public:
    ImExp_Struct(RStructDecl* structDecl, RTypeArguments* typeArgs);

public:
    void Accept(ImExpVisitor& visitor) override;
};

class ImExp_StructFuncs 
    : public ImExp
    , private FuncsWithPartialTypeArgsComponent<RStructFuncDecl>
{
    using FuncComp = FuncsWithPartialTypeArgsComponent<RStructFuncDecl>;

public:
    using FuncComp::items;
    using FuncComp::partialTypeArgsExceptOuter;

    bool hasExplicitInstance;
    ReExp* explicitInstance;

public:
    ImExp_StructFuncs(const std::vector<DeclWithOuterTypeArgs<RStructFuncDecl>>& items, RTypeArguments* partialTypeArgsExceptOuter, bool hasExplicitInstance, ReExp* explicitInstance);
    virtual ~ImExp_StructFuncs();

public:
    using FuncComp::GetCount;
    using FuncComp::GetDecl;
    using FuncComp::GetOuterTypeArgs;
    using FuncComp::GetPartialTypeArgsExceptOuter;

    void Accept(ImExpVisitor& visitor) override;
};

class ImExp_Enum : public ImExp
{
public:
    REnumDecl* decl;
    RTypeArguments* typeArgs;

public:
    ImExp_Enum(REnumDecl* decl, RTypeArguments* typeArgs);

public:
    void Accept(ImExpVisitor& visitor) override;
};

class ImExp_EnumElem : public ImExp
{
public:
    REnumElemDecl* decl;
    RTypeArguments* typeArgs;

public:
    ImExp_EnumElem(REnumElemDecl* decl, RTypeArguments* typeArgs);

public:
    void Accept(ImExpVisitor& visitor) override;
};

// exp로 사용할 수 있는
class ImExp_ThisVar : public ImExp
{
public:
    RType* type;

public:
    ImExp_ThisVar(RType* type);

public:
    void Accept(ImExpVisitor& visitor) override;
};

class ImExp_LocalVar : public ImExp
{
public:
    RType* type;
    RName name;

public: 
    ImExp_LocalVar(RType* type, const RName& name);

public:
    void Accept(ImExpVisitor& visitor) override;
};

class ImExp_LocalRef : public ImExp
{
public:
    RType* type;
    RName name;

public:
    ImExp_LocalRef(RType* type, const RName& name);

public:
    void Accept(ImExpVisitor& visitor) override;
};

class ImExp_LambdaVar : public ImExp
{
public:
    NLambdaVarDecl* decl;
    RTypeArguments* typeArgs;

public:
    ImExp_LambdaVar(NLambdaVarDecl* decl, RTypeArguments* typeArgs);

public:
    void Accept(ImExpVisitor& visitor) override;
};

class ImExp_ClassVar : public ImExp
{
public:
    RClassVarDecl* decl;
    RTypeArguments* typeArgs;
    
    bool hasExplicitInstance;
    ReExp* explicitInstance;

public:
    ImExp_ClassVar(RClassVarDecl* decl, RTypeArguments* typeArgs, bool hasExplicitInstance, ReExp* explicitInstance);

public:
    void Accept(ImExpVisitor& visitor) override;
};

class ImExp_StructVar : public ImExp
{
public:
    RStructVarDecl* decl;
    RTypeArguments* typeArgs;
    
    bool hasExplicitInstance;
    ReExp* explicitInstance;

public:
    ImExp_StructVar(RStructVarDecl* decl, RTypeArguments* typeArgs, bool hasExplicitInstance, ReExp* explicitInstance);

public:
    void Accept(ImExpVisitor& visitor) override;
};

class ImExp_EnumElemVar : public ImExp
{
public:
    REnumElemVarDecl* decl;
    RTypeArguments* typeArgs;
    ReExp* instance;

public:
    ImExp_EnumElemVar(REnumElemVarDecl* decl, RTypeArguments* typeArgs, ReExp* instance);

public:
    void Accept(ImExpVisitor& visitor) override;
};

class ImExp_ListIndexer : public ImExp
{
public:
    ReExp* instance;
    ReExp* index;
    RType* itemType;

public:
    ImExp_ListIndexer(ReExp* instance, ReExp* index, RType* itemType);

public:    
    void Accept(ImExpVisitor& visitor) override;
};

class ImExp_PtrDeref : public ImExp
{
public:
    ReExp* target;

public:
    ImExp_PtrDeref(ReExp* target);

public:
    void Accept(ImExpVisitor& visitor) override;
};

class ImExp_SharedDeref : public ImExp
{
public:
    ReExp* target;

public:
    ImExp_SharedDeref(ReExp* target);

public:
    void Accept(ImExpVisitor& visitor) override;
};

// 기타의 경우
class ImExp_Else : public ImExp
{
public:
    MExp* exp;

public:
    ImExp_Else(MExp* exp);

public:
    void Accept(ImExpVisitor& visitor) override;
};

} // namespace Citron

#include "ImExpVisitor.g.h"