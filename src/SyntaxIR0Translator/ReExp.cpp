#include "ReExp.h"
#include "RSymbol/RTypes.h"
#include "NSymbol/NLambdaVarDecl.h"
#include "NSymbol/NClassVarDecl.h"
#include "NSymbol/NStructVarDecl.h"
#include "NSymbol/NEnumElemVarDecl.h"
#include "MIR/MExp.h"

namespace Citron {

void ReExp_ThisVar::Accept(ReExpVisitor& visitor) { visitor.Visit(this); }
void ReExp_LocalVar::Accept(ReExpVisitor& visitor) { visitor.Visit(this); }
void ReExp_LocalRef::Accept(ReExpVisitor& visitor) { visitor.Visit(this); }
void ReExp_LambdaVar::Accept(ReExpVisitor& visitor) { visitor.Visit(this); }
void ReExp_ClassVar::Accept(ReExpVisitor& visitor) { visitor.Visit(this); }
void ReExp_StructVar::Accept(ReExpVisitor& visitor) { visitor.Visit(this); }
void ReExp_EnumElemVar::Accept(ReExpVisitor& visitor) { visitor.Visit(this); }
void ReExp_PtrDeref::Accept(ReExpVisitor& visitor) { visitor.Visit(this); }
void ReExp_BoxDeref::Accept(ReExpVisitor& visitor) { visitor.Visit(this); }
void ReExp_ListIndexer::Accept(ReExpVisitor& visitor) { visitor.Visit(this); }
void ReExp_Else::Accept(ReExpVisitor& visitor) { visitor.Visit(this); }

ReExp_ThisVar::ReExp_ThisVar(RType* type)
    : type(type)
{
}

ReExp_LocalVar::ReExp_LocalVar(RType* type, const RName& name)
    : type(type), name(name)
{
}

ReExp_LocalRef::ReExp_LocalRef(RType* type, const RName& name)
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

ReExp_PtrDeref::ReExp_PtrDeref(ReExp* target)
    : target(target)
{

}

RType* ReExp_PtrDeref::GetType()
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