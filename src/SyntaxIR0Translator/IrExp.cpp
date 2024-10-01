#include "pch.h"
#include "IrExp.h"

#include <IR0/RClassMemberVarDecl.h>
#include <IR0/RStructMemberVarDecl.h>

namespace Citron::SyntaxIR0Translator {

RTypePtr IrClassMemberBoxRefExp::GetTargetType(RTypeFactory& factory)
{
    return decl->GetDeclType(*typeArgs, factory);
}

//RLocPtr ImClassMemberBoxRefRef::MakeLoc()
//{
//    return MakePtr<RClassMemberLoc>(loc, decl, typeArgs);
//}

RTypePtr IrStructIndirectMemberBoxRefExp::GetTargetType(RTypeFactory& factory)
{
    return decl->GetDeclType(*typeArgs, factory);
}

//RLocPtr ImStructIndirectMemberBoxRefRef::MakeLoc()
//{
//    return MakePtr<RStructMemberLoc>(MakePtr<RBoxDerefLoc>(loc, decl, typeArgs));
//}

RTypePtr IrStructMemberBoxRefExp::GetTargetType(RTypeFactory& factory)
{
    return decl->GetDeclType(*typeArgs, factory);
}

//RLocPtr ImStructMemberBoxRefRef::MakeLoc()
//{
//    return MakePtr<RStructMemberLoc>(parent->MakeLoc(), decl, typeArgs);
//}

IrLocalRefExp::IrLocalRefExp(const RLocPtr& loc, const RTypePtr& locType)
    : loc(loc), locType(locType)
{
}

} // Citron::SyntaxIR0Translator