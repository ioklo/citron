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

FuncContext_FuncDecl::FuncContext_FuncDecl(RFuncDecl* rFuncDecl, bool bSeqFunc, TakeRef<RFactoryPtr> rFactory, TakeRef<MFactoryPtr> mFactory)
    : rFuncDecl{rFuncDecl}, bSeqFunc{bSeqFunc}, rFactory{rFactory.Take()}, mFactory{mFactory.Take()}
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
        if (RTypeDecl* typeDecl = curDecl->GetTypeMember(name))
            return typeDecl;

        curDecl = curDecl->GetOuter();
    }

    return nullptr;
}

expected<optional<BodyRes>, DiagPtr> FuncContext_FuncDecl::ResolveIdentifier(InRef<RName> name)
{
    // 함수 인자는 최상위 ScopeContext에서 관리한다

    auto* openTypeArgs = rFuncDecl->RFuncDecl_GetDecl()->MakeOpenTypeArgs(*rFactory); // typeArgs를 만들어서 rFuncDecl에 넣어준다

    auto o_rDeclRes = rFuncDecl->RFuncDecl_GetDecl()->ResolveIdentifier(openTypeArgs, name);
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
    // RFuncDecl은 외부 시그니처라서 seq int F(); 를 모른다
    // FuncContext_FuncDecl 생성시에 syntax로부터 seq여부를 전달받아서 리턴한다
    return bSeqFunc;
}

MLoc_This* FuncContext_FuncDecl::MakeThisLoc()
{
    // struct S에서는 this가 S& 타입
    // class C에서는 this가 C 타입
    // lambda에서는 this가 lambda를 선언한 함수의 this타입

    auto* thisType = rFuncDecl->GetThisKind().GetThisType();
    return mFactory->MakeMLoc<MLoc_This>(thisType);
}

} // namespace Citron