#pragma once
#include <variant>
#include <memory>
#include <string>

#include "RType.h"
#include "RNames.h"

namespace Citron {

class NLoc_Temp;
class NLoc_LocalVar;
class NLoc_LambdaVar;
class NLoc_ListIndexer;
class NLoc_StructVar;
class NLoc_ClassVar;
class NLoc_EnumElemVar;
class NLoc_This;
class NLoc_LocalDeref;
class NLoc_BoxDeref;
class NLoc_NullableValue;

class NLambdaVarDecl;
class RStructVarDecl;
class RClassVarDecl;
class REnumElemVarDecl;

using NExpPtr = std::shared_ptr<class NExp>;

class NLocVisitor
{
public:
    virtual ~NLocVisitor() { }
    virtual void Visit(NLoc_Temp& loc) = 0;
    virtual void Visit(NLoc_LocalVar& loc) = 0;
    virtual void Visit(NLoc_LambdaVar& loc) = 0;
    virtual void Visit(NLoc_ListIndexer& loc) = 0;
    virtual void Visit(NLoc_StructVar& loc) = 0;
    virtual void Visit(NLoc_ClassVar& loc) = 0;
    virtual void Visit(NLoc_EnumElemVar& loc) = 0;
    virtual void Visit(NLoc_This& loc) = 0;
    virtual void Visit(NLoc_LocalDeref& loc) = 0;
    virtual void Visit(NLoc_BoxDeref& loc) = 0;
    virtual void Visit(NLoc_NullableValue& loc) = 0;
};

class NLoc
{
public:
    virtual ~NLoc() { }
    virtual void Accept(NLocVisitor& visitor) = 0;
    virtual RTypePtr GetType(RTypeFactory& factory) = 0;
};

using NLocPtr = std::shared_ptr<NLoc>;

class NLoc_Temp : public NLoc
{
public:
    NExpPtr exp;
    
public:
    IR0_API NLoc_Temp(const NExpPtr& exp);
    IR0_API NLoc_Temp(NExpPtr&& exp);
    void Accept(NLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
};

class NLoc_LocalVar : public NLoc
{
public:
    RName name;
    RTypePtr declType;

public:
    IR0_API NLoc_LocalVar(const RName& name, const RTypePtr& declType);
    void Accept(NLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
};

// only this member allowed, so no need this
class NLoc_LambdaVar : public NLoc
{
public:
    std::shared_ptr<NLambdaVarDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    IR0_API NLoc_LambdaVar(const std::shared_ptr<NLambdaVarDecl>& decl, const RTypeArgumentsPtr& typeArgs);
    void Accept(NLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
};

// l[b], l is list
class NLoc_ListIndexer : public NLoc
{
public:
    NLocPtr list;
    NLocPtr index;
    RTypePtr itemType;

public:
    IR0_API NLoc_ListIndexer(NLocPtr&& list, const NLocPtr& index, const RTypePtr& itemType);
    void Accept(NLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
};

// Instance가 null이면 static
class NLoc_StructVar : public NLoc
{
public:
    NLocPtr instance;
    std::shared_ptr<RStructVarDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    IR0_API NLoc_StructVar(const NLocPtr& instance, const std::shared_ptr<RStructVarDecl>& decl, const RTypeArgumentsPtr& typeArgs);
    void Accept(NLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
};

class NLoc_ClassVar : public NLoc
{
public:
    NLocPtr instance;
    std::shared_ptr<RClassVarDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    NLoc_ClassVar(NLocPtr&& instance, const std::shared_ptr<RClassVarDecl>& decl, const RTypeArgumentsPtr& typeArgs);
    void Accept(NLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
};

class NLoc_EnumElemVar : public NLoc
{
public:
    NLocPtr instance;
    std::shared_ptr<REnumElemVarDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    IR0_API NLoc_EnumElemVar(const NLocPtr& instance, std::shared_ptr<REnumElemVarDecl>& decl, const RTypeArgumentsPtr& typeArgs);
    void Accept(NLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
};

class NLoc_This : public NLoc
{
public:
    RTypePtr type;

public:
    IR0_API NLoc_This(RTypePtr type);
    void Accept(NLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
};

// dereference pointer, *
class NLoc_LocalDeref : public NLoc
{
public:
    NLocPtr innerLoc;
public:
    IR0_API NLoc_LocalDeref(NLocPtr&& innerLoc);
    void Accept(NLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
};

// dereference box pointer, *
class NLoc_BoxDeref : public NLoc
{
public:
    NLocPtr innerLoc;

public:
    IR0_API NLoc_BoxDeref(NLocPtr&& innerLoc);
    void Accept(NLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
};

// nullable value에서 value를 가져온다
class NLoc_NullableValue : public NLoc
{
public:
    NLocPtr loc;
public:
    IR0_API NLoc_NullableValue(const NLocPtr& loc);
    void Accept(NLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
};

}

