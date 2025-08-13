#include "ReExp.h"
#include "IR0/RTypes.h"
#include "IR0/NLambdaVarDecl.h"
#include "IR0/NExp.h"
#include "IR0/NClassVarDecl.h"
#include "IR0/NStructVarDecl.h"
#include "IR0/NEnumElemVarDecl.h"

namespace Citron::SyntaxIR0Translator {

ReExp_ThisVar::ReExp_ThisVar(RType* type)
    : type(type)
{
}

ReExp_LocalVar::ReExp_LocalVar(RType* type, const std::string& name)
    : type(type), name(name)
{
}

ReExp_LambdaVar::ReExp_LambdaVar(NLambdaVarDecl* decl, RTypeArguments* typeArgs)
    : decl(decl), typeArgs(typeArgs)
{
}

RType* ReExp_LambdaVar::GetType(IR0Factory& factory)
{
    return decl->GetDeclType(*typeArgs, factory);
}

ReExp_ClassVar::ReExp_ClassVar(RClassVarDecl* decl, RTypeArguments* typeArgs, bool hasExplicitInstance, const ReExpPtr& explicitInstance)
    : decl(decl), typeArgs(typeArgs), hasExplicitInstance(hasExplicitInstance), explicitInstance(explicitInstance)
{
}

RType* ReExp_ClassVar::GetType(IR0Factory& factory)
{
    return decl->GetDeclType(*typeArgs, factory);
}

ReExp_StructVar::ReExp_StructVar(RStructVarDecl* decl, RTypeArguments* typeArgs, bool hasExplicitInstance, const ReExpPtr& explicitInstance)
    : decl(decl), typeArgs(typeArgs), hasExplicitInstance(hasExplicitInstance), explicitInstance(explicitInstance)
{
}

RType* ReExp_StructVar::GetType(IR0Factory& factory)
{
    return decl->GetDeclType(*typeArgs, factory);
}

ReExp_EnumElemVar::ReExp_EnumElemVar(REnumElemVarDecl* decl, RTypeArguments* typeArgs, const ReExpPtr& instance)
    : decl(decl), typeArgs(typeArgs), instance(instance)
{
}

RType* ReExp_EnumElemVar::GetType(IR0Factory& factory)
{
    return decl->GetDeclType(*typeArgs, factory);
}

ReExp_LocalDeref::ReExp_LocalDeref(const ReExpPtr& target)
    : target(target)
{

}

RType* ReExp_LocalDeref::GetType(IR0Factory& factory)
{
    auto type = target->GetType(factory);

    // TODO: remove reinterpret cast. 어떻게?
    return ((RType_LocalPtr*)type.get())->innerType;
}

ReExp_BoxDeref::ReExp_BoxDeref(const ReExpPtr& target)
    : target(target)
{

}

RType* ReExp_BoxDeref::GetType(IR0Factory& factory)
{
    auto type = target->GetType(factory);

    // TODO: remove reinterpret cast
    return ((RType_BoxPtr*)type.get())->innerType;
}

ReExp_ListIndexer::ReExp_ListIndexer(const ReExpPtr& instance, const ReExpPtr& index, RType* itemType)
    : instance(instance), index(index), itemType(itemType)
{

}

ReExp_Else::ReExp_Else(NExp* nExp)
    : nExp(nExp)
{
}

RType* ReExp_Else::GetType(IR0Factory& factory)
{
    return nExp->GetType(factory);
}



}