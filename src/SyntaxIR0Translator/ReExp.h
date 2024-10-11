#pragma once

#include <memory>

#include <IR0/RType.h>
#include <IR0/RLoc.h>
#include <IR0/RExp.h>

namespace Citron {

class RTypeFactory;

namespace SyntaxIR0Translator {

class ReExp_ThisVar;
class ReExp_LocalVar;
class ReExp_LambdaMemberVar;
class ReExp_ClassMemberVar;
class ReExp_StructMemberVar;
class ReExp_EnumElemMemberVar;
class ReExp_LocalDeref;
class ReExp_BoxDeref;
class ReExp_ListIndexer;
class ReExp_Else;

class ReExpVisitor;

// NameResolvedExp 
class ReExp
{
public:
    virtual ~ReExp() { }
    virtual void Accept(ReExpVisitor& visitor) = 0;
    virtual RTypePtr GetType(RTypeFactory& factory) = 0;
};

using ReExpPtr = std::shared_ptr<ReExp>;

class ReExpVisitor
{
public:    
    virtual ~ReExpVisitor() { }
    virtual void Visit(ReExp_ThisVar& exp) = 0;
    virtual void Visit(ReExp_LocalVar& exp) = 0;
    virtual void Visit(ReExp_LambdaMemberVar& exp) = 0;
    virtual void Visit(ReExp_ClassMemberVar& exp) = 0;
    virtual void Visit(ReExp_StructMemberVar& exp) = 0;
    virtual void Visit(ReExp_EnumElemMemberVar& exp) = 0;
    virtual void Visit(ReExp_LocalDeref& exp) = 0;
    virtual void Visit(ReExp_BoxDeref& exp) = 0;
    virtual void Visit(ReExp_ListIndexer& exp) = 0;
    virtual void Visit(ReExp_Else& exp) = 0;
};

class ReExp_ThisVar : public ReExp
{ 
public:
    RTypePtr type;

public:
    ReExp_ThisVar(const RTypePtr& type);
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RTypePtr GetType(RTypeFactory& factory) override { return type; }
};

class ReExp_LocalVar : public ReExp
{
public:
    RTypePtr type;
    std::string name;
    
public:
    ReExp_LocalVar(const RTypePtr& type, const std::string& name);
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RTypePtr GetType(RTypeFactory& factory) override { return type; }
};

class ReExp_LambdaMemberVar : public ReExp
{
public:
    std::shared_ptr<RLambdaMemberVarDecl> decl;
    RTypeArgumentsPtr typeArgs;
    
public:
    ReExp_LambdaMemberVar(const std::shared_ptr<RLambdaMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs);
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RTypePtr GetType(RTypeFactory& factory) override;
};

class ReExp_ClassMemberVar : public ReExp
{
public:
    std::shared_ptr<RClassMemberVarDecl> decl;
    RTypeArgumentsPtr typeArgs;
    bool hasExplicitInstance;
    ReExpPtr explicitInstance;
    
public:
    ReExp_ClassMemberVar(const std::shared_ptr<RClassMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs, bool hasExplicitInstance, const ReExpPtr& explicitInstance);
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RTypePtr GetType(RTypeFactory& factory) override;
};

class ReExp_StructMemberVar : public ReExp
{
public:
    std::shared_ptr<RStructMemberVarDecl> decl;
    RTypeArgumentsPtr typeArgs;
    bool hasExplicitInstance;
    ReExpPtr explicitInstance;
    
public:
    ReExp_StructMemberVar(const std::shared_ptr<RStructMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs, bool hasExplicitInstance, const ReExpPtr& explicitInstance);
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RTypePtr GetType(RTypeFactory& factory) override;
};

class ReExp_EnumElemMemberVar : public ReExp
{
public:
    std::shared_ptr<REnumElemMemberVarDecl> decl;
    RTypeArgumentsPtr typeArgs;
    ReExpPtr instance;

public:
    ReExp_EnumElemMemberVar(const std::shared_ptr<REnumElemMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs, const ReExpPtr& instance);
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RTypePtr GetType(RTypeFactory& factory) override;
};

class ReExp_LocalDeref : public ReExp
{
public:
    ReExpPtr target;
    
public:
    ReExp_LocalDeref(const ReExpPtr& target);
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RTypePtr GetType(RTypeFactory& factory) override;
};

class ReExp_BoxDeref : public ReExp
{
public:
    ReExpPtr target;

public:
    ReExp_BoxDeref(const ReExpPtr& target);
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RTypePtr GetType(RTypeFactory& factory) override;
};

class ReExp_ListIndexer : public ReExp
{   
public:
    ReExpPtr instance;
    RLocPtr index;
    RTypePtr itemType;
    
public:
    ReExp_ListIndexer(const ReExpPtr& instance, const RLocPtr& index, const RTypePtr& itemType);
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RTypePtr GetType(RTypeFactory& factory) override { return itemType; }
};

// 기타의 경우, Value
class ReExp_Else : public ReExp
{
public:
    RExpPtr rExp;
    
public:
    ReExp_Else(const RExpPtr& ptr);

    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RTypePtr GetType(RTypeFactory& factory) override;
};


} // namespace SyntaxIR0Translator

} // namespace Citron