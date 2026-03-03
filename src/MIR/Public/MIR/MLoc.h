#pragma once
#include "MIRConfig.h"

#include <variant>
#include <string>
#include <optional>

#include "RSymbol/RNames.h"
#include "MCreate.h"

namespace Citron {

class RType;
class RTypeArguments;

class RFactory;
class RLambdaVarDecl;
class RStructVarDecl;
class RClassVarDecl;
class REnumElemVarDecl;

class NLambdaVarDecl;

struct MLocVisitor;

class MLoc
{
public:
    virtual ~MLoc() {}
    virtual void Accept(MLocVisitor& visitor) = 0;
    virtual RType* GetType() = 0;
};

class MLoc_Materialize : public MLoc
{
public:
    MCreate create;

public:
    MIR_API MLoc_Materialize(MCreate&& create);
    MIR_API void Accept(MLocVisitor& visitor) override;
    MIR_API RType* GetType() override;
};

class MLoc_LocalVar : public MLoc
{
public:
    RName name;
    RType* declType;

public:
    MIR_API MLoc_LocalVar(const RName& name, RType* declType);
    MIR_API void Accept(MLocVisitor& visitor) override;
    MIR_API RType* GetType() override;
};

class MLoc_LocalRef : public MLoc
{
public:
    RName name;
    RType* declType;

public:
    MIR_API MLoc_LocalRef(const RName& name, RType* declType);
    MIR_API void Accept(MLocVisitor& visitor) override;
    MIR_API RType* GetType() override;
};

// only this member allowed, so no need this
class MLoc_LambdaVar : public MLoc
{
public:
    RLambdaVarDecl* decl;
    RTypeArguments* typeArgs;

public:
    MIR_API MLoc_LambdaVar(RLambdaVarDecl* decl, RTypeArguments* typeArgs);
    MIR_API void Accept(MLocVisitor& visitor) override;
    MIR_API RType* GetType() override;
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
    MIR_API void Accept(MLocVisitor& visitor) override;
    MIR_API RType* GetType() override;
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
    MIR_API void Accept(MLocVisitor& visitor) override;
    MIR_API RType* GetType() override;
};

class MLoc_ClassVar : public MLoc
{
public:
    MLoc* instance;
    RClassVarDecl* decl;
    RTypeArguments* typeArgs;

public:
    MIR_API MLoc_ClassVar(MLoc* instance, RClassVarDecl* decl, RTypeArguments* typeArgs);
    MIR_API void Accept(MLocVisitor& visitor) override;
    MIR_API RType* GetType() override;
};

class MLoc_EnumElemVar : public MLoc
{
public:
    MLoc* instance;
    REnumElemVarDecl* decl;
    RTypeArguments* typeArgs;

public:
    MIR_API MLoc_EnumElemVar(MLoc* instance, REnumElemVarDecl* decl, RTypeArguments* typeArgs);
    MIR_API void Accept(MLocVisitor& visitor) override;
    MIR_API RType* GetType() override;
};

class MLoc_This : public MLoc
{
public:
    RType* type;

public:
    MIR_API MLoc_This(RType* type);
    MIR_API void Accept(MLocVisitor& visitor) override;
    MIR_API RType* GetType() override;
};

// dereference pointer, *
class MLoc_PtrDeref : public MLoc
{
public:
    MLoc* innerLoc;
public:
    MIR_API MLoc_PtrDeref(MLoc* innerLoc);
    MIR_API void Accept(MLocVisitor& visitor) override;
    MIR_API RType* GetType() override;
};

// dereference box pointer, *
class MLoc_SharedDeref : public MLoc
{
public:
    MLoc* innerLoc;

public:
    MIR_API MLoc_SharedDeref(MLoc* innerLoc);
    MIR_API void Accept(MLocVisitor& visitor) override;
    MIR_API RType* GetType() override;
};

// nullable value에서 value를 가져온다
class MLoc_NullableValue : public MLoc
{
public:
    MLoc* loc;
public:
    MIR_API MLoc_NullableValue(MLoc* loc);
    MIR_API void Accept(MLocVisitor& visitor) override;
    MIR_API RType* GetType() override;
};

}

#include "MLocVisitor.g.h"

