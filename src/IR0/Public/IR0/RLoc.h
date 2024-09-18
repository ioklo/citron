#pragma once
#include <variant>
#include <memory>

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
using RExpPtr = std::unique_ptr<RExp>;

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
};

using RLocPtr = std::unique_ptr<RLoc>;

class RTempLoc : public RLoc
{
    RExpPtr exp;
    RTypePtr type;

public:
    void Accept(RLocVisitor& visitor) override { visitor.Visit(*this); }
};

class RLocalVarLoc : public RLoc
{
    RName Name;

public:
    void Accept(RLocVisitor& visitor) override { visitor.Visit(*this); }
};

// only this member allowed, so no need this
class RLambdaMemberVarLoc : public RLoc
{
    std::shared_ptr<RLambdaMemberVarDecl> memberVarDecl;
    RTypeArgumentsPtr typeArgs;

public:
    void Accept(RLocVisitor& visitor) override { visitor.Visit(*this); }
};


// l[b], l is list
class RListIndexerLoc : public RLoc
{
    RLocPtr list;
    RLocPtr index;

public:
    void Accept(RLocVisitor& visitor) override { visitor.Visit(*this); }
};

// Instance가 null이면 static
class RStructMemberLoc : public RLoc
{
    RLocPtr instance;
    std::shared_ptr<RStructMemberVarDecl> memberVarDecl;
    RTypeArgumentsPtr typeArgs;

public:
    void Accept(RLocVisitor& visitor) override { visitor.Visit(*this); }
};

class RClassMemberLoc : public RLoc
{
    RLocPtr instance;
    std::shared_ptr<RClassMemberVarDecl> memberVarDecl;
    RTypeArgumentsPtr typeArgs;

public:
    void Accept(RLocVisitor& visitor) override { visitor.Visit(*this); }
};

class REnumElemMemberLoc : public RLoc
{
    RLocPtr instance;
    std::shared_ptr<REnumElemMemberVarDecl> memberVarDecl;
    RTypeArgumentsPtr typeArgs;

public:
    void Accept(RLocVisitor& visitor) override { visitor.Visit(*this); }
};

class RThisLoc : public RLoc
{
public:
    void Accept(RLocVisitor& visitor) override { visitor.Visit(*this); }
};

// dereference pointer, *
class RLocalDerefLoc : public RLoc
{
    RLocPtr innerLoc;
public:
    void Accept(RLocVisitor& visitor) override { visitor.Visit(*this); }
};

// dereference box pointer, *
class RBoxDerefLoc : public RLoc
{
    RLocPtr innerLoc;
public:
    void Accept(RLocVisitor& visitor) override { visitor.Visit(*this); }
};

// nullable value에서 value를 가져온다
class RNullableValueLoc : public RLoc
{
    RLocPtr loc;
public:
    void Accept(RLocVisitor& visitor) override { visitor.Visit(*this); }
};

}

