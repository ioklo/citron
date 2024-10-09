#include "pch.h"
#include "IrExp.h"

#include <IR0/RClassMemberVarDecl.h>
#include <IR0/RStructMemberVarDecl.h>

using namespace std;

namespace Citron::SyntaxIR0Translator {

IrNamespaceExp::IrNamespaceExp(const shared_ptr<RNamespaceDecl>& decl)
    : decl(decl)
{
}

IrTypeVarExp::IrTypeVarExp(const shared_ptr<RType_TypeVar>& type)
    : type(type)
{

}

IrClassExp::IrClassExp(const std::shared_ptr<RClassDecl>& decl, RTypeArgumentsPtr&& typeArgs)
    : decl(decl), typeArgs(std::move(typeArgs))
{
}

IrStructExp::IrStructExp(const std::shared_ptr<RStructDecl>& decl, RTypeArgumentsPtr&& typeArgs)
    : decl(decl), typeArgs(std::move(typeArgs))
{
}

IrEnumExp::IrEnumExp(const std::shared_ptr<REnumDecl>& decl, RTypeArgumentsPtr&& typeArgs)
    : decl(decl), typeArgs(std::move(typeArgs))
{
}

IrThisVarExp::IrThisVarExp(const RTypePtr& type)
    : type(type)
{
}

IrStaticRefExp::IrStaticRefExp(const RLocPtr& loc)
    : loc(loc)
{
}

IrClassMemberBoxRefExp::IrClassMemberBoxRefExp(const RLocPtr& loc, const std::shared_ptr<RClassMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : loc(loc), decl(decl), typeArgs(typeArgs)
{
}

RTypePtr IrClassMemberBoxRefExp::GetTargetType(RTypeFactory& factory)
{
    return decl->GetDeclType(*typeArgs, factory);
}

//RLocPtr ImClassMemberBoxRefRef::MakeLoc()
//{
//    return MakePtr<RClassMemberLoc>(loc, decl, typeArgs);
//}

IrStructIndirectMemberBoxRefExp::IrStructIndirectMemberBoxRefExp(const RLocPtr& loc, const std::shared_ptr<RStructMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : loc(loc), decl(decl), typeArgs(typeArgs)
{
}

RTypePtr IrStructIndirectMemberBoxRefExp::GetTargetType(RTypeFactory& factory)
{
    return decl->GetDeclType(*typeArgs, factory);
}


//RLocPtr ImStructIndirectMemberBoxRefRef::MakeLoc()
//{
//    return MakePtr<RStructMemberLoc>(MakePtr<RBoxDerefLoc>(loc, decl, typeArgs));
//}

IrStructMemberBoxRefExp::IrStructMemberBoxRefExp(const std::shared_ptr<IrBoxRefExp>& parent, const std::shared_ptr<RStructMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : parent(parent), decl(decl), typeArgs(typeArgs)
{
}

RTypePtr IrStructMemberBoxRefExp::GetTargetType(RTypeFactory& factory)
{
    return decl->GetDeclType(*typeArgs, factory);
}


//RLocPtr ImStructMemberBoxRefRef::MakeLoc()
//{
//    return MakePtr<RStructMemberLoc>(parent->MakeLoc(), decl, typeArgs);
//}

IrLocalRefExp::IrLocalRefExp(const RLocPtr& loc)
    : loc(loc)
{
}

IrLocalValueExp::IrLocalValueExp(RExpPtr&& exp)
    : exp(std::move(exp))
{

}

IrDerefedBoxValueExp::IrDerefedBoxValueExp(RLocPtr&& innerLoc)
    : innerLoc(std::move(innerLoc))
{
}

} // Citron::SyntaxIR0Translator