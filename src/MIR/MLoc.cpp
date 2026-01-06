#include "MLoc.h"

#include "Infra/Exceptions.h"

#include "RSymbol/RStructVarDecl.h"
#include "RSymbol/RClassVarDecl.h"
#include "RSymbol/REnumElemVarDecl.h"
#include "RSymbol/RTypes.h"

#include "MExp.h"
#include "NSymbol/NLambdaVarDecl.h"

namespace Citron {

MLoc_Temp::MLoc_Temp(MExp* exp)
    : exp{exp}
{
}

RType* MLoc_Temp::GetType()
{
    return exp->GetType();
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

MLoc_BoxDeref::MLoc_BoxDeref(MLoc* innerLoc)
    : innerLoc{innerLoc}
{
}

RType* MLoc_BoxDeref::GetType()
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