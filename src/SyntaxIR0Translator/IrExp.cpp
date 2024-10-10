#include "pch.h"
#include "IrExp.h"

#include <IR0/RClassMemberVarDecl.h>
#include <IR0/RStructMemberVarDecl.h>

using namespace std;

namespace Citron::SyntaxIR0Translator {

IrExp_Namespace::IrExp_Namespace(const shared_ptr<RNamespaceDecl>& decl)
    : decl(decl)
{
}

IrExp_TypeVar::IrExp_TypeVar(const shared_ptr<RType_TypeVar>& type)
    : type(type)
{

}

IrExp_Class::IrExp_Class(const std::shared_ptr<RClassDecl>& decl, RTypeArgumentsPtr&& typeArgs)
    : decl(decl), typeArgs(std::move(typeArgs))
{
}

IrExp_Struct::IrExp_Struct(const std::shared_ptr<RStructDecl>& decl, RTypeArgumentsPtr&& typeArgs)
    : decl(decl), typeArgs(std::move(typeArgs))
{
}

IrExp_Enum::IrExp_Enum(const std::shared_ptr<REnumDecl>& decl, RTypeArgumentsPtr&& typeArgs)
    : decl(decl), typeArgs(std::move(typeArgs))
{
}

IrExp_ThisVar::IrExp_ThisVar(const RTypePtr& type)
    : type(type)
{
}

IrExp_StaticRef::IrExp_StaticRef(const RLocPtr& loc)
    : loc(loc)
{
}

IrExp_BoxRef_ClassMember::IrExp_BoxRef_ClassMember(const RLocPtr& loc, const std::shared_ptr<RClassMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : loc(loc), decl(decl), typeArgs(typeArgs)
{
}

RTypePtr IrExp_BoxRef_ClassMember::GetTargetType(RTypeFactory& factory)
{
    return decl->GetDeclType(*typeArgs, factory);
}

//RLocPtr ImClassMemberBoxRefRef::MakeLoc()
//{
//    return MakePtr<RClassMemberLoc>(loc, decl, typeArgs);
//}

IrExp_BoxRef_StructIndirectMember::IrExp_BoxRef_StructIndirectMember(const RLocPtr& loc, const std::shared_ptr<RStructMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : loc(loc), decl(decl), typeArgs(typeArgs)
{
}

RTypePtr IrExp_BoxRef_StructIndirectMember::GetTargetType(RTypeFactory& factory)
{
    return decl->GetDeclType(*typeArgs, factory);
}


//RLocPtr ImStructIndirectMemberBoxRefRef::MakeLoc()
//{
//    return MakePtr<RStructMemberLoc>(MakePtr<RBoxDerefLoc>(loc, decl, typeArgs));
//}

IrExp_BoxRef_StructMember::IrExp_BoxRef_StructMember(const std::shared_ptr<IrExp_BoxRef>& parent, const std::shared_ptr<RStructMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : parent(parent), decl(decl), typeArgs(typeArgs)
{
}

RTypePtr IrExp_BoxRef_StructMember::GetTargetType(RTypeFactory& factory)
{
    return decl->GetDeclType(*typeArgs, factory);
}


//RLocPtr ImStructMemberBoxRefRef::MakeLoc()
//{
//    return MakePtr<RStructMemberLoc>(parent->MakeLoc(), decl, typeArgs);
//}

IrExp_LocalRef::IrExp_LocalRef(const RLocPtr& loc)
    : loc(loc)
{
}

IrExp_LocalValue::IrExp_LocalValue(RExpPtr&& exp)
    : exp(std::move(exp))
{

}

IrExp_DerefedBoxValue::IrExp_DerefedBoxValue(RLocPtr&& innerLoc)
    : innerLoc(std::move(innerLoc))
{
}

} // Citron::SyntaxIR0Translator