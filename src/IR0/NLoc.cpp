module Citron.NDecls:NLoc;

import Citron.Exceptions;
import Citron.RDecls;
import :NExp;
import :NLambdaVarDecl;

namespace Citron {

NLoc_Temp::NLoc_Temp(const NExpPtr& exp)
    : exp(exp)
{
}

NLoc_Temp::NLoc_Temp(NExpPtr&& exp)
    : exp(move(exp))
{
}

RTypePtr NLoc_Temp::GetType(RTypeFactory& factory)
{
    return exp->GetType(factory);
}

NLoc_LocalVar::NLoc_LocalVar(const RName& name, const RTypePtr& declType)
    : name(name), declType(declType)
{

}

RTypePtr NLoc_LocalVar::GetType(RTypeFactory& factory)
{
    return declType;
}

NLoc_LambdaVar::NLoc_LambdaVar(const std::shared_ptr<RLambdaVarDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : decl(decl), typeArgs(typeArgs)
{
}

RTypePtr NLoc_LambdaVar::GetType(RTypeFactory& factory)
{
    return decl->GetDeclType(*typeArgs, factory);
}

NLoc_ListIndexer::NLoc_ListIndexer(NLocPtr&& list, const NLocPtr& index, const RTypePtr& itemType)
    : list(move(list)), index(index), itemType(itemType)
{
}

RTypePtr NLoc_ListIndexer::GetType(RTypeFactory& factory)
{
    return itemType;
}

NLoc_StructVar::NLoc_StructVar(const NLocPtr& instance, const std::shared_ptr<RStructVarDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : instance(instance), decl(decl), typeArgs(typeArgs)
{
}

RTypePtr NLoc_StructVar::GetType(RTypeFactory& factory)
{
    return decl->GetDeclType(*typeArgs, factory);

}

NLoc_ClassVar::NLoc_ClassVar(NLocPtr&& instance, const std::shared_ptr<RClassVarDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : instance(move(instance)), decl(decl), typeArgs(typeArgs)
{
}


RTypePtr NLoc_ClassVar::GetType(RTypeFactory& factory)
{
    return decl->GetDeclType(*typeArgs, factory);
}

NLoc_EnumElemVar::NLoc_EnumElemVar(const NLocPtr& instance, std::shared_ptr<REnumElemVarDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : instance(instance), decl(decl), typeArgs(typeArgs)
{
}

RTypePtr NLoc_EnumElemVar::GetType(RTypeFactory& factory)
{
    return decl->GetDeclType(*typeArgs, factory);
}

NLoc_This::NLoc_This(RTypePtr type)
    : type(move(type))
{
}

RTypePtr NLoc_This::GetType(RTypeFactory& factory)
{
    return type;
}

NLoc_LocalDeref::NLoc_LocalDeref(NLocPtr&& innerLoc)
    : innerLoc(move(innerLoc))
{
}

RTypePtr NLoc_LocalDeref::GetType(RTypeFactory& factory)
{
    auto type = innerLoc->GetType(factory);
    
    if (auto* localPtrType = dynamic_cast<RType_LocalPtr*>(type.get()))
        return localPtrType->innerType;

    // 에러, 어떻게 해야할지 생각해본다
    throw NotImplementedException();
}

NLoc_BoxDeref::NLoc_BoxDeref(NLocPtr&& innerLoc)
    : innerLoc(move(innerLoc))
{
}

RTypePtr NLoc_BoxDeref::GetType(RTypeFactory& factory)
{
    auto type = innerLoc->GetType(factory);

    if (auto* boxPtrType = dynamic_cast<RType_BoxPtr*>(type.get()))
        return boxPtrType->innerType;

    // 에러, 어떻게 해야할지 생각해본다
    throw NotImplementedException();
}

NLoc_NullableValue::NLoc_NullableValue(const NLocPtr& loc)
    : loc(loc)
{

}

RTypePtr NLoc_NullableValue::GetType(RTypeFactory& factory)
{
    auto type = loc->GetType(factory);

    if (auto* nullableType = dynamic_cast<RType_NullableValue*>(type.get()))
        return nullableType->innerType;

    // 에러, 어떻게 해야할지 생각해본다
    throw NotImplementedException();
}

}