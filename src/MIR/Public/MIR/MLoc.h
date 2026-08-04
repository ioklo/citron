#pragma once
#include "MIRConfig.h"

#include "Infra/Ref.h"
#include "RSymbol/RNames.h"
#include "RSymbol/RAppliedDecl.h"
#include "MCreate.h"
#include "MRead.h"

namespace Citron {

class RType;

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
    MLoc_Materialize(MCreate&& create)
        : create{std::move(create)}
    { }
    MIR_API void Accept(MLocVisitor& visitor) override;
};

struct MLoc_LocalVar : MLoc
{
    RName name;
    RType* declType;

    MLoc_LocalVar(TakeRef<RName> name, RType* declType)
        : name{name.Take()}, declType{declType}
    { }
    MIR_API void Accept(MLocVisitor& visitor) override;
};

struct MLoc_LocalRef : MLoc
{
    RName name;
    RType* declType;

    MLoc_LocalRef(TakeRef<RName> name, RType* declType)
        : name{name.Take()}, declType{declType}
    { }
    MIR_API void Accept(MLocVisitor& visitor) override;
};

// only this member allowed, so no need this
struct MLoc_LambdaVar : MLoc
{
    RAppliedDecl<RLambdaVarDecl> appliedDecl;
    MLoc_LambdaVar(RAppliedDecl<RLambdaVarDecl>&& appliedDecl)
        : appliedDecl{std::move(appliedDecl)}
    { }
    MIR_API void Accept(MLocVisitor& visitor) override;
};

// l[b], l is list
struct MLoc_ListIndexer : MLoc
{
    MRead_Loc list; // NBC
    MRead index; // BC
    RType* itemType;

    MLoc_ListIndexer(MRead_Loc&& list, MRead&& index, RType* itemType)
        : list{std::move(list)}, index{std::move(index)}, itemType{itemType}
    { }
    MIR_API void Accept(MLocVisitor& visitor) override;
};

// Instance가 null이면 static
struct MLoc_StructVar : MLoc
{
    MLoc* instance;
    RAppliedDecl<RStructVarDecl> appliedDecl;

    MLoc_StructVar(MLoc* instance, TakeRef<RAppliedDecl<RStructVarDecl>> appliedDecl)
        : instance{instance}, appliedDecl{appliedDecl.Take()}
    { }
    MIR_API void Accept(MLocVisitor& visitor) override;
};

struct MLoc_ClassVar : MLoc
{
    MLoc* instance;
    RAppliedDecl<RClassVarDecl> appliedDecl;

    MLoc_ClassVar(MLoc* instance, TakeRef<RAppliedDecl<RClassVarDecl>> appliedDecl)
        : instance{instance}, appliedDecl{appliedDecl.Take()}
    { }
    MIR_API void Accept(MLocVisitor& visitor) override;
};

struct MLoc_EnumElemVar : MLoc
{
    MLoc* instance;
    RAppliedDecl<REnumElemVarDecl> appliedDecl;

    MLoc_EnumElemVar(MLoc* instance, TakeRef<RAppliedDecl<REnumElemVarDecl>> appliedDecl)
        : instance{instance}, appliedDecl{appliedDecl.Take()}
    { }
    MIR_API void Accept(MLocVisitor& visitor) override;
};

struct MLoc_This : MLoc
{
    RType* type;

    MLoc_This(RType* type)
        : type{type}
    { }
    MIR_API void Accept(MLocVisitor& visitor) override;
};

// dereference pointer, *
struct MLoc_PtrDeref : MLoc
{
    MRead srcPtr; // ptr이니까 BC

    MLoc_PtrDeref(MRead&& srcPtr)
        : srcPtr{std::move(srcPtr)}
    { }
    MIR_API void Accept(MLocVisitor& visitor) override;
};

// dereference shared pointer, *
struct MLoc_SharedDeref : MLoc
{
    MRead_Loc srcShared; // shared이니까 NBC
    
    template<typename T>
    MLoc_SharedDeref(T&& srcShared)
        : srcShared{std::forward<T>(srcShared)}
    { }
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

