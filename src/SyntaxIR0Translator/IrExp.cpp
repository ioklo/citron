#include "IrExp.h"

#include "Infra/Ptr.h"
#include "RSymbol/RNamespaceDecl.h"
#include "RSymbol/RTypes.h"
#include "NSymbol/NClassVarDecl.h"
#include "NSymbol/NStructVarDecl.h"

#include "MIR/MLoc.h"
#include "MIR/MFactory.h"

#include "TranslationContexts.h"

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

IrExp_BoxRef_ClassMember::IrExp_BoxRef_ClassMember(MLoc* loc, RClassVarDecl* decl, RTypeArguments* typeArgs, const MFactoryPtr& mFactory)
    : loc{loc}, decl{decl}, typeArgs{typeArgs}, mFactory{mFactory}
{
}

RType* IrExp_BoxRef_ClassMember::GetTargetType()
{
    return decl->GetDeclType(*typeArgs);
}

MLoc* IrExp_BoxRef_ClassMember::MakeLoc()
{
    return mFactory->MakeMLoc<MLoc_ClassVar>(loc, decl, typeArgs);
}

IrExp_BoxRef_StructIndirectMember::IrExp_BoxRef_StructIndirectMember(MLoc* loc, RStructVarDecl* decl, RTypeArguments* typeArgs, const MFactoryPtr& mFactory)
    : loc{loc}, decl{decl}, typeArgs{typeArgs}, mFactory{mFactory}
{
}

RType* IrExp_BoxRef_StructIndirectMember::GetTargetType()
{
    return decl->GetDeclType(*typeArgs);
}

MLoc* IrExp_BoxRef_StructIndirectMember::MakeLoc()
{
    return mFactory->MakeMLoc<MLoc_StructVar>(mFactory->MakeMLoc<MLoc_BoxDeref>(loc), decl, typeArgs);
}

IrExp_BoxRef_StructMember::IrExp_BoxRef_StructMember(IrExp_BoxRef* parent, RStructVarDecl* decl, RTypeArguments* typeArgs, const MFactoryPtr& mFactory)
    : parent{parent}, decl{decl}, typeArgs{typeArgs}, mFactory{mFactory}
{
}

RType* IrExp_BoxRef_StructMember::GetTargetType()
{
    return decl->GetDeclType(*typeArgs);
}

MLoc* IrExp_BoxRef_StructMember::MakeLoc()
{
    return mFactory->MakeMLoc<MLoc_StructVar>(parent->MakeLoc(), decl, typeArgs);
}

IrExp_PtrRef::IrExp_PtrRef(MLoc* loc)
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