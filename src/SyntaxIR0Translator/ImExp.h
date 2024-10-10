#pragma once

#include <memory>
#include <IR0/RGlobalFuncDecl.h>
#include <IR0/RExp.h>
#include <IR0/RLoc.h>
#include <IR0/RType.h>

#include "FuncsWithPartialTypeArgsComponent.h"

namespace Citron {

class RNamespaceDecl;
class RType_TypeVar;

namespace SyntaxIR0Translator {

using ReExpPtr = std::shared_ptr<class ReExp>;

class ImExp_Namespace;
class ImExp_GlobalFuncs;
class ImExp_TypeVar;
class ImExp_Class;
class ImExp_ClassMemberFuncs;
class ImExp_Struct;
class ImExp_StructMemberFuncs;
class ImExp_Enum;
class ImExp_EnumElem;
class ImExp_ThisVar;
class ImExp_LocalVar;
class ImExp_LambdaMemberVar;
class ImExp_ClassMemberVar;
class ImExp_StructMemberVar;
class ImExp_EnumElemMemberVar;
class ImExp_ListIndexer;
class ImExp_LocalDeref;
class ImExp_BoxDeref;
class ImExp_Else;
class ImExpVisitor;

class ImExp
{
public:
    virtual void Accept(ImExpVisitor& visitor) = 0;
};

class ImExpVisitor
{
public:
    virtual void Visit(ImExp_Namespace& imExp) = 0;
    virtual void Visit(ImExp_GlobalFuncs& imExp) = 0;
    virtual void Visit(ImExp_TypeVar& imExp) = 0;
    virtual void Visit(ImExp_Class& imExp) = 0;
    virtual void Visit(ImExp_ClassMemberFuncs& imExp) = 0;
    virtual void Visit(ImExp_Struct& imExp) = 0;
    virtual void Visit(ImExp_StructMemberFuncs& imExp) = 0;
    virtual void Visit(ImExp_Enum& imExp) = 0;
    virtual void Visit(ImExp_EnumElem& imExp) = 0;
    virtual void Visit(ImExp_ThisVar& imExp) = 0;
    virtual void Visit(ImExp_LocalVar& imExp) = 0;
    virtual void Visit(ImExp_LambdaMemberVar& imExp) = 0;
    virtual void Visit(ImExp_ClassMemberVar& imExp) = 0;
    virtual void Visit(ImExp_StructMemberVar& imExp) = 0;
    virtual void Visit(ImExp_EnumElemMemberVar& imExp) = 0;
    virtual void Visit(ImExp_ListIndexer& imExp) = 0;
    virtual void Visit(ImExp_LocalDeref& imExp) = 0;
    virtual void Visit(ImExp_BoxDeref& imExp) = 0;
    virtual void Visit(ImExp_Else& imExp) = 0;
};

class ImExp_Namespace : public ImExp
{
public:
    std::shared_ptr<RNamespaceDecl> _namespace; // namespace를 뭘로 저장하고 있어야 하나

public:
    ImExp_Namespace(const std::shared_ptr<RNamespaceDecl>& _namespace);
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

// 
class ImExp_GlobalFuncs
    : public ImExp
    , private FuncsWithPartialTypeArgsComponent<RGlobalFuncDecl>
{
    using FuncComp = FuncsWithPartialTypeArgsComponent<RGlobalFuncDecl>;

public:
    ImExp_GlobalFuncs(const std::vector<RDeclWithOuterTypeArgs<RGlobalFuncDecl>>& items, const std::shared_ptr<RTypeArguments>& partialTypeArgsExceptOuter);

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
    std::shared_ptr<RType_TypeVar> type;

public:
    ImExp_TypeVar(const std::shared_ptr<RType_TypeVar>& type);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

class ImExp_Class : public ImExp
{
public:
    std::shared_ptr<RClassDecl> classDecl;
    RTypeArgumentsPtr typeArgs;

public:
    ImExp_Class(const std::shared_ptr<RClassDecl>& classDecl, RTypeArgumentsPtr&& typeArgs);
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

class ImExp_ClassMemberFuncs 
    : public ImExp
    , private FuncsWithPartialTypeArgsComponent<RClassMemberFuncDecl>
{
public:
    // HasExplicitInstance: x.F 처럼 x가 명시적으로 있는 경우 true, F 처럼 this.F 나 C.F 를 암시적으로 나타낸 경우라면 false, C.F는 명시적이지만 인스턴스가 아니므로 false
    // ExplicitInstance: HasExplicitInstance가 true일때만 의미가 있다

    // C.F => HasExplicitInstance: true, null
    // x.F => HasExplicitInstance: true, "x"
    // F   => HasExplicitInstance: false, null
    bool hasExplicitInstance;
    ReExpPtr explicitInstance;

private:
    using FuncComp = FuncsWithPartialTypeArgsComponent<RClassMemberFuncDecl>;

public:
    ImExp_ClassMemberFuncs(const std::vector<RDeclWithOuterTypeArgs<RClassMemberFuncDecl>>& items, const std::shared_ptr<RTypeArguments>& partialTypeArgsExceptOuter, bool hasExplicitInstance, const ReExpPtr& explicitInstance);

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
    std::shared_ptr<RStructDecl> structDecl;
    RTypeArgumentsPtr typeArgs;

public:
    ImExp_Struct(const std::shared_ptr<RStructDecl>& structDecl, RTypeArgumentsPtr&& typeArgs);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

class ImExp_StructMemberFuncs 
    : public ImExp
    , private FuncsWithPartialTypeArgsComponent<RStructMemberFuncDecl>
{
    using FuncComp = FuncsWithPartialTypeArgsComponent<RStructMemberFuncDecl>;
public:
    bool hasExplicitInstance;
    ReExpPtr explicitInstance;

public:
    ImExp_StructMemberFuncs(const std::vector<RDeclWithOuterTypeArgs<RStructMemberFuncDecl>>& items, const std::shared_ptr<RTypeArguments>& partialTypeArgsExceptOuter, bool hasExplicitInstance, const ReExpPtr& explicitInstance);

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
    std::shared_ptr<REnumDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    ImExp_Enum(const std::shared_ptr<REnumDecl>& decl, const RTypeArgumentsPtr& typeArgs);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

class ImExp_EnumElem : public ImExp
{
public:
    std::shared_ptr<REnumElemDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    ImExp_EnumElem(const std::shared_ptr<REnumElemDecl>& decl, const RTypeArgumentsPtr& typeArgs);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

// exp로 사용할 수 있는
class ImExp_ThisVar : public ImExp
{
public:
    RTypePtr type;

public:
    ImExp_ThisVar(const RTypePtr& type);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

class ImExp_LocalVar : public ImExp
{
public:
    RTypePtr type;
    std::string name;

public: 
    ImExp_LocalVar(const RTypePtr& type, const std::string& name);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

class ImExp_LambdaMemberVar : public ImExp
{
public:
    std::shared_ptr<RLambdaMemberVarDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    ImExp_LambdaMemberVar(const std::shared_ptr<RLambdaMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

class ImExp_ClassMemberVar : public ImExp
{
public:
    std::shared_ptr<RClassMemberVarDecl> decl;
    RTypeArgumentsPtr typeArgs;
    
    bool hasExplicitInstance;
    ReExpPtr explicitInstance;

public:
    ImExp_ClassMemberVar(const std::shared_ptr<RClassMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs, bool hasExplicitInstance, const ReExpPtr& explicitInstance);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

class ImExp_StructMemberVar : public ImExp
{
public:
    std::shared_ptr<RStructMemberVarDecl> decl;
    RTypeArgumentsPtr typeArgs;
    
    bool hasExplicitInstance;
    ReExpPtr explicitInstance;

public:
    ImExp_StructMemberVar(const std::shared_ptr<RStructMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs, bool hasExplicitInstance, const ReExpPtr& explicitInstance);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

class ImExp_EnumElemMemberVar : public ImExp
{
public:
    std::shared_ptr<REnumElemMemberVarDecl> decl;
    RTypeArgumentsPtr typeArgs;
    ReExpPtr instance;

public:
    ImExp_EnumElemMemberVar(const std::shared_ptr<REnumElemMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs, const ReExpPtr& instance);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

class ImExp_ListIndexer : public ImExp
{
public:
    ReExpPtr instance;
    RLocPtr index;
    RTypePtr itemType;

public:
    ImExp_ListIndexer(ReExpPtr&& instance, RLocPtr&& index, RTypePtr&& itemType);

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
    RExpPtr exp;

public:
    ImExp_Else(const RExpPtr& exp);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

} // namespace SyntaxIR0Translator

} // namespace Citron