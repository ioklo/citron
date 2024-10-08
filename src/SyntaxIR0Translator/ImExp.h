#pragma once

#include <memory>
#include <IR0/RGlobalFuncDecl.h>
#include <IR0/RExp.h>
#include <IR0/RLoc.h>
#include <IR0/RType.h>

#include "FuncsWithPartialTypeArgsComponent.h"

namespace Citron {

class RNamespaceDecl;
class RTypeVarType;

namespace SyntaxIR0Translator {

class ReExp;
using ReExpPtr = std::shared_ptr<ReExp>;

class ImNamespaceExp;
class ImGlobalFuncsExp;
class ImTypeVarExp;
class ImClassExp;
class ImClassMemberFuncsExp;
class ImStructExp;
class ImStructMemberFuncsExp;
class ImEnumExp;
class ImEnumElemExp;
class ImThisVarExp;
class ImLocalVarExp;
class ImLambdaMemberVarExp;
class ImClassMemberVarExp;
class ImStructMemberVarExp;
class ImEnumElemMemberVarExp;
class ImListIndexerExp;
class ImLocalDerefExp;
class ImBoxDerefExp;
class ImElseExp;
class ImExpVisitor;

class ImExp
{
public:
    virtual void Accept(ImExpVisitor& visitor) = 0;
};

class ImExpVisitor
{
public:
    virtual void Visit(ImNamespaceExp& imExp) = 0;
    virtual void Visit(ImGlobalFuncsExp& imExp) = 0;
    virtual void Visit(ImTypeVarExp& imExp) = 0;
    virtual void Visit(ImClassExp& imExp) = 0;
    virtual void Visit(ImClassMemberFuncsExp& imExp) = 0;
    virtual void Visit(ImStructExp& imExp) = 0;
    virtual void Visit(ImStructMemberFuncsExp& imExp) = 0;
    virtual void Visit(ImEnumExp& imExp) = 0;
    virtual void Visit(ImEnumElemExp& imExp) = 0;
    virtual void Visit(ImThisVarExp& imExp) = 0;
    virtual void Visit(ImLocalVarExp& imExp) = 0;
    virtual void Visit(ImLambdaMemberVarExp& imExp) = 0;
    virtual void Visit(ImClassMemberVarExp& imExp) = 0;
    virtual void Visit(ImStructMemberVarExp& imExp) = 0;
    virtual void Visit(ImEnumElemMemberVarExp& imExp) = 0;
    virtual void Visit(ImListIndexerExp& imExp) = 0;
    virtual void Visit(ImLocalDerefExp& imExp) = 0;
    virtual void Visit(ImBoxDerefExp& imExp) = 0;
    virtual void Visit(ImElseExp& imExp) = 0;
};

class ImNamespaceExp : public ImExp
{
public:
    std::shared_ptr<RNamespaceDecl> _namespace; // namespace를 뭘로 저장하고 있어야 하나

public:
    ImNamespaceExp(const std::shared_ptr<RNamespaceDecl>& _namespace);
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

// 
class ImGlobalFuncsExp
    : public ImExp
    , private FuncsWithPartialTypeArgsComponent<RGlobalFuncDecl>
{
    using FuncComp = FuncsWithPartialTypeArgsComponent<RGlobalFuncDecl>;

public:
    ImGlobalFuncsExp(const std::vector<RDeclWithOuterTypeArgs<RGlobalFuncDecl>>& items, const std::shared_ptr<RTypeArguments>& partialTypeArgsExceptOuter);

public:
    using FuncComp::GetCount;
    using FuncComp::GetDecl;
    using FuncComp::GetOuterTypeArgs;
    using FuncComp::GetPartialTypeArgsExceptOuter;

    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

class ImTypeVarExp : public ImExp
{
public:
    std::shared_ptr<RTypeVarType> type;

public:
    ImTypeVarExp(const std::shared_ptr<RTypeVarType>& type);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

class ImClassExp : public ImExp
{
public:
    std::shared_ptr<RClassDecl> classDecl;
    RTypeArgumentsPtr typeArgs;

public:
    ImClassExp(const std::shared_ptr<RClassDecl>& classDecl, RTypeArgumentsPtr&& typeArgs);
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

class ImClassMemberFuncsExp 
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
    ImClassMemberFuncsExp(const std::vector<RDeclWithOuterTypeArgs<RClassMemberFuncDecl>>& items, const std::shared_ptr<RTypeArguments>& partialTypeArgsExceptOuter, bool hasExplicitInstance, const ReExpPtr& explicitInstance);

public:
    using FuncComp::GetCount;
    using FuncComp::GetDecl;
    using FuncComp::GetOuterTypeArgs;
    using FuncComp::GetPartialTypeArgsExceptOuter;

    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

class ImStructExp : public ImExp
{
public:
    std::shared_ptr<RStructDecl> structDecl;
    RTypeArgumentsPtr typeArgs;

public:
    ImStructExp(const std::shared_ptr<RStructDecl>& structDecl, RTypeArgumentsPtr&& typeArgs);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

class ImStructMemberFuncsExp 
    : public ImExp
    , private FuncsWithPartialTypeArgsComponent<RStructMemberFuncDecl>
{
    using FuncComp = FuncsWithPartialTypeArgsComponent<RStructMemberFuncDecl>;
public:
    bool hasExplicitInstance;
    ReExpPtr explicitInstance;

public:
    ImStructMemberFuncsExp(const std::vector<RDeclWithOuterTypeArgs<RStructMemberFuncDecl>>& items, const std::shared_ptr<RTypeArguments>& partialTypeArgsExceptOuter, bool hasExplicitInstance, const ReExpPtr& explicitInstance);

public:
    using FuncComp::GetCount;
    using FuncComp::GetDecl;
    using FuncComp::GetOuterTypeArgs;
    using FuncComp::GetPartialTypeArgsExceptOuter;

    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

class ImEnumExp : public ImExp
{
public:
    std::shared_ptr<REnumDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    ImEnumExp(const std::shared_ptr<REnumDecl>& decl, const RTypeArgumentsPtr& typeArgs);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

class ImEnumElemExp : public ImExp
{
public:
    std::shared_ptr<REnumElemDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    ImEnumElemExp(const std::shared_ptr<REnumElemDecl>& decl, const RTypeArgumentsPtr& typeArgs);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

// exp로 사용할 수 있는
class ImThisVarExp : public ImExp
{
public:
    RTypePtr type;

public:
    ImThisVarExp(const RTypePtr& type);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

class ImLocalVarExp : public ImExp
{
public:
    RTypePtr type;
    std::string name;

public: 
    ImLocalVarExp(const RTypePtr& type, const std::string& name);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

class ImLambdaMemberVarExp : public ImExp
{
public:
    std::shared_ptr<RLambdaMemberVarDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    ImLambdaMemberVarExp(const std::shared_ptr<RLambdaMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

class ImClassMemberVarExp : public ImExp
{
public:
    std::shared_ptr<RClassMemberVarDecl> decl;
    RTypeArgumentsPtr typeArgs;
    
    bool hasExplicitInstance;
    ReExpPtr explicitInstance;

public:
    ImClassMemberVarExp(const std::shared_ptr<RClassMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs, bool hasExplicitInstance, const ReExpPtr& explicitInstance);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

class ImStructMemberVarExp : public ImExp
{
public:
    std::shared_ptr<RStructMemberVarDecl> decl;
    RTypeArgumentsPtr typeArgs;
    
    bool hasExplicitInstance;
    ReExpPtr explicitInstance;

public:
    ImStructMemberVarExp(const std::shared_ptr<RStructMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs, bool hasExplicitInstance, const ReExpPtr& explicitInstance);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

class ImEnumElemMemberVarExp : public ImExp
{
public:
    std::shared_ptr<REnumElemMemberVarDecl> decl;
    RTypeArgumentsPtr typeArgs;
    ReExpPtr instance;

public:
    ImEnumElemMemberVarExp(const std::shared_ptr<REnumElemMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs, const ReExpPtr& instance);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

class ImListIndexerExp : public ImExp
{
public:
    ReExpPtr instance;
    RLocPtr index;
    RTypePtr itemType;

public:
    ImListIndexerExp(ReExpPtr&& instance, RLocPtr&& index, RTypePtr&& itemType);

public:    
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

class ImLocalDerefExp : public ImExp
{
public:
    ReExpPtr target;

public:
    ImLocalDerefExp(const ReExpPtr& target);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

class ImBoxDerefExp : public ImExp
{
public:
    ReExpPtr target;

public:
    ImBoxDerefExp(const ReExpPtr& target);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

// 기타의 경우
class ImElseExp : public ImExp
{
public:
    RExpPtr exp;

public:
    ImElseExp(const RExpPtr& exp);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

} // namespace SyntaxIR0Translator

} // namespace Citron