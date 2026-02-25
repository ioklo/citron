#include "IrExp.h"

#include <cassert>
#include "Infra/Ptr.h"
#include "RSymbol/RNamespaceDecl.h"
#include "RSymbol/RTypes.h"
#include "NSymbol/NClassVarDecl.h"
#include "NSymbol/NStructVarDecl.h"

#include "MIR/MLoc.h"
#include "MIR/MFactory.h"
#include "MIR/MSharedExp.h"

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

IrExp_SharedRef_ClassVar::IrExp_SharedRef_ClassVar(MLoc* loc, RClassVarDecl* decl, RTypeArguments* typeArgs, const MFactoryPtr& mFactory)
    : loc{loc}, decl{decl}, typeArgs{typeArgs}, mFactory{mFactory}
{
}

RType* IrExp_SharedRef_ClassVar::GetTargetType()
{
    return decl->GetDeclType(*typeArgs);
}

MLoc* IrExp_SharedRef_ClassVar::MakeLoc()
{
    return mFactory->MakeMLoc<MLoc_ClassVar>(loc, decl, typeArgs);
}

IrExp_SharedRef_SharedStructVar::IrExp_SharedRef_SharedStructVar(MLoc* loc, RStructVarDecl* decl, RTypeArguments* typeArgs, const MFactoryPtr& mFactory)
    : loc{loc}, decl{decl}, typeArgs{typeArgs}, mFactory{mFactory}
{
}

RType* IrExp_SharedRef_SharedStructVar::GetTargetType()
{
    return decl->GetDeclType(*typeArgs);
}

MLoc* IrExp_SharedRef_SharedStructVar::MakeLoc()
{
    return mFactory->MakeMLoc<MLoc_StructVar>(mFactory->MakeMLoc<MLoc_BoxDeref>(loc), decl, typeArgs);
}

IrExp_SharedRef_StructVar::IrExp_SharedRef_StructVar(IrExp_SharedRef* parent, RStructVarDecl* decl, RTypeArguments* typeArgs, const MFactoryPtr& mFactory)
    : parent{parent}, decl{decl}, typeArgs{typeArgs}, mFactory{mFactory}
{
}

RType* IrExp_SharedRef_StructVar::GetTargetType()
{
    return decl->GetDeclType(*typeArgs);
}

MLoc* IrExp_SharedRef_StructVar::MakeLoc()
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

IrExp_SharedDeref::IrExp_SharedDeref(MLoc* innerLoc)
    : innerLoc{innerLoc}
{
}

RType* IrExp_SharedRef::GetTargetType()
{
    auto* sharedExpType = dynamic_cast<RType_Shared*>(sharedExp->GetType());
    assert(sharedExpType);

    return sharedExpType->innerType;
}

} // Citron::SyntaxIR0Translator