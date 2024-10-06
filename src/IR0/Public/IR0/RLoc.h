#pragma once
#include <variant>
#include <memory>
#include <string>

#include "RType.h"
#include "RNames.h"

namespace Citron {

class RTempLoc;
class RLocalVarLoc;
class RLambdaMemberVarLoc;
class RListIndexerLoc;
class RStructMemberLoc;
class RClassMemberLoc;
class REnumElemMemberLoc;
class RThisLoc;
class RLocalDerefLoc;
class RBoxDerefLoc;
class RNullableValueLoc;

class RLambdaMemberVarDecl;
class RStructMemberVarDecl;
class RClassMemberVarDecl;
class REnumElemMemberVarDecl;

class RExp;
using RExpPtr = std::shared_ptr<RExp>;

class RLocVisitor
{
public:
    virtual void Visit(RTempLoc& loc) = 0;
    virtual void Visit(RLocalVarLoc& loc) = 0;
    virtual void Visit(RLambdaMemberVarLoc& loc) = 0;
    virtual void Visit(RListIndexerLoc& loc) = 0;
    virtual void Visit(RStructMemberLoc& loc) = 0;
    virtual void Visit(RClassMemberLoc& loc) = 0;
    virtual void Visit(REnumElemMemberLoc& loc) = 0;
    virtual void Visit(RThisLoc& loc) = 0;
    virtual void Visit(RLocalDerefLoc& loc) = 0;
    virtual void Visit(RBoxDerefLoc& loc) = 0;
    virtual void Visit(RNullableValueLoc& loc) = 0;
};

class RLoc
{
public:
    virtual void Accept(RLocVisitor& visitor) = 0;
    virtual RTypePtr GetType(RTypeFactory& factory) = 0;
};

using RLocPtr = std::shared_ptr<RLoc>;

class RTempLoc : public RLoc
{
public:
    RExpPtr exp;
    
public:
    IR0_API RTempLoc(const RExpPtr& exp);
    void Accept(RLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
};

class RLocalVarLoc : public RLoc
{
public:
    std::string name;
    RTypePtr declType;

public:
    IR0_API RLocalVarLoc(const std::string& name, const RTypePtr& declType);
    void Accept(RLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
};

// only this member allowed, so no need this
class RLambdaMemberVarLoc : public RLoc
{
public:
    std::shared_ptr<RLambdaMemberVarDecl> memberVarDecl;
    RTypeArgumentsPtr typeArgs;

public:
    IR0_API RLambdaMemberVarLoc(const std::shared_ptr<RLambdaMemberVarDecl>& memberVarDecl, const RTypeArgumentsPtr& typeArgs);
    void Accept(RLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
};

// l[b], l is list
class RListIndexerLoc : public RLoc
{
public:
    RLocPtr list;
    RLocPtr index;
    RTypePtr itemType;

public:
    IR0_API RListIndexerLoc(RLocPtr&& list, const RLocPtr& index, const RTypePtr& itemType);
    void Accept(RLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
};

// Instance가 null이면 static
class RStructMemberLoc : public RLoc
{
public:
    RLocPtr instance;
    std::shared_ptr<RStructMemberVarDecl> memberVarDecl;
    RTypeArgumentsPtr typeArgs;

public:
    IR0_API RStructMemberLoc(const RLocPtr& instance, const std::shared_ptr<RStructMemberVarDecl>& memberVarDecl, const RTypeArgumentsPtr& typeArgs);
    void Accept(RLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
};

class RClassMemberLoc : public RLoc
{
public:
    RLocPtr instance;
    std::shared_ptr<RClassMemberVarDecl> memberVarDecl;
    RTypeArgumentsPtr typeArgs;

public:
    RClassMemberLoc(RLocPtr&& instance, const std::shared_ptr<RClassMemberVarDecl>& memberVarDecl, const RTypeArgumentsPtr& typeArgs);
    void Accept(RLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
};

class REnumElemMemberLoc : public RLoc
{
public:
    RLocPtr instance;
    std::shared_ptr<REnumElemMemberVarDecl> memberVarDecl;
    RTypeArgumentsPtr typeArgs;

public:
    IR0_API REnumElemMemberLoc(const RLocPtr& instance, std::shared_ptr<REnumElemMemberVarDecl>& memberVarDecl, const RTypeArgumentsPtr& typeArgs);
    void Accept(RLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
};

class RThisLoc : public RLoc
{
public:
    RTypePtr type;

public:
    IR0_API RThisLoc(RTypePtr type);
    void Accept(RLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
};

// dereference pointer, *
class RLocalDerefLoc : public RLoc
{
public:
    RLocPtr innerLoc;
public:
    IR0_API RLocalDerefLoc(RLocPtr&& innerLoc);
    void Accept(RLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
};

// dereference box pointer, *
class RBoxDerefLoc : public RLoc
{
public:
    RLocPtr innerLoc;

public:
    IR0_API RBoxDerefLoc(RLocPtr&& innerLoc);
    void Accept(RLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
};

// nullable value에서 value를 가져온다
class RNullableValueLoc : public RLoc
{
public:
    RLocPtr loc;
public:
    IR0_API RNullableValueLoc(const RLocPtr& loc);
    void Accept(RLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
};

}

