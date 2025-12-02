#include "IrExp.h"

#include "Infra/Ptr.h"
#include "RSymbol/RNamespaceDecl.h"
#include "RSymbol/RTypes.h"
#include "NSymbol/NClassVarDecl.h"
#include "NSymbol/NStructVarDecl.h"

#include "MIR/MLoc.h"

#include "TranslationContext.h"

using namespace std;

namespace Citron {

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

IrExp_StaticRef::IrExp_StaticRef(MLoc* loc)
    : loc(loc)
{
}

IrExp_BoxRef_ClassMember::IrExp_BoxRef_ClassMember(MLoc* loc, RClassVarDecl* decl, RTypeArguments* typeArgs)
    : loc(loc), decl(decl), typeArgs(typeArgs)
{
}

RType* IrExp_BoxRef_ClassMember::GetTargetType(RFactory& factory)
{
    return decl->GetDeclType(*typeArgs, factory);
}

MLoc* IrExp_BoxRef_ClassMember::MakeLoc(TranslationContext& context)
{
    return context.MakeNLoc<MLoc_ClassVar>(loc, decl, typeArgs);
}

IrExp_BoxRef_StructIndirectMember::IrExp_BoxRef_StructIndirectMember(MLoc* loc, RStructVarDecl* decl, RTypeArguments* typeArgs)
    : loc(loc), decl(decl), typeArgs(typeArgs)
{
}

RType* IrExp_BoxRef_StructIndirectMember::GetTargetType(RFactory& factory)
{
    return decl->GetDeclType(*typeArgs, factory);
}

MLoc* IrExp_BoxRef_StructIndirectMember::MakeLoc(TranslationContext& context)
{
    return context.MakeNLoc<MLoc_StructVar>(context.MakeNLoc<MLoc_BoxDeref>(loc), decl, typeArgs);
}

IrExp_BoxRef_StructMember::IrExp_BoxRef_StructMember(IrExp_BoxRef* parent, RStructVarDecl* decl, RTypeArguments* typeArgs)
    : parent{parent}, decl{decl}, typeArgs{typeArgs}
{
}

RType* IrExp_BoxRef_StructMember::GetTargetType(RFactory& factory)
{
    return decl->GetDeclType(*typeArgs, factory);
}

MLoc* IrExp_BoxRef_StructMember::MakeLoc(TranslationContext& context)
{
    return context.MakeNLoc<MLoc_StructVar>(parent->MakeLoc(context), decl, typeArgs);
}

IrExp_LocalRef::IrExp_LocalRef(MLoc* loc)
    : loc(loc)
{
}

IrExp_LocalValue::IrExp_LocalValue(MExp* exp)
    : exp{exp}
{

}

IrExp_DerefedBoxValue::IrExp_DerefedBoxValue(MLoc* innerLoc)
    : innerLoc{innerLoc}
{
}

} // Citron::SyntaxIR0Translator