#include "NLoc.h"

#include "Infra/Exceptions.h"

#include "RStructVarDecl.h"
#include "RClassVarDecl.h"
#include "REnumElemVarDecl.h"
#include "RTypes.h"

#include "NExp.h"
#include "NLambdaVarDecl.h"

namespace Citron {

NLoc_Temp::NLoc_Temp(NExp* exp)
    : exp{exp}
{
}

RType* NLoc_Temp::GetType(RTypeFactory& factory)
{
    return exp->GetType(factory);
}

NLoc_LocalVar::NLoc_LocalVar(const RName& name, RType* declType)
    : name(name), declType(declType)
{

}

RType* NLoc_LocalVar::GetType(RTypeFactory& factory)
{
    return declType;
}

NLoc_LambdaVar::NLoc_LambdaVar(RLambdaVarDecl* decl, RTypeArguments* typeArgs)
    : decl(decl), typeArgs(typeArgs)
{
}

RType* NLoc_LambdaVar::GetType(RTypeFactory& factory)
{
    return decl->GetDeclType(*typeArgs, factory);
}

NLoc_ListIndexer::NLoc_ListIndexer(NLoc* list, NLoc* index, RType* itemType)
    : list{list}, index(index), itemType(itemType)
{
}

RType* NLoc_ListIndexer::GetType(RTypeFactory& factory)
{
    return itemType;
}

NLoc_StructVar::NLoc_StructVar(NLoc* instance, RStructVarDecl* decl, RTypeArguments* typeArgs)
    : instance(instance), decl(decl), typeArgs(typeArgs)
{
}

RType* NLoc_StructVar::GetType(RTypeFactory& factory)
{
    return decl->GetDeclType(*typeArgs, factory);

}

NLoc_ClassVar::NLoc_ClassVar(NLoc* instance, RClassVarDecl* decl, RTypeArguments* typeArgs)
    : instance{instance}, decl(decl), typeArgs(typeArgs)
{
}


RType* NLoc_ClassVar::GetType(RTypeFactory& factory)
{
    return decl->GetDeclType(*typeArgs, factory);
}

NLoc_EnumElemVar::NLoc_EnumElemVar(NLoc* instance, REnumElemVarDecl* decl, RTypeArguments* typeArgs)
    : instance(instance), decl(decl), typeArgs(typeArgs)
{
}

RType* NLoc_EnumElemVar::GetType(RTypeFactory& factory)
{
    return decl->GetDeclType(*typeArgs, factory);
}

NLoc_This::NLoc_This(RType* type)
    : type{type}
{
}

RType* NLoc_This::GetType(RTypeFactory& factory)
{
    return type;
}

NLoc_LocalDeref::NLoc_LocalDeref(NLoc* innerLoc)
    : innerLoc{innerLoc}
{
}

RType* NLoc_LocalDeref::GetType(RTypeFactory& factory)
{
    auto type = innerLoc->GetType(factory);
    
    if (auto* localPtrType = dynamic_cast<RType_LocalPtr*>(type))
        return localPtrType->innerType;

    // 에러, 어떻게 해야할지 생각해본다
    throw NotImplementedException();
}

NLoc_BoxDeref::NLoc_BoxDeref(NLoc* innerLoc)
    : innerLoc{innerLoc}
{
}

RType* NLoc_BoxDeref::GetType(RTypeFactory& factory)
{
    auto type = innerLoc->GetType(factory);

    if (auto* boxPtrType = dynamic_cast<RType_BoxPtr*>(type))
        return boxPtrType->innerType;

    // 에러, 어떻게 해야할지 생각해본다
    throw NotImplementedException();
}

NLoc_NullableValue::NLoc_NullableValue(NLoc* loc)
    : loc(loc)
{

}

RType* NLoc_NullableValue::GetType(RTypeFactory& factory)
{
    auto type = loc->GetType(factory);

    if (auto* nullableType = dynamic_cast<RType_NullableValue*>(type))
        return nullableType->innerType;

    // 에러, 어떻게 해야할지 생각해본다
    throw NotImplementedException();
}

}