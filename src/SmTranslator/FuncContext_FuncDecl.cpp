#include "FuncContext_FuncDecl.h"

#include "Infra/Exceptions.h"
#include "Infra/Expected.h"
#include "RSymbol/RDecl.h"
#include "RSymbol/RTypes.h"
#include "RSymbol/RFactory.h"
#include "MIR/MLoc.h"
#include "MIR/MFactory.h"
#include "NSymbol/NFuncDecl.h"
#include "NSymbol/NFuncDeclOuter.h"
#include "NSymbol/NStructDecl.h"
#include "NSymbol/NNamespaceDecl.h"
#include "NSymbol/NGlobalFuncDecl.h"
#include "NSymbol/NClassDecl.h"
#include "NSymbol/NClassCtorDecl.h"
#include "NSymbol/NClassFuncDecl.h"
#include "NSymbol/NStructCtorDecl.h"
#include "NSymbol/NStructDtorDecl.h"
#include "NSymbol/NStructFuncDecl.h"
#include "NSymbol/NLambdaDecl.h"

using namespace std;

namespace Citron {

FuncContext_FuncDecl::FuncContext_FuncDecl(NFuncDecl nFuncDecl, const RFactoryPtr& rFactory, const MFactoryPtr& mFactory)
    : nFuncDecl{move(nFuncDecl)}, rFactory{rFactory}, mFactory{mFactory}
{
}

bool FuncContext_FuncDecl::CanAccess(RDecl* target)
{
    return GetNDecl(nFuncDecl)->GetRDecl()->CanAccess(target);
}

RTypeDecl* FuncContext_FuncDecl::ResolveTypeDecl(const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    RDecl* curDecl = GetNDecl(nFuncDecl)->GetRDecl();

    while (curDecl)
    {
        if (RTypeDecl* typeDecl = curDecl->GetTypeMember(name, explicitTypeParamsExceptOuterCount))
            return typeDecl;

        curDecl = curDecl->GetROuter();
    }

    return nullptr;
}

expected<optional<BodyRes>, DiagPtr> FuncContext_FuncDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    auto o_rDeclRes = GetNDecl(nFuncDecl)->GetRDecl()->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount);
    if (!o_rDeclRes) return nullopt;

    return BodyRes_RDeclRes{move(*o_rDeclRes)};
}

RFuncReturn FuncContext_FuncDecl::GetUnboundFuncReturn()
{
    return GetRFuncDecl(nFuncDecl)->GetUnboundFuncReturn();
}

void FuncContext_FuncDecl::SetOpenFuncReturn(RType* retType)
{
    throw RuntimeFatalException{};
}

RTypeArguments* FuncContext_FuncDecl::MakeOpenTypeArgs()
{
    return GetNDecl(nFuncDecl)->GetRDecl()->MakeOpenTypeArgs(*rFactory);
}

bool FuncContext_FuncDecl::IsSeqFunc()
{
    return Citron::IsSeqFunc(nFuncDecl);
}

struct GetThisTypeFunctor
{
    RFactoryPtr rFactory;

    RType* operator()(auto* nDecl) { return Visit(nDecl); }

    RType* Visit(NStructDecl* nDecl)
    {
        auto* typeArgs = nDecl->GetRDecl()->MakeOpenTypeArgs(*rFactory);
        return rFactory->MakeStructType(nDecl, typeArgs);
    }

    RType* Visit(NDecl* nDecl)
    {
        throw NotImplementedException{};
    }
};

MLoc_This* FuncContext_FuncDecl::MakeThisLoc()
{
    // struct S에서는 this가 S 타입
    // class C에서는 this가 C 타입
    // lambda에서는 this가 lambda를 선언한 함수의 this타입

    auto* rThisType = visit(GetThisTypeFunctor{rFactory}, GetNFuncDeclOuter(nFuncDecl));
    return mFactory->MakeMLoc<MLoc_This>(rThisType);
}

} // namespace Citron