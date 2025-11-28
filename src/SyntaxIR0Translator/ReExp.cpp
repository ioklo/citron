#include "ReExp.h"
#include "RSymbol/RTypes.h"
#include "NSymbol/NLambdaVarDecl.h"
#include "NSymbol/NClassVarDecl.h"
#include "NSymbol/NStructVarDecl.h"
#include "NSymbol/NEnumElemVarDecl.h"
#include "MIR/MExp.h"

namespace Citron::SyntaxIR0Translator {

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

RType* ReExp_LambdaVar::GetType(RFactory& factory)
{
    return decl->GetDeclType(*typeArgs, factory);
}

ReExp_ClassVar::ReExp_ClassVar(RClassVarDecl* decl, RTypeArguments* typeArgs, bool hasExplicitInstance, ReExp* explicitInstance)
    : decl(decl), typeArgs(typeArgs), hasExplicitInstance(hasExplicitInstance), explicitInstance(explicitInstance)
{
}

RType* ReExp_ClassVar::GetType(RFactory& factory)
{
    return decl->GetDeclType(*typeArgs, factory);
}

ReExp_StructVar::ReExp_StructVar(RStructVarDecl* decl, RTypeArguments* typeArgs, bool hasExplicitInstance, ReExp* explicitInstance)
    : decl(decl), typeArgs(typeArgs), hasExplicitInstance(hasExplicitInstance), explicitInstance(explicitInstance)
{
}

RType* ReExp_StructVar::GetType(RFactory& factory)
{
    return decl->GetDeclType(*typeArgs, factory);
}

ReExp_EnumElemVar::ReExp_EnumElemVar(REnumElemVarDecl* decl, RTypeArguments* typeArgs, ReExp* instance)
    : decl(decl), typeArgs(typeArgs), instance(instance)
{
}

RType* ReExp_EnumElemVar::GetType(RFactory& factory)
{
    return decl->GetDeclType(*typeArgs, factory);
}

ReExp_LocalDeref::ReExp_LocalDeref(ReExp* target)
    : target(target)
{

}

RType* ReExp_LocalDeref::GetType(RFactory& factory)
{
    auto type = target->GetType(factory);

    // TODO: remove reinterpret cast. 어떻게?
    return ((RType_LocalPtr*)type)->innerType;
}

ReExp_BoxDeref::ReExp_BoxDeref(ReExp* target)
    : target(target)
{

}

RType* ReExp_BoxDeref::GetType(RFactory& factory)
{
    auto type = target->GetType(factory);

    // TODO: remove reinterpret cast
    return ((RType_BoxPtr*)type)->innerType;
}

ReExp_ListIndexer::ReExp_ListIndexer(ReExp* instance, ReExp* index, RType* itemType)
    : instance(instance), index(index), itemType(itemType)
{

}

ReExp_Else::ReExp_Else(MExp* mExp)
    : mExp(mExp)
{
}

RType* ReExp_Else::GetType(RFactory& factory)
{
    return mExp->GetType(factory);
}



}