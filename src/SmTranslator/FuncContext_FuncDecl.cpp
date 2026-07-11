#include "FuncContext_FuncDecl.h"

#include "Infra/Exceptions.h"
#include "Infra/Expected.h"
#include "RSymbol/RDecl.h"
#include "RSymbol/RTypes.h"
#include "RSymbol/RFactory.h"
#include "MIR/MLoc.h"
#include "MIR/MFactory.h"
#include "RSymbol/RFuncDecl.h"
#include "RSymbol/RFuncDeclOuter.h"
#include "RSymbol/RStructDecl.h"
#include "RSymbol/RNamespaceDecl.h"
#include "RSymbol/RGlobalFuncDecl.h"
#include "RSymbol/RClassDecl.h"
#include "RSymbol/RClassCtorDecl.h"
#include "RSymbol/RClassFuncDecl.h"
#include "RSymbol/RStructCtorDecl.h"
#include "RSymbol/RStructDtorDecl.h"
#include "RSymbol/RStructFuncDecl.h"
#include "RSymbol/RLambdaDecl.h"

using namespace std;

namespace Citron {

FuncContext_FuncDecl::FuncContext_FuncDecl(RFuncDecl* rFuncDecl, TakeRef<RFactoryPtr> rFactory, TakeRef<MFactoryPtr> mFactory)
    : rFuncDecl{rFuncDecl}, rFactory{rFactory.Take()}, mFactory{mFactory.Take()}
{
}

bool FuncContext_FuncDecl::CanAccess(RDecl* target)
{
    return rFuncDecl->RFuncDecl_GetDecl()->CanAccess(target);
}

RTypeDecl* FuncContext_FuncDecl::ResolveTypeDecl(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{
    RDecl* curDecl = rFuncDecl->RFuncDecl_GetDecl();

    while (curDecl)
    {
        if (RTypeDecl* typeDecl = curDecl->GetTypeMember(name, explicitTypeParamsExceptOuterCount))
            return typeDecl;

        curDecl = curDecl->GetOuter();
    }

    return nullptr;
}

expected<optional<BodyRes>, DiagPtr> FuncContext_FuncDecl::ResolveIdentifier(InRef<RName> name, size_t explicitTypeParamsExceptOuterCount)
{
    auto o_rDeclRes = rFuncDecl->RFuncDecl_GetDecl()->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount);
    if (!o_rDeclRes) return nullopt;

    return BodyRes_RDeclRes{move(*o_rDeclRes)};
}

RFuncReturn FuncContext_FuncDecl::GetUnboundFuncReturn()
{
    return rFuncDecl->GetUnboundFuncReturn();
}

void FuncContext_FuncDecl::SetOpenFuncReturn(RType* retType)
{
    throw RuntimeFatalException{};
}

RTypeArguments* FuncContext_FuncDecl::MakeOpenTypeArgs()
{
    return rFuncDecl->RFuncDecl_GetDecl()->MakeOpenTypeArgs(*rFactory);
}

bool FuncContext_FuncDecl::IsSeqFunc()
{
    // RFuncDecl은 seq int F(); 를 모른다. 내부 분석용으로 seq를 알고 싶은 용도라면, N*Decl을 호출해야 한다
    // TODO: [67] 2026-07-10, NSymbol, RSymbol 정리하면서 생긴 문제들 해결
    throw NotImplementedException{};
    // return rFuncDecl->IsSeqFunc();
}

struct GetThisTypeFunctor
{
    RFactoryPtr rFactory;

    RType* operator()(auto* nDecl) { return Visit(nDecl); }

    RType* Visit(RStructDecl* rDecl)
    {
        auto* typeArgs = rDecl->MakeOpenTypeArgs(*rFactory);
        return rFactory->MakeStructType(rDecl, typeArgs);
    }

    RType* Visit(RDecl* nDecl)
    {
        // TODO: [67] 2026-07-10, NSymbol, RSymbol 정리하면서 생긴 문제들 해결
        throw NotImplementedException{};
    }
};

MLoc_This* FuncContext_FuncDecl::MakeThisLoc()
{
    // struct S에서는 this가 S 타입
    // class C에서는 this가 C 타입
    // lambda에서는 this가 lambda를 선언한 함수의 this타입

    auto thisTypeKind = rFuncDecl->GetThisKind().GetThisType();

    // auto* rThisType = rFuncDecl.GetNFuncDeclOuter().Visit(GetThisTypeFunctor{rFactory});
    // return mFactory->MakeMLoc<MLoc_This>(rThisType
    // TODO: [67] 2026-07-10, NSymbol, RSymbol 정리하면서 생긴 문제들 해결
    throw NotImplementedException{};
}

} // namespace Citron