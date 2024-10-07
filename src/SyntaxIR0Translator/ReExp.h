#pragma once

#include <memory>

#include <IR0/RType.h>
#include <IR0/RLoc.h>
#include <IR0/RExp.h>

namespace Citron {

class RTypeFactory;

namespace SyntaxIR0Translator {

class ReThisVarExp;
class ReLocalVarExp;
class ReLambdaMemberVarExp;
class ReClassMemberVarExp;
class ReStructMemberVarExp;
class ReEnumElemMemberVarExp;
class ReLocalDerefExp;
class ReBoxDerefExp;
class ReListIndexerExp;
class ReElseExp;

class ReExpVisitor;

// NameResolvedExp 
class ReExp
{
public:
    virtual void Accept(ReExpVisitor& visitor) = 0;
    virtual RTypePtr GetType(RTypeFactory& factory) = 0;
};

using ReExpPtr = std::shared_ptr<ReExp>;

class ReExpVisitor
{
public:    
    virtual void Visit(ReThisVarExp& exp) = 0;
    virtual void Visit(ReLocalVarExp& exp) = 0;
    virtual void Visit(ReLambdaMemberVarExp& exp) = 0;
    virtual void Visit(ReClassMemberVarExp& exp) = 0;
    virtual void Visit(ReStructMemberVarExp& exp) = 0;
    virtual void Visit(ReEnumElemMemberVarExp& exp) = 0;
    virtual void Visit(ReLocalDerefExp& exp) = 0;
    virtual void Visit(ReBoxDerefExp& exp) = 0;
    virtual void Visit(ReListIndexerExp& exp) = 0;
    virtual void Visit(ReElseExp& exp) = 0;
};

class ReThisVarExp : public ReExp
{
public:
    RTypePtr type;

public:
    ReThisVarExp(const RTypePtr& type);
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RTypePtr GetType(RTypeFactory& factory) override { return type; }
};

class ReLocalVarExp : public ReExp
{
public:
    RTypePtr type;
    std::string name;
    
public:
    ReLocalVarExp(const RTypePtr& type, const std::string& name);
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RTypePtr GetType(RTypeFactory& factory) override { return type; }
};

class ReLambdaMemberVarExp : public ReExp
{
public:
    std::shared_ptr<RLambdaMemberVarDecl> decl;
    RTypeArgumentsPtr typeArgs;
    
public:
    ReLambdaMemberVarExp(const std::shared_ptr<RLambdaMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs);
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RTypePtr GetType(RTypeFactory& factory) override;
};

class ReClassMemberVarExp : public ReExp
{
public:
    std::shared_ptr<RClassMemberVarDecl> decl;
    RTypeArgumentsPtr typeArgs;
    bool hasExplicitInstance;
    ReExpPtr explicitInstance;
    
public:
    ReClassMemberVarExp(const std::shared_ptr<RClassMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs, bool hasExplicitInstance, const ReExpPtr& explicitInstance);
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RTypePtr GetType(RTypeFactory& factory) override;
};

class ReStructMemberVarExp : public ReExp
{
public:
    std::shared_ptr<RStructMemberVarDecl> decl;
    RTypeArgumentsPtr typeArgs;
    bool hasExplicitInstance;
    ReExpPtr explicitInstance;
    
public:
    ReStructMemberVarExp(const std::shared_ptr<RStructMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs, bool hasExplicitInstance, const ReExpPtr& explicitInstance);
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RTypePtr GetType(RTypeFactory& factory) override;
};

class ReEnumElemMemberVarExp : public ReExp
{
public:
    std::shared_ptr<REnumElemMemberVarDecl> decl;
    RTypeArgumentsPtr typeArgs;
    ReExpPtr instance;

public:
    ReEnumElemMemberVarExp(const std::shared_ptr<REnumElemMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs, const ReExpPtr& instance);
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RTypePtr GetType(RTypeFactory& factory) override;
};

class ReLocalDerefExp : public ReExp
{
public:
    ReExpPtr target;
    
public:
    ReLocalDerefExp(const ReExpPtr& target);
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RTypePtr GetType(RTypeFactory& factory) override;
};

class ReBoxDerefExp : public ReExp
{
public:
    ReExpPtr target;

public:
    ReBoxDerefExp(const ReExpPtr& target);
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RTypePtr GetType(RTypeFactory& factory) override;
};

class ReListIndexerExp : public ReExp
{   
public:
    ReExpPtr instance;
    RLocPtr index;
    RTypePtr itemType;
    
public:
    ReListIndexerExp(const ReExpPtr& instance, const RLocPtr& index, const RTypePtr& itemType);
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RTypePtr GetType(RTypeFactory& factory) override { return itemType; }
};

// 기타의 경우, Value
class ReElseExp : public ReExp
{
public:
    RExpPtr rExp;
    
public:
    ReElseExp(const RExpPtr& ptr);

    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RTypePtr GetType(RTypeFactory& factory) override;
};


} // namespace SyntaxIR0Translator

} // namespace Citron