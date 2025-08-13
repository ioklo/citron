#pragma once

#include <memory>
#include <string>

namespace Citron {

class RType;
class IR0Factory;
class RTypeArguments;
class RClassVarDecl;
class RStructVarDecl;
class REnumElemVarDecl;

class NExp;

class NLambdaVarDecl;

namespace SyntaxIR0Translator {

class ReExp_ThisVar;
class ReExp_LocalVar;
class ReExp_LambdaVar;
class ReExp_ClassVar;
class ReExp_StructVar;
class ReExp_EnumElemVar;
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
    virtual RType* GetType(IR0Factory& factory) = 0;
};

using ReExpPtr = std::shared_ptr<ReExp>;

class ReExpVisitor
{
public:    
    virtual ~ReExpVisitor() { }
    virtual void Visit(ReExp_ThisVar& exp) = 0;
    virtual void Visit(ReExp_LocalVar& exp) = 0;
    virtual void Visit(ReExp_LambdaVar& exp) = 0;
    virtual void Visit(ReExp_ClassVar& exp) = 0;
    virtual void Visit(ReExp_StructVar& exp) = 0;
    virtual void Visit(ReExp_EnumElemVar& exp) = 0;
    virtual void Visit(ReExp_LocalDeref& exp) = 0;
    virtual void Visit(ReExp_BoxDeref& exp) = 0;
    virtual void Visit(ReExp_ListIndexer& exp) = 0;
    virtual void Visit(ReExp_Else& exp) = 0;
};

class ReExp_ThisVar : public ReExp
{ 
public:
    RType* type;

public:
    ReExp_ThisVar(RType* type);
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RType* GetType(IR0Factory& factory) override { return type; }
};

class ReExp_LocalVar : public ReExp
{
public:
    RType* type;
    std::string name;
    
public:
    ReExp_LocalVar(RType* type, const std::string& name);
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RType* GetType(IR0Factory& factory) override { return type; }
};

class ReExp_LambdaVar : public ReExp
{
public:
    NLambdaVarDecl* decl;
    RTypeArguments* typeArgs;
    
public:
    ReExp_LambdaVar(NLambdaVarDecl* decl, RTypeArguments* typeArgs);
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RType* GetType(IR0Factory& factory) override;
};

class ReExp_ClassVar : public ReExp
{
public:
    RClassVarDecl* decl;
    RTypeArguments* typeArgs;
    bool hasExplicitInstance;
    ReExpPtr explicitInstance;
    
public:
    ReExp_ClassVar(RClassVarDecl* decl, RTypeArguments* typeArgs, bool hasExplicitInstance, const ReExpPtr& explicitInstance);
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RType* GetType(IR0Factory& factory) override;
};

class ReExp_StructVar : public ReExp
{
public:
    RStructVarDecl* decl;
    RTypeArguments* typeArgs;
    bool hasExplicitInstance;
    ReExpPtr explicitInstance;
    
public:
    ReExp_StructVar(RStructVarDecl* decl, RTypeArguments* typeArgs, bool hasExplicitInstance, const ReExpPtr& explicitInstance);
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RType* GetType(IR0Factory& factory) override;
};

class ReExp_EnumElemVar : public ReExp
{
public:
    REnumElemVarDecl* decl;
    RTypeArguments* typeArgs;
    ReExpPtr instance;

public:
    ReExp_EnumElemVar(REnumElemVarDecl* decl, RTypeArguments* typeArgs, const ReExpPtr& instance);
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RType* GetType(IR0Factory& factory) override;
};

class ReExp_LocalDeref : public ReExp
{
public:
    ReExpPtr target;
    
public:
    ReExp_LocalDeref(const ReExpPtr& target);
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RType* GetType(IR0Factory& factory) override;
};

class ReExp_BoxDeref : public ReExp
{
public:
    ReExpPtr target;

public:
    ReExp_BoxDeref(const ReExpPtr& target);
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RType* GetType(IR0Factory& factory) override;
};

class ReExp_ListIndexer : public ReExp
{   
public:
    ReExpPtr instance;
    ReExpPtr index;
    RType* itemType;
    
public:
    ReExp_ListIndexer(const ReExpPtr& instance, const ReExpPtr& index, RType* itemType);
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RType* GetType(IR0Factory& factory) override { return itemType; }
};

// 기타의 경우, Value
class ReExp_Else : public ReExp
{
public:
    NExp* nExp;
    
public:
    ReExp_Else(NExp* ptr);

    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RType* GetType(IR0Factory& factory) override;
};

} // namespace SyntaxIR0Translator
} // namespace Citron