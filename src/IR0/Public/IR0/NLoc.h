#pragma once
#include "IR0Config.h"

#include <variant>
#include <memory>
#include <string>

#include "RNames.h"

namespace Citron {

class RType;
class RTypeArguments;

class RTypeFactory;
class RLambdaVarDecl;
class RStructVarDecl;
class RClassVarDecl;
class REnumElemVarDecl;

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

class NExp;

class NLambdaVarDecl;

class NLocVisitor
{
public:
    virtual ~NLocVisitor() {}
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
    virtual ~NLoc() {}
    virtual void Accept(NLocVisitor& visitor) = 0;
    virtual RType* GetType(RTypeFactory& factory) = 0;
};

class NLoc_Temp : public NLoc
{
public:
    NExp* exp;

public:
    IR0_API NLoc_Temp(NExp* exp);
    void Accept(NLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RType* GetType(RTypeFactory& factory) override;
};

class NLoc_LocalVar : public NLoc
{
public:
    RName name;
    RType* declType;

public:
    IR0_API NLoc_LocalVar(const RName& name, RType* declType);
    void Accept(NLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RType* GetType(RTypeFactory& factory) override;
};

// only this member allowed, so no need this
class NLoc_LambdaVar : public NLoc
{
public:
    RLambdaVarDecl* decl;
    RTypeArguments* typeArgs;

public:
    IR0_API NLoc_LambdaVar(RLambdaVarDecl* decl, RTypeArguments* typeArgs);
    void Accept(NLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RType* GetType(RTypeFactory& factory) override;
};

// l[b], l is list
class NLoc_ListIndexer : public NLoc
{
public:
    NLoc* list;
    NLoc* index;
    RType* itemType;

public:
    IR0_API NLoc_ListIndexer(NLoc* list, NLoc* index, RType* itemType);
    void Accept(NLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RType* GetType(RTypeFactory& factory) override;
};

// Instance가 null이면 static
class NLoc_StructVar : public NLoc
{
public:
    NLoc* instance;
    RStructVarDecl* decl;
    RTypeArguments* typeArgs;

public:
    IR0_API NLoc_StructVar(NLoc* instance, RStructVarDecl* decl, RTypeArguments* typeArgs);
    void Accept(NLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RType* GetType(RTypeFactory& factory) override;
};

class NLoc_ClassVar : public NLoc
{
public:
    NLoc* instance;
    RClassVarDecl* decl;
    RTypeArguments* typeArgs;

public:
    IR0_API NLoc_ClassVar(NLoc* instance, RClassVarDecl* decl, RTypeArguments* typeArgs);
    void Accept(NLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RType* GetType(RTypeFactory& factory) override;
};

class NLoc_EnumElemVar : public NLoc
{
public:
    NLoc* instance;
    REnumElemVarDecl* decl;
    RTypeArguments* typeArgs;

public:
    IR0_API NLoc_EnumElemVar(NLoc* instance, REnumElemVarDecl* decl, RTypeArguments* typeArgs);
    void Accept(NLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RType* GetType(RTypeFactory& factory) override;
};

class NLoc_This : public NLoc
{
public:
    RType* type;

public:
    IR0_API NLoc_This(RType* type);
    void Accept(NLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RType* GetType(RTypeFactory& factory) override;
};

// dereference pointer, *
class NLoc_LocalDeref : public NLoc
{
public:
    NLoc* innerLoc;
public:
    IR0_API NLoc_LocalDeref(NLoc* innerLoc);
    void Accept(NLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RType* GetType(RTypeFactory& factory) override;
};

// dereference box pointer, *
class NLoc_BoxDeref : public NLoc
{
public:
    NLoc* innerLoc;

public:
    IR0_API NLoc_BoxDeref(NLoc* innerLoc);
    void Accept(NLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RType* GetType(RTypeFactory& factory) override;
};

// nullable value에서 value를 가져온다
class NLoc_NullableValue : public NLoc
{
public:
    NLoc* loc;
public:
    IR0_API NLoc_NullableValue(NLoc* loc);
    void Accept(NLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RType* GetType(RTypeFactory& factory) override;
};

}

