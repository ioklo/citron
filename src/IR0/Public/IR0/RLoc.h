#pragma once
#include <variant>
#include <memory>
#include <string>

#include "RType.h"
#include "RNames.h"

namespace Citron {

class RLoc_Temp;
class RLoc_LocalVar;
class RLoc_LambdaMemberVar;
class RLoc_ListIndexer;
class RLoc_StructMember;
class RLoc_ClassMember;
class RLoc_EnumElemMember;
class RLoc_This;
class RLoc_LocalDeref;
class RLoc_BoxDeref;
class RLoc_NullableValue;

class RLambdaMemberVarDecl;
class RStructMemberVarDecl;
class RClassMemberVarDecl;
class REnumElemMemberVarDecl;

class RExp;
using RExpPtr = std::shared_ptr<RExp>;

class RLocVisitor
{
public:
    virtual void Visit(RLoc_Temp& loc) = 0;
    virtual void Visit(RLoc_LocalVar& loc) = 0;
    virtual void Visit(RLoc_LambdaMemberVar& loc) = 0;
    virtual void Visit(RLoc_ListIndexer& loc) = 0;
    virtual void Visit(RLoc_StructMember& loc) = 0;
    virtual void Visit(RLoc_ClassMember& loc) = 0;
    virtual void Visit(RLoc_EnumElemMember& loc) = 0;
    virtual void Visit(RLoc_This& loc) = 0;
    virtual void Visit(RLoc_LocalDeref& loc) = 0;
    virtual void Visit(RLoc_BoxDeref& loc) = 0;
    virtual void Visit(RLoc_NullableValue& loc) = 0;
};

class RLoc
{
public:
    virtual void Accept(RLocVisitor& visitor) = 0;
    virtual RTypePtr GetType(RTypeFactory& factory) = 0;
};

using RLocPtr = std::shared_ptr<RLoc>;

class RLoc_Temp : public RLoc
{
public:
    RExpPtr exp;
    
public:
    IR0_API RLoc_Temp(const RExpPtr& exp);
    IR0_API RLoc_Temp(RExpPtr&& exp);
    void Accept(RLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
};

class RLoc_LocalVar : public RLoc
{
public:
    std::string name;
    RTypePtr declType;

public:
    IR0_API RLoc_LocalVar(const std::string& name, const RTypePtr& declType);
    void Accept(RLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
};

// only this member allowed, so no need this
class RLoc_LambdaMemberVar : public RLoc
{
public:
    std::shared_ptr<RLambdaMemberVarDecl> memberVarDecl;
    RTypeArgumentsPtr typeArgs;

public:
    IR0_API RLoc_LambdaMemberVar(const std::shared_ptr<RLambdaMemberVarDecl>& memberVarDecl, const RTypeArgumentsPtr& typeArgs);
    void Accept(RLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
};

// l[b], l is list
class RLoc_ListIndexer : public RLoc
{
public:
    RLocPtr list;
    RLocPtr index;
    RTypePtr itemType;

public:
    IR0_API RLoc_ListIndexer(RLocPtr&& list, const RLocPtr& index, const RTypePtr& itemType);
    void Accept(RLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
};

// Instance가 null이면 static
class RLoc_StructMember : public RLoc
{
public:
    RLocPtr instance;
    std::shared_ptr<RStructMemberVarDecl> memberVarDecl;
    RTypeArgumentsPtr typeArgs;

public:
    IR0_API RLoc_StructMember(const RLocPtr& instance, const std::shared_ptr<RStructMemberVarDecl>& memberVarDecl, const RTypeArgumentsPtr& typeArgs);
    void Accept(RLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
};

class RLoc_ClassMember : public RLoc
{
public:
    RLocPtr instance;
    std::shared_ptr<RClassMemberVarDecl> memberVarDecl;
    RTypeArgumentsPtr typeArgs;

public:
    RLoc_ClassMember(RLocPtr&& instance, const std::shared_ptr<RClassMemberVarDecl>& memberVarDecl, const RTypeArgumentsPtr& typeArgs);
    void Accept(RLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
};

class RLoc_EnumElemMember : public RLoc
{
public:
    RLocPtr instance;
    std::shared_ptr<REnumElemMemberVarDecl> memberVarDecl;
    RTypeArgumentsPtr typeArgs;

public:
    IR0_API RLoc_EnumElemMember(const RLocPtr& instance, std::shared_ptr<REnumElemMemberVarDecl>& memberVarDecl, const RTypeArgumentsPtr& typeArgs);
    void Accept(RLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
};

class RLoc_This : public RLoc
{
public:
    RTypePtr type;

public:
    IR0_API RLoc_This(RTypePtr type);
    void Accept(RLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
};

// dereference pointer, *
class RLoc_LocalDeref : public RLoc
{
public:
    RLocPtr innerLoc;
public:
    IR0_API RLoc_LocalDeref(RLocPtr&& innerLoc);
    void Accept(RLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
};

// dereference box pointer, *
class RLoc_BoxDeref : public RLoc
{
public:
    RLocPtr innerLoc;

public:
    IR0_API RLoc_BoxDeref(RLocPtr&& innerLoc);
    void Accept(RLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
};

// nullable value에서 value를 가져온다
class RLoc_NullableValue : public RLoc
{
public:
    RLocPtr loc;
public:
    IR0_API RLoc_NullableValue(const RLocPtr& loc);
    void Accept(RLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
};

}

