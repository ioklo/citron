#include "ReExp.h"
#include "RSymbol/RTypes.h"
#include "NSymbol/NLambdaVarDecl.h"
#include "NSymbol/NClassVarDecl.h"
#include "NSymbol/NStructVarDecl.h"
#include "NSymbol/NEnumElemVarDecl.h"
#include "MIR/MExp.h"

namespace Citron {

ReExp_ThisVar::ReExp_ThisVar(RType* type)
    : type(type)
{
}

ReExp_LocalVar::ReExp_LocalVar(RType* type, const RName& name)
    : type(type), name(name)
{
}

ReExp_LambdaVar::ReExp_LambdaVar(NLambdaVarDecl* decl, RTypeArguments* typeArgs)
    : decl(decl), typeArgs(typeArgs)
{
}

RType* ReExp_LambdaVar::GetType()
{
    return decl->GetDeclType(*typeArgs);
}

ReExp_ClassVar::ReExp_ClassVar(RClassVarDecl* decl, RTypeArguments* typeArgs, bool hasExplicitInstance, ReExp* explicitInstance)
    : decl(decl), typeArgs(typeArgs), hasExplicitInstance(hasExplicitInstance), explicitInstance(explicitInstance)
{
}

RType* ReExp_ClassVar::GetType()
{
    return decl->GetDeclType(*typeArgs);
}

ReExp_StructVar::ReExp_StructVar(RStructVarDecl* decl, RTypeArguments* typeArgs, bool hasExplicitInstance, ReExp* explicitInstance)
    : decl(decl), typeArgs(typeArgs), hasExplicitInstance(hasExplicitInstance), explicitInstance(explicitInstance)
{
}

RType* ReExp_StructVar::GetType()
{
    return decl->GetDeclType(*typeArgs);
}

ReExp_EnumElemVar::ReExp_EnumElemVar(REnumElemVarDecl* decl, RTypeArguments* typeArgs, ReExp* instance)
    : decl(decl), typeArgs(typeArgs), instance(instance)
{
}

RType* ReExp_EnumElemVar::GetType()
{
    return decl->GetDeclType(*typeArgs);
}

ReExp_Deref::ReExp_Deref(ReExp* target)
    : target(target)
{

}

RType* ReExp_Deref::GetType()
{
    auto type = target->GetType();

    // TODO: remove reinterpret cast. 어떻게?
    return ((RType_Ptr*)type)->innerType;
}

ReExp_BoxDeref::ReExp_BoxDeref(ReExp* target)
    : target(target)
{

}

RType* ReExp_BoxDeref::GetType()
{
    auto type = target->GetType();

    // TODO: remove reinterpret cast
    return ((RType_Box*)type)->innerType;
}

ReExp_ListIndexer::ReExp_ListIndexer(ReExp* instance, ReExp* index, RType* itemType)
    : instance(instance), index(index), itemType(itemType)
{

}

ReExp_Else::ReExp_Else(MExp* mExp)
    : mExp(mExp)
{
}

RType* ReExp_Else::GetType()
{
    return mExp->GetType();
}



}