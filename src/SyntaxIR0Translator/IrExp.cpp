#include "IrExp.h"

#include "Infra/Ptr.h"
#include "IR0/RNamespaceDecl.h"
#include "IR0/RTypes.h"
#include "IR0/NLoc.h"
#include "IR0/NClassVarDecl.h"
#include "IR0/NStructVarDecl.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

IrExp_Namespace::IrExp_Namespace(RNamespaceDecl* decl)
    : decl(decl)
{
}

IrExp_TypeVar::IrExp_TypeVar(RType_TypeVar* type)
    : type(type)
{

}

IrExp_Class::IrExp_Class(RClassDecl* decl, RTypeArguments* typeArgs)
    : decl(decl), typeArgs(typeArgs)
{
}

IrExp_Struct::IrExp_Struct(RStructDecl* decl, RTypeArguments* typeArgs)
    : decl(decl), typeArgs(typeArgs)
{
}

IrExp_Enum::IrExp_Enum(REnumDecl* decl, RTypeArguments* typeArgs)
    : decl(decl), typeArgs(typeArgs)
{
}

IrExp_ThisVar::IrExp_ThisVar(RType* type)
    : type(type)
{
}

IrExp_StaticRef::IrExp_StaticRef(NLoc* loc)
    : loc(loc)
{
}

IrExp_BoxRef_ClassMember::IrExp_BoxRef_ClassMember(NLoc* loc, RClassVarDecl* decl, RTypeArguments* typeArgs)
    : loc(loc), decl(decl), typeArgs(typeArgs)
{
}

RType* IrExp_BoxRef_ClassMember::GetTargetType(IR0Factory& factory)
{
    return decl->GetDeclType(*typeArgs, factory);
}

NLoc* IrExp_BoxRef_ClassMember::MakeLoc()
{
    return MakePtr<NLoc_ClassVar>(loc, decl, typeArgs);
}

IrExp_BoxRef_StructIndirectMember::IrExp_BoxRef_StructIndirectMember(NLoc* loc, RStructVarDecl* decl, RTypeArguments* typeArgs)
    : loc(loc), decl(decl), typeArgs(typeArgs)
{
}

RType* IrExp_BoxRef_StructIndirectMember::GetTargetType(IR0Factory& factory)
{
    return decl->GetDeclType(*typeArgs, factory);
}

NLoc* IrExp_BoxRef_StructIndirectMember::MakeLoc()
{
    return MakePtr<NLoc_StructVar>(MakePtr<NLoc_BoxDeref>(loc), decl, typeArgs);
}

IrExp_BoxRef_StructMember::IrExp_BoxRef_StructMember(const std::shared_ptr<IrExp_BoxRef>& parent, RStructVarDecl* decl, RTypeArguments* typeArgs)
    : parent(parent), decl(decl), typeArgs(typeArgs)
{
}

RType* IrExp_BoxRef_StructMember::GetTargetType(IR0Factory& factory)
{
    return decl->GetDeclType(*typeArgs, factory);
}

NLoc* IrExp_BoxRef_StructMember::MakeLoc()
{
    return MakePtr<NLoc_StructVar>(parent->MakeLoc(), decl, typeArgs);
}

IrExp_LocalRef::IrExp_LocalRef(NLoc* loc)
    : loc(loc)
{
}

IrExp_LocalValue::IrExp_LocalValue(NExp* exp)
    : exp(move(exp))
{

}

IrExp_DerefedBoxValue::IrExp_DerefedBoxValue(NLoc* innerLoc)
    : innerLoc(move(innerLoc))
{
}

} // Citron::SyntaxIR0Translator