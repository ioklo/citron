export module Citron.SyntaxIR0Translator:ReExp;

import <memory>;
import <string>;

import Citron.RDecls;
import Citron.NDecls;

namespace Citron::SyntaxIR0Translator {

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
export class ReExp
{
public:
    virtual ~ReExp() { }
    virtual void Accept(ReExpVisitor& visitor) = 0;
    virtual RTypePtr GetType(RTypeFactory& factory) = 0;
};

export using ReExpPtr = std::shared_ptr<ReExp>;

export class ReExpVisitor
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

export class ReExp_ThisVar : public ReExp
{ 
public:
    RTypePtr type;

public:
    ReExp_ThisVar(const RTypePtr& type);
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RTypePtr GetType(RTypeFactory& factory) override { return type; }
};

export class ReExp_LocalVar : public ReExp
{
public:
    RTypePtr type;
    std::string name;
    
public:
    ReExp_LocalVar(const RTypePtr& type, const std::string& name);
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RTypePtr GetType(RTypeFactory& factory) override { return type; }
};

export class ReExp_LambdaVar : public ReExp
{
public:
    std::shared_ptr<NLambdaVarDecl> decl;
    RTypeArgumentsPtr typeArgs;
    
public:
    ReExp_LambdaVar(const std::shared_ptr<NLambdaVarDecl>& decl, const RTypeArgumentsPtr& typeArgs);
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RTypePtr GetType(RTypeFactory& factory) override;
};

export class ReExp_ClassVar : public ReExp
{
public:
    std::shared_ptr<RClassVarDecl> decl;
    RTypeArgumentsPtr typeArgs;
    bool hasExplicitInstance;
    ReExpPtr explicitInstance;
    
public:
    ReExp_ClassVar(const std::shared_ptr<RClassVarDecl>& decl, const RTypeArgumentsPtr& typeArgs, bool hasExplicitInstance, const ReExpPtr& explicitInstance);
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RTypePtr GetType(RTypeFactory& factory) override;
};

export class ReExp_StructVar : public ReExp
{
public:
    std::shared_ptr<RStructVarDecl> decl;
    RTypeArgumentsPtr typeArgs;
    bool hasExplicitInstance;
    ReExpPtr explicitInstance;
    
public:
    ReExp_StructVar(const std::shared_ptr<RStructVarDecl>& decl, const RTypeArgumentsPtr& typeArgs, bool hasExplicitInstance, const ReExpPtr& explicitInstance);
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RTypePtr GetType(RTypeFactory& factory) override;
};

export class ReExp_EnumElemVar : public ReExp
{
public:
    std::shared_ptr<REnumElemVarDecl> decl;
    RTypeArgumentsPtr typeArgs;
    ReExpPtr instance;

public:
    ReExp_EnumElemVar(const std::shared_ptr<REnumElemVarDecl>& decl, const RTypeArgumentsPtr& typeArgs, const ReExpPtr& instance);
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RTypePtr GetType(RTypeFactory& factory) override;
};

export class ReExp_LocalDeref : public ReExp
{
public:
    ReExpPtr target;
    
public:
    ReExp_LocalDeref(const ReExpPtr& target);
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RTypePtr GetType(RTypeFactory& factory) override;
};

export class ReExp_BoxDeref : public ReExp
{
public:
    ReExpPtr target;

public:
    ReExp_BoxDeref(const ReExpPtr& target);
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RTypePtr GetType(RTypeFactory& factory) override;
};

export class ReExp_ListIndexer : public ReExp
{   
public:
    ReExpPtr instance;
    ReExpPtr index;
    RTypePtr itemType;
    
public:
    ReExp_ListIndexer(const ReExpPtr& instance, const ReExpPtr& index, const RTypePtr& itemType);
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RTypePtr GetType(RTypeFactory& factory) override { return itemType; }
};

// 기타의 경우, Value
export class ReExp_Else : public ReExp
{
public:
    NExpPtr nExp;
    
public:
    ReExp_Else(const NExpPtr& ptr);

    void Accept(ReExpVisitor& visitor) override { visitor.Visit(*this); }
    RTypePtr GetType(RTypeFactory& factory) override;
};

} // namespace Citron::SyntaxIR0Translator