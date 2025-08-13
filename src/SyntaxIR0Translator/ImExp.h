#pragma once

#include <memory>
#include <string>
#include "IR0/RGlobalFuncDecl.h"

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
class NExp;

class NLambdaVarDecl;

namespace SyntaxIR0Translator {

class ReExp;
using ReExpPtr = std::shared_ptr<ReExp>;

class ImExp_Namespace;
class ImExp_GlobalFuncs;
class ImExp_TypeVar;
class ImExp_Class;
class ImExp_ClassFuncs;
class ImExp_Struct;
class ImExp_StructFuncs;
class ImExp_Enum;
class ImExp_EnumElem;
class ImExp_ThisVar;
class ImExp_LocalVar;
class ImExp_LambdaVar;
class ImExp_ClassVar;
class ImExp_StructVar;
class ImExp_EnumElemVar;
class ImExp_ListIndexer;
class ImExp_LocalDeref;
class ImExp_BoxDeref;
class ImExp_Else;
class ImExpVisitor;

class ImExp
{
public:
    virtual ~ImExp() { }
    virtual void Accept(ImExpVisitor& visitor) = 0;
};

class ImExpVisitor
{
public:
    virtual ~ImExpVisitor() {}
    virtual void Visit(ImExp_Namespace& imExp) = 0;
    virtual void Visit(ImExp_GlobalFuncs& imExp) = 0;
    virtual void Visit(ImExp_TypeVar& imExp) = 0;
    virtual void Visit(ImExp_Class& imExp) = 0;
    virtual void Visit(ImExp_ClassFuncs& imExp) = 0;
    virtual void Visit(ImExp_Struct& imExp) = 0;
    virtual void Visit(ImExp_StructFuncs& imExp) = 0;
    virtual void Visit(ImExp_Enum& imExp) = 0;
    virtual void Visit(ImExp_EnumElem& imExp) = 0;
    virtual void Visit(ImExp_ThisVar& imExp) = 0;
    virtual void Visit(ImExp_LocalVar& imExp) = 0;
    virtual void Visit(ImExp_LambdaVar& imExp) = 0;
    virtual void Visit(ImExp_ClassVar& imExp) = 0;
    virtual void Visit(ImExp_StructVar& imExp) = 0;
    virtual void Visit(ImExp_EnumElemVar& imExp) = 0;
    virtual void Visit(ImExp_ListIndexer& imExp) = 0;
    virtual void Visit(ImExp_LocalDeref& imExp) = 0;
    virtual void Visit(ImExp_BoxDeref& imExp) = 0;
    virtual void Visit(ImExp_Else& imExp) = 0;
};

class ImExp_Namespace : public ImExp
{
public:
    RNamespaceDecl* _namespace; // namespace를 뭘로 저장하고 있어야 하나

public:
    ImExp_Namespace(RNamespaceDecl* _namespace);
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

// 
class ImExp_GlobalFuncs
    : public ImExp
    , private FuncsWithPartialTypeArgsComponent<RGlobalFuncDecl>
{
    using FuncComp = FuncsWithPartialTypeArgsComponent<RGlobalFuncDecl>;

public:
    using FuncComp::items;

public:
    ImExp_GlobalFuncs(const std::vector<DeclWithOuterTypeArgs<RGlobalFuncDecl>>& items, RTypeArguments* partialTypeArgsExceptOuter);
    virtual ~ImExp_GlobalFuncs();

public:
    using FuncComp::GetCount;
    using FuncComp::GetDecl;
    using FuncComp::GetOuterTypeArgs;
    using FuncComp::GetPartialTypeArgsExceptOuter;

    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

class ImExp_TypeVar : public ImExp
{
public:
    RType_TypeVar* type;

public:
    ImExp_TypeVar(RType_TypeVar* type);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

class ImExp_Class : public ImExp
{
public:
    RClassDecl* classDecl;
    RTypeArguments* typeArgs;

public:
    ImExp_Class(RClassDecl* classDecl, RTypeArguments* typeArgs);
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
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
    bool hasExplicitInstance;
    ReExpPtr explicitInstance;

private:
    using FuncComp = FuncsWithPartialTypeArgsComponent<RClassFuncDecl>;

public:
    ImExp_ClassFuncs(const std::vector<DeclWithOuterTypeArgs<RClassFuncDecl>>& items, RTypeArguments* partialTypeArgsExceptOuter, bool hasExplicitInstance, const ReExpPtr& explicitInstance);
    virtual ~ImExp_ClassFuncs();

public:
    using FuncComp::GetCount;
    using FuncComp::GetDecl;
    using FuncComp::GetOuterTypeArgs;
    using FuncComp::GetPartialTypeArgsExceptOuter;

    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

class ImExp_Struct : public ImExp
{
public:
    RStructDecl* structDecl;
    RTypeArguments* typeArgs;

public:
    ImExp_Struct(RStructDecl* structDecl, RTypeArguments* typeArgs);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

class ImExp_StructFuncs 
    : public ImExp
    , private FuncsWithPartialTypeArgsComponent<RStructFuncDecl>
{
    using FuncComp = FuncsWithPartialTypeArgsComponent<RStructFuncDecl>;

public:
    using FuncComp::items;
    bool hasExplicitInstance;
    ReExpPtr explicitInstance;

public:
    ImExp_StructFuncs(const std::vector<DeclWithOuterTypeArgs<RStructFuncDecl>>& items, RTypeArguments* partialTypeArgsExceptOuter, bool hasExplicitInstance, const ReExpPtr& explicitInstance);
    virtual ~ImExp_StructFuncs();

public:
    using FuncComp::GetCount;
    using FuncComp::GetDecl;
    using FuncComp::GetOuterTypeArgs;
    using FuncComp::GetPartialTypeArgsExceptOuter;

    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

class ImExp_Enum : public ImExp
{
public:
    REnumDecl* decl;
    RTypeArguments* typeArgs;

public:
    ImExp_Enum(REnumDecl* decl, RTypeArguments* typeArgs);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

class ImExp_EnumElem : public ImExp
{
public:
    REnumElemDecl* decl;
    RTypeArguments* typeArgs;

public:
    ImExp_EnumElem(REnumElemDecl* decl, RTypeArguments* typeArgs);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

// exp로 사용할 수 있는
class ImExp_ThisVar : public ImExp
{
public:
    RType* type;

public:
    ImExp_ThisVar(RType* type);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

class ImExp_LocalVar : public ImExp
{
public:
    RType* type;
    std::string name;

public: 
    ImExp_LocalVar(RType* type, const std::string& name);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

class ImExp_LambdaVar : public ImExp
{
public:
    NLambdaVarDecl* decl;
    RTypeArguments* typeArgs;

public:
    ImExp_LambdaVar(NLambdaVarDecl* decl, RTypeArguments* typeArgs);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

class ImExp_ClassVar : public ImExp
{
public:
    RClassVarDecl* decl;
    RTypeArguments* typeArgs;
    
    bool hasExplicitInstance;
    ReExpPtr explicitInstance;

public:
    ImExp_ClassVar(RClassVarDecl* decl, RTypeArguments* typeArgs, bool hasExplicitInstance, const ReExpPtr& explicitInstance);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

class ImExp_StructVar : public ImExp
{
public:
    RStructVarDecl* decl;
    RTypeArguments* typeArgs;
    
    bool hasExplicitInstance;
    ReExpPtr explicitInstance;

public:
    ImExp_StructVar(RStructVarDecl* decl, RTypeArguments* typeArgs, bool hasExplicitInstance, const ReExpPtr& explicitInstance);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

class ImExp_EnumElemVar : public ImExp
{
public:
    REnumElemVarDecl* decl;
    RTypeArguments* typeArgs;
    ReExpPtr instance;

public:
    ImExp_EnumElemVar(REnumElemVarDecl* decl, RTypeArguments* typeArgs, const ReExpPtr& instance);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

class ImExp_ListIndexer : public ImExp
{
public:
    ReExpPtr instance;
    ReExpPtr index;
    RType* itemType;

public:
    ImExp_ListIndexer(ReExpPtr&& instance, ReExpPtr&& index, RType* itemType);

public:    
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

class ImExp_LocalDeref : public ImExp
{
public:
    ReExpPtr target;

public:
    ImExp_LocalDeref(const ReExpPtr& target);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

class ImExp_BoxDeref : public ImExp
{
public:
    ReExpPtr target;

public:
    ImExp_BoxDeref(const ReExpPtr& target);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

// 기타의 경우
class ImExp_Else : public ImExp
{
public:
    NExp* exp;

public:
    ImExp_Else(NExp* exp);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

} // namespace SyntaxIR0Translator
} // namespace Citron