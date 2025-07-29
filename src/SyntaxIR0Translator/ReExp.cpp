#include "ReExp.h"
#include "IR0/RTypes.h"
#include "IR0/NLambdaVarDecl.h"
#include "IR0/NExp.h"
#include "IR0/NClassVarDecl.h"
#include "IR0/NStructVarDecl.h"
#include "IR0/NEnumElemVarDecl.h"

namespace Citron::SyntaxIR0Translator {

ReExp_ThisVar::ReExp_ThisVar(const RTypePtr& type)
    : type(type)
{
}

ReExp_LocalVar::ReExp_LocalVar(const RTypePtr& type, const std::string& name)
    : type(type), name(name)
{
}

ReExp_LambdaVar::ReExp_LambdaVar(const std::shared_ptr<NLambdaVarDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : decl(decl), typeArgs(typeArgs)
{
}

RTypePtr ReExp_LambdaVar::GetType(RTypeFactory& factory)
{
    return decl->GetDeclType(*typeArgs, factory);
}

ReExp_ClassVar::ReExp_ClassVar(const std::shared_ptr<RClassVarDecl>& decl, const RTypeArgumentsPtr& typeArgs, bool hasExplicitInstance, const ReExpPtr& explicitInstance)
    : decl(decl), typeArgs(typeArgs), hasExplicitInstance(hasExplicitInstance), explicitInstance(explicitInstance)
{
}

RTypePtr ReExp_ClassVar::GetType(RTypeFactory& factory)
{
    return decl->GetDeclType(*typeArgs, factory);
}

ReExp_StructVar::ReExp_StructVar(const std::shared_ptr<RStructVarDecl>& decl, const RTypeArgumentsPtr& typeArgs, bool hasExplicitInstance, const ReExpPtr& explicitInstance)
    : decl(decl), typeArgs(typeArgs), hasExplicitInstance(hasExplicitInstance), explicitInstance(explicitInstance)
{
}

RTypePtr ReExp_StructVar::GetType(RTypeFactory& factory)
{
    return decl->GetDeclType(*typeArgs, factory);
}

ReExp_EnumElemVar::ReExp_EnumElemVar(const std::shared_ptr<REnumElemVarDecl>& decl, const RTypeArgumentsPtr& typeArgs, const ReExpPtr& instance)
    : decl(decl), typeArgs(typeArgs), instance(instance)
{
}

RTypePtr ReExp_EnumElemVar::GetType(RTypeFactory& factory)
{
    return decl->GetDeclType(*typeArgs, factory);
}

ReExp_LocalDeref::ReExp_LocalDeref(const ReExpPtr& target)
    : target(target)
{

}

RTypePtr ReExp_LocalDeref::GetType(RTypeFactory& factory)
{
    auto type = target->GetType(factory);

    // TODO: remove reinterpret cast. 어떻게?
    return ((RType_LocalPtr*)type.get())->innerType;
}

ReExp_BoxDeref::ReExp_BoxDeref(const ReExpPtr& target)
    : target(target)
{

}

RTypePtr ReExp_BoxDeref::GetType(RTypeFactory& factory)
{
    auto type = target->GetType(factory);

    // TODO: remove reinterpret cast
    return ((RType_BoxPtr*)type.get())->innerType;
}

ReExp_ListIndexer::ReExp_ListIndexer(const ReExpPtr& instance, const ReExpPtr& index, const RTypePtr& itemType)
    : instance(instance), index(index), itemType(itemType)
{

}

ReExp_Else::ReExp_Else(const NExpPtr& nExp)
    : nExp(nExp)
{
}

RTypePtr ReExp_Else::GetType(RTypeFactory& factory)
{
    return nExp->GetType(factory);
}



}