#pragma once
#include "MIRConfig.h"

#include <variant>
#include <string>

#include "RSymbol/RNames.h"

namespace Citron {

class RType;
class RTypeArguments;

class RFactory;
class RLambdaVarDecl;
class RStructVarDecl;
class RClassVarDecl;
class REnumElemVarDecl;

class MLoc_Temp;
class MLoc_LocalVar;
class MLoc_LambdaVar;
class MLoc_ListIndexer;
class MLoc_StructVar;
class MLoc_ClassVar;
class MLoc_EnumElemVar;
class MLoc_This;
class MLoc_LocalDeref;
class MLoc_BoxDeref;
class MLoc_NullableValue;

class MExp;

class NLambdaVarDecl;

class MLocVisitor
{
public:
    virtual ~MLocVisitor() {}
    virtual void Visit(MLoc_Temp* loc) = 0;
    virtual void Visit(MLoc_LocalVar* loc) = 0;
    virtual void Visit(MLoc_LambdaVar* loc) = 0;
    virtual void Visit(MLoc_ListIndexer* loc) = 0;
    virtual void Visit(MLoc_StructVar* loc) = 0;
    virtual void Visit(MLoc_ClassVar* loc) = 0;
    virtual void Visit(MLoc_EnumElemVar* loc) = 0;
    virtual void Visit(MLoc_This* loc) = 0;
    virtual void Visit(MLoc_LocalDeref* loc) = 0;
    virtual void Visit(MLoc_BoxDeref* loc) = 0;
    virtual void Visit(MLoc_NullableValue* loc) = 0;
};

class MLoc
{
public:
    virtual ~MLoc() {}
    virtual void Accept(MLocVisitor& visitor) = 0;
    virtual RType* GetType(RFactory& factory) = 0;
};

class MLoc_Temp : public MLoc
{
public:
    MExp* exp;

public:
    MIR_API MLoc_Temp(MExp* exp);
    void Accept(MLocVisitor& visitor) override { visitor.Visit(this); }
    MIR_API RType* GetType(RFactory& factory) override;
};

class MLoc_LocalVar : public MLoc
{
public:
    RName name;
    RType* declType;

public:
    MIR_API MLoc_LocalVar(const RName& name, RType* declType);
    void Accept(MLocVisitor& visitor) override { visitor.Visit(this); }
    MIR_API RType* GetType(RFactory& factory) override;
};

// only this member allowed, so no need this
class MLoc_LambdaVar : public MLoc
{
public:
    RLambdaVarDecl* decl;
    RTypeArguments* typeArgs;

public:
    MIR_API MLoc_LambdaVar(RLambdaVarDecl* decl, RTypeArguments* typeArgs);
    void Accept(MLocVisitor& visitor) override { visitor.Visit(this); }
    MIR_API RType* GetType(RFactory& factory) override;
};

// l[b], l is list
class MLoc_ListIndexer : public MLoc
{
public:
    MLoc* list;
    MLoc* index;
    RType* itemType;

public:
    MIR_API MLoc_ListIndexer(MLoc* list, MLoc* index, RType* itemType);
    void Accept(MLocVisitor& visitor) override { visitor.Visit(this); }
    MIR_API RType* GetType(RFactory& factory) override;
};

// Instance가 null이면 static
class MLoc_StructVar : public MLoc
{
public:
    MLoc* instance;
    RStructVarDecl* decl;
    RTypeArguments* typeArgs;

public:
    MIR_API MLoc_StructVar(MLoc* instance, RStructVarDecl* decl, RTypeArguments* typeArgs);
    void Accept(MLocVisitor& visitor) override { visitor.Visit(this); }
    MIR_API RType* GetType(RFactory& factory) override;
};

class MLoc_ClassVar : public MLoc
{
public:
    MLoc* instance;
    RClassVarDecl* decl;
    RTypeArguments* typeArgs;

public:
    MIR_API MLoc_ClassVar(MLoc* instance, RClassVarDecl* decl, RTypeArguments* typeArgs);
    void Accept(MLocVisitor& visitor) override { visitor.Visit(this); }
    MIR_API RType* GetType(RFactory& factory) override;
};

class MLoc_EnumElemVar : public MLoc
{
public:
    MLoc* instance;
    REnumElemVarDecl* decl;
    RTypeArguments* typeArgs;

public:
    MIR_API MLoc_EnumElemVar(MLoc* instance, REnumElemVarDecl* decl, RTypeArguments* typeArgs);
    void Accept(MLocVisitor& visitor) override { visitor.Visit(this); }
    MIR_API RType* GetType(RFactory& factory) override;
};

class MLoc_This : public MLoc
{
public:
    RType* type;

public:
    MIR_API MLoc_This(RType* type);
    void Accept(MLocVisitor& visitor) override { visitor.Visit(this); }
    MIR_API RType* GetType(RFactory& factory) override;
};

// dereference pointer, *
class MLoc_LocalDeref : public MLoc
{
public:
    MLoc* innerLoc;
public:
    MIR_API MLoc_LocalDeref(MLoc* innerLoc);
    void Accept(MLocVisitor& visitor) override { visitor.Visit(this); }
    MIR_API RType* GetType(RFactory& factory) override;
};

// dereference box pointer, *
class MLoc_BoxDeref : public MLoc
{
public:
    MLoc* innerLoc;

public:
    MIR_API MLoc_BoxDeref(MLoc* innerLoc);
    void Accept(MLocVisitor& visitor) override { visitor.Visit(this); }
    MIR_API RType* GetType(RFactory& factory) override;
};

// nullable value에서 value를 가져온다
class MLoc_NullableValue : public MLoc
{
public:
    MLoc* loc;
public:
    MIR_API MLoc_NullableValue(MLoc* loc);
    void Accept(MLocVisitor& visitor) override { visitor.Visit(this); }
    MIR_API RType* GetType(RFactory& factory) override;
};

}

