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

RType* MLoc_Temp::GetType(RFactory& factory)
{
    return exp->GetType(factory);
}

MLoc_LocalVar::MLoc_LocalVar(const RName& name, RType* declType)
    : name(name), declType(declType)
{

}

RType* MLoc_LocalVar::GetType(RFactory& factory)
{
    return declType;
}

MLoc_LambdaVar::MLoc_LambdaVar(RLambdaVarDecl* decl, RTypeArguments* typeArgs)
    : decl(decl), typeArgs(typeArgs)
{
}

RType* MLoc_LambdaVar::GetType(RFactory& factory)
{
    return decl->GetDeclType(*typeArgs, factory);
}

MLoc_ListIndexer::MLoc_ListIndexer(MLoc* list, MLoc* index, RType* itemType)
    : list{list}, index(index), itemType(itemType)
{
}

RType* MLoc_ListIndexer::GetType(RFactory& factory)
{
    return itemType;
}

MLoc_StructVar::MLoc_StructVar(MLoc* instance, RStructVarDecl* decl, RTypeArguments* typeArgs)
    : instance(instance), decl(decl), typeArgs(typeArgs)
{
}

RType* MLoc_StructVar::GetType(RFactory& factory)
{
    return decl->GetDeclType(*typeArgs, factory);

}

MLoc_ClassVar::MLoc_ClassVar(MLoc* instance, RClassVarDecl* decl, RTypeArguments* typeArgs)
    : instance{instance}, decl(decl), typeArgs(typeArgs)
{
}


RType* MLoc_ClassVar::GetType(RFactory& factory)
{
    return decl->GetDeclType(*typeArgs, factory);
}

MLoc_EnumElemVar::MLoc_EnumElemVar(MLoc* instance, REnumElemVarDecl* decl, RTypeArguments* typeArgs)
    : instance(instance), decl(decl), typeArgs(typeArgs)
{
}

RType* MLoc_EnumElemVar::GetType(RFactory& factory)
{
    return decl->GetDeclType(*typeArgs, factory);
}

MLoc_This::MLoc_This(RType* type)
    : type{type}
{
}

RType* MLoc_This::GetType(RFactory& factory)
{
    return type;
}

MLoc_LocalDeref::MLoc_LocalDeref(MLoc* innerLoc)
    : innerLoc{innerLoc}
{
}

RType* MLoc_LocalDeref::GetType(RFactory& factory)
{
    auto type = innerLoc->GetType(factory);
    
    if (auto* localPtrType = dynamic_cast<RType_LocalPtr*>(type))
        return localPtrType->innerType;

    // 에러, 어떻게 해야할지 생각해본다
    throw NotImplementedException();
}

MLoc_BoxDeref::MLoc_BoxDeref(MLoc* innerLoc)
    : innerLoc{innerLoc}
{
}

RType* MLoc_BoxDeref::GetType(RFactory& factory)
{
    auto type = innerLoc->GetType(factory);

    if (auto* boxPtrType = dynamic_cast<RType_BoxPtr*>(type))
        return boxPtrType->innerType;

    // 에러, 어떻게 해야할지 생각해본다
    throw NotImplementedException();
}

MLoc_NullableValue::MLoc_NullableValue(MLoc* loc)
    : loc(loc)
{

}

RType* MLoc_NullableValue::GetType(RFactory& factory)
{
    auto type = loc->GetType(factory);

    if (auto* nullableType = dynamic_cast<RType_NullableValue*>(type))
        return nullableType->innerType;

    // 에러, 어떻게 해야할지 생각해본다
    throw NotImplementedException();
}

}