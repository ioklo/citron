#include "MSharedExp.h"

#include "RSymbol/RFactory.h"
#include "RSymbol/RClassVarDecl.h"
#include "RSymbol/RStructVarDecl.h"
#include "MLoc.h"

namespace Citron {

MSharedExp_Static::MSharedExp_Static(MLoc* loc, const RFactoryPtr& rFactory)
    : loc{loc}, rFactory{rFactory}
{
}

RType* MSharedExp_Static::GetType()
{
    return rFactory->MakeSharedType(loc->GetType());
}

MSharedExp_ClassVar::MSharedExp_ClassVar(MLoc* base, RClassVarDecl* decl, RTypeArguments* typeArgs, const RFactoryPtr& rFactory)
    : base{base}, decl{decl}, typeArgs{typeArgs}, rFactory{rFactory}
{
}

RType* MSharedExp_ClassVar::GetType()
{
    auto* declType = decl->GetDeclType(*typeArgs);
    return rFactory->MakeSharedType(declType);
}

MSharedExp_SharedStructVar::MSharedExp_SharedStructVar(MLoc* base, RStructVarDecl* decl, RTypeArguments* typeArgs, const RFactoryPtr& rFactory)
    : base{base}, decl{decl}, typeArgs{typeArgs}, rFactory{rFactory}
{
}

RType* MSharedExp_SharedStructVar::GetType()
{
    auto* declType = decl->GetDeclType(*typeArgs);
    return rFactory->MakeBoxType(declType);
}

MSharedExp_StructVar::MSharedExp_StructVar(MSharedExp* base, RStructVarDecl* decl, RTypeArguments* typeArgs, const RFactoryPtr& rFactory)
    : base{base}, decl{decl}, typeArgs{typeArgs}, rFactory{rFactory}
{
}

RType* MSharedExp_StructVar::GetType()
{
    auto* declType = decl->GetDeclType(*typeArgs);
    return rFactory->MakeBoxType(declType);
}
} // namespace Citron