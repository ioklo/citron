#include "MLoc.h"

#include "Infra/Exceptions.h"

#include "RSymbol/RStructVarDecl.h"
#include "RSymbol/RClassVarDecl.h"
#include "RSymbol/REnumElemVarDecl.h"
#include "RSymbol/RTypes.h"

#include "MExp.h"
#include "NSymbol/NLambdaVarDecl.h"

namespace Citron {

void MLoc_Materialize::Accept(MLocVisitor& visitor) { visitor.Visit(this); }
void MLoc_LocalVar::Accept(MLocVisitor& visitor) { visitor.Visit(this); }
void MLoc_LocalRef::Accept(MLocVisitor& visitor) { visitor.Visit(this); }
void MLoc_LambdaVar::Accept(MLocVisitor& visitor) { visitor.Visit(this); }
void MLoc_ListIndexer::Accept(MLocVisitor& visitor) { visitor.Visit(this); }
void MLoc_StructVar::Accept(MLocVisitor& visitor) { visitor.Visit(this); }
void MLoc_ClassVar::Accept(MLocVisitor& visitor) { visitor.Visit(this); }
void MLoc_EnumElemVar::Accept(MLocVisitor& visitor) { visitor.Visit(this); }
void MLoc_This::Accept(MLocVisitor& visitor) { visitor.Visit(this); }
void MLoc_PtrDeref::Accept(MLocVisitor& visitor) { visitor.Visit(this); }
void MLoc_SharedDeref::Accept(MLocVisitor& visitor) { visitor.Visit(this); }
void MLoc_NullableValue::Accept(MLocVisitor& visitor) { visitor.Visit(this); }

MLoc_Materialize::MLoc_Materialize(MCreate&& create)
    : create{move(create)}
{
}

RType* MLoc_Materialize::GetType()
{
    return Citron::GetType(create);
}

MLoc_LocalVar::MLoc_LocalVar(const RName& name, RType* declType)
    : name{name}, declType{declType}
{
}

RType* MLoc_LocalVar::GetType()
{
    return declType;
}

MLoc_LocalRef::MLoc_LocalRef(const RName& name, RType* declType)
    : name{name}, declType{declType}
{
}

RType* MLoc_LocalRef::GetType()
{
    return declType;
}


MLoc_LambdaVar::MLoc_LambdaVar(RLambdaVarDecl* decl, RTypeArguments* typeArgs)
    : decl(decl), typeArgs(typeArgs)
{
}

RType* MLoc_LambdaVar::GetType()
{
    return decl->GetDeclType(*typeArgs);
}

MLoc_ListIndexer::MLoc_ListIndexer(MLoc* list, MLoc* index, RType* itemType)
    : list{list}, index(index), itemType(itemType)
{
}

RType* MLoc_ListIndexer::GetType()
{
    return itemType;
}

MLoc_StructVar::MLoc_StructVar(MLoc* instance, RStructVarDecl* decl, RTypeArguments* typeArgs)
    : instance(instance), decl(decl), typeArgs(typeArgs)
{
}

RType* MLoc_StructVar::GetType()
{
    return decl->GetDeclType(*typeArgs);

}

MLoc_ClassVar::MLoc_ClassVar(MLoc* instance, RClassVarDecl* decl, RTypeArguments* typeArgs)
    : instance{instance}, decl(decl), typeArgs(typeArgs)
{
}

RType* MLoc_ClassVar::GetType()
{
    return decl->GetDeclType(*typeArgs);
}

MLoc_EnumElemVar::MLoc_EnumElemVar(MLoc* instance, REnumElemVarDecl* decl, RTypeArguments* typeArgs)
    : instance(instance), decl(decl), typeArgs(typeArgs)
{
}

RType* MLoc_EnumElemVar::GetType()
{
    return decl->GetDeclType(*typeArgs);
}

MLoc_This::MLoc_This(RType* type)
    : type{type}
{
}

RType* MLoc_This::GetType()
{
    return type;
}

MLoc_PtrDeref::MLoc_PtrDeref(MLoc* innerLoc)
    : innerLoc{innerLoc}
{
}

RType* MLoc_PtrDeref::GetType()
{
    auto* type = innerLoc->GetType();
    
    if (auto* ptrType = dynamic_cast<RType_Ptr*>(type))
        return ptrType->innerType;

    // 에러, 어떻게 해야할지 생각해본다
    throw NotImplementedException();
}

MLoc_SharedDeref::MLoc_SharedDeref(MLoc* innerLoc)
    : innerLoc{innerLoc}
{
}

RType* MLoc_SharedDeref::GetType()
{
    auto type = innerLoc->GetType();

    if (auto* boxType = dynamic_cast<RType_Box*>(type))
        return boxType->innerType;

    // 에러, 어떻게 해야할지 생각해본다
    throw NotImplementedException();
}

MLoc_NullableValue::MLoc_NullableValue(MLoc* loc)
    : loc(loc)
{

}

RType* MLoc_NullableValue::GetType()
{
    auto* type = loc->GetType();

    if (auto* nullableType = dynamic_cast<RType_NullableValue*>(type))
        return nullableType->innerType;

    // 에러, 어떻게 해야할지 생각해본다
    throw NotImplementedException();
}


}