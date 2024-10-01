#pragma once

#include <memory>

#include <IR0/RType.h>
#include <IR0/RLoc.h>
#include <IR0/RExp.h>

namespace Citron {

namespace SyntaxIR0Translator {

class ReThisVarExp;
class ReLocalVarExp;
class ReLambdaMemberVarExp;
class ReClassMemberVarExp;
class ReStructMemberVarExp;
class ReEnumElemMemberVarExp;
class ReListIndexerExp;
class ReLocalDerefExp;
class ReBoxDerefExp;
class ReElseExp;

class ReExpVisitor;

// NameResolvedExp 
class ReExp
{
public:
    virtual void Accept(ReExpVisitor& visitor) = 0;
    virtual RTypePtr GetType() = 0;
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
    virtual void Visit(ReListIndexerExp& exp) = 0;
    virtual void Visit(ReLocalDerefExp& exp) = 0;
    virtual void Visit(ReBoxDerefExp& exp) = 0;
    virtual void Visit(ReElseExp& exp) = 0;
};

class ReThisVarExp : public ReExp
{
    RTypePtr type;

public:
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RTypePtr GetType() override { return type; }
};

class ReLocalVarExp : public ReExp
{
    RTypePtr type;
    std::string name;
    
public:
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RTypePtr GetType() override { return type; }
};

class ReLambdaMemberVarExp : public ReExp
{
    std::shared_ptr<RLambdaMemberVarDecl> decl;
    RTypeArgumentsPtr typeArgs;
    
public:
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RTypePtr GetType() override;
};

class ReClassMemberVarExp : public ReExp
{
    std::shared_ptr<RClassMemberVarDecl> decl;
    RTypeArgumentsPtr typeArgs;
    bool hasExplicitInstance;
    ReExpPtr explicitInstance;
    
public:
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RTypePtr GetType() override;
};

class ReStructMemberVarExp : public ReExp
{
    std::shared_ptr<RStructMemberVarDecl> decl;
    RTypeArgumentsPtr typeArgs;
    bool hasExplicitInstance;
    ReExpPtr explicitInstance;
    
public:
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RTypePtr GetType() override;
};

class ReEnumElemMemberVarExp : public ReExp
{
    std::shared_ptr<REnumElemMemberVarDecl> decl;
    RTypeArgumentsPtr typeArgs;

    ReExpPtr instance;

public:
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RTypePtr GetType() override;
};

class ReLocalDerefExp : public ReExp
{
    ReExpPtr target;
    
public:
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RTypePtr GetType() override;
};

class ReBoxDerefExp : public ReExp
{
    ReExpPtr target;

public:
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RTypePtr GetType() override;
};

class ReListIndexerExp : public ReExp
{   
    ReExpPtr instance;
    RLocPtr index;
    RTypePtr itemType;
    
public:
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RTypePtr GetType() override { return itemType; }
};

// 기타의 경우, Value
class ReElseExp : public ReExp
{
    RExpPtr rExp;
    
public:
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RTypePtr GetType() override;
};


} // namespace SyntaxIR0Translator

} // namespace Citron