#pragma once
#include "MIRConfig.h"

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

struct MLocVisitor;

struct MLoc
{
public:
    virtual ~MLoc() {}
    virtual void Accept(MLocVisitor& visitor) = 0;
};

struct MLoc_Materialize : MLoc
{
    MCreate create;
    MIR_API void Accept(MLocVisitor& visitor) override;
};

struct MLoc_LocalVar : MLoc
{
    RName name;
    RType* declType;

    MIR_API void Accept(MLocVisitor& visitor) override;
};

struct MLoc_LocalRef : MLoc
{
    RName name;
    RType* declType;
    MIR_API void Accept(MLocVisitor& visitor) override;
};

// only this member allowed, so no need this
struct MLoc_LambdaVar : MLoc
{
    RLambdaVarDecl* decl;
    RTypeArguments* typeArgs;
    MIR_API void Accept(MLocVisitor& visitor) override;
};

// l[b], l is list
struct MLoc_ListIndexer : MLoc
{
    MLoc* list;
    MLoc* index;
    RType* itemType;

    MIR_API void Accept(MLocVisitor& visitor) override;
};

// Instance가 null이면 static
struct MLoc_StructVar : MLoc
{
    MLoc* instance;
    RStructVarDecl* decl;
    RTypeArguments* typeArgs;

    MIR_API void Accept(MLocVisitor& visitor) override;
};

struct MLoc_ClassVar : MLoc
{
    MLoc* instance;
    RClassVarDecl* decl;
    RTypeArguments* typeArgs;

    MIR_API void Accept(MLocVisitor& visitor) override;
};

struct MLoc_EnumElemVar : MLoc
{
    MLoc* instance;
    REnumElemVarDecl* decl;
    RTypeArguments* typeArgs;

    MIR_API void Accept(MLocVisitor& visitor) override;
};

struct MLoc_This : MLoc
{
    RType* type;

    MIR_API void Accept(MLocVisitor& visitor) override;
};

// dereference pointer, *
struct MLoc_PtrDeref : MLoc
{
    MLoc* innerLoc;

    MIR_API void Accept(MLocVisitor& visitor) override;
};

// dereference box pointer, *
struct MLoc_SharedDeref : MLoc
{
    MLoc* innerLoc;

    MIR_API void Accept(MLocVisitor& visitor) override;
};

// nullable value에서 value를 가져온다
struct MLoc_NullableValue : MLoc
{
    MLoc* loc;

    MIR_API void Accept(MLocVisitor& visitor) override;
};

MIR_API RType* GetType(MLoc* loc, RFactory* rFactory);

}

#include "MLocVisitor.g.h"

