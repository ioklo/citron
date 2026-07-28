#include "SmFuncContext_FuncDecl.h"

#include "Infra/Exceptions.h"
#include "RSymbol/RDecl.h"
#include "RSymbol/RTypes.h"
#include "MIR/MLoc.h"
#include "MIR/MFactory.h"
#include "RSymbol/RFuncDecl.h"
#include "SmTypeRes.h"
#include "SmDeclContext.h"

using namespace std;

namespace Citron {

SmFuncContext_FuncDecl::SmFuncContext_FuncDecl(TakeRef<SmDeclContextPtr> funcDeclContext, RFuncDecl* rFuncDecl, bool bSeqFunc, TakeRef<RFactoryPtr> rFactory, TakeRef<MFactoryPtr> mFactory)
    : funcDeclContext{funcDeclContext.Take()}, rFuncDecl{rFuncDecl}, bSeqFunc{bSeqFunc}, rFactory{rFactory.Take()}, mFactory{mFactory.Take()}
{
}

bool SmFuncContext_FuncDecl::CanAccess(RDecl* target)
{
    return rFuncDecl->RFuncDecl_GetDecl()->CanAccess(target);
}

std::optional<SmTypeRes> SmFuncContext_FuncDecl::ResolveTypeIdentifier(InRef<RName> name)
{   
    return funcDeclContext->ResolveTypeIdentifier(name); // declContext에서 type identifier를 검색한다
}

expected<optional<SmBodyRes>, DiagPtr> SmFuncContext_FuncDecl::ResolveIdentifier(InRef<RName> name)
{
    // 함수 인자는 최상위 ScopeContext에서 관리한다
    auto o_declRes = funcDeclContext->ResolveIdentifier(name);
    if (!o_declRes) return nullopt;

    return SmBodyRes_DeclRes{move(*o_declRes)};
}

RFuncReturn SmFuncContext_FuncDecl::GetUnboundFuncReturn()
{
    return rFuncDecl->GetUnboundFuncReturn();
}

void SmFuncContext_FuncDecl::SetOpenFuncReturn(RType* retType)
{
    throw RuntimeFatalException{};
}

RTypeArguments* SmFuncContext_FuncDecl::MakeOpenTypeArgs()
{
    return rFuncDecl->RFuncDecl_GetDecl()->MakeOpenTypeArgs(*rFactory);
}

bool SmFuncContext_FuncDecl::IsSeqFunc()
{
    // RFuncDecl은 외부 시그니처라서 seq int F(); 를 모른다
    // SmFuncContext_FuncDecl 생성시에 syntax로부터 seq여부를 전달받아서 리턴한다
    return bSeqFunc;
}

MLoc_This* SmFuncContext_FuncDecl::MakeThisLoc()
{
    // struct S에서는 this가 S& 타입
    // class C에서는 this가 C 타입
    // lambda에서는 this가 lambda를 선언한 함수의 this타입

    auto* thisType = rFuncDecl->GetThisKind().GetThisType();
    return mFactory->MakeMLoc<MLoc_This>(thisType);
}

} // namespace Citron