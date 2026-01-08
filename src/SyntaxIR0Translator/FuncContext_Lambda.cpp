#include "FuncContext_Lambda.h"
#include "Infra/Expected.h"
#include "Infra/Exceptions.h"
#include "RSymbol/RMember.h"
#include "RSymbol/RTypes.h"
#include "NSymbol/NLambdaVarDecl.h"
#include "MIR/MFactory.h"
#include "MIR/MLoc.h"
#include "MIR/MExp.h"
#include "ScopeContext.h"

using namespace std;

namespace Citron {

FuncContext_Lambda::FuncContext_Lambda(const FuncContextPtr& outerFunc, const ScopeContextPtr& outerScope, bool bSeqFunc, RFuncReturn&& funcReturn, std::vector<RFuncParameter>&& funcParams, bool bLastParamVariadic)
    : outerFunc{outerFunc}, outerScope{outerScope}, bSeqFunc{bSeqFunc}, funcReturn{move(funcReturn)}, funcParams{move(funcParams)}, bLastParamVariadic{bLastParamVariadic}
{
}

bool FuncContext_Lambda::CanAccess(RDecl* target)
{
    return outerFunc->CanAccess(target);
}

RTypeDecl* FuncContext_Lambda::ResolveTypeDecl(const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    return outerFunc->ResolveTypeDecl(name, explicitTypeParamsExceptOuterCount);
}

expected<optional<RMember>, DiagPtr> FuncContext_Lambda::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    auto e_o_rMember = outerScope->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount);
    RETURN_ON_ERROR(e_o_rMember);

    if (!*e_o_rMember) return nullopt;

    // 상위 스코프에서 얻어오는 
    // 로컬과 람다 멤버, this만 감싸는 대상이다
    if (auto* localVar = get_if<RMember_LocalVar>(&**e_o_rMember))
    {
        auto* localVarLoc = mFactory->MakeMLoc<MLoc_LocalVar>(localVar->name, localVar->type);
        auto* initExp = mFactory->MakeMExp<MExp_Load>(localVarLoc);
        auto initArg = MArgument_Exp(initExp);

        auto* lambdaVar = StageLambdaVar(localVar->type, localVar->name, move(initArg));

        auto* openTypeArgs = MakeOpenTypeArgs();
        return RMember_LambdaVar{openTypeArgs, lambdaVar};
    }

    if (auto* localRef = get_if<RMember_LocalRef>(&**e_o_rMember))
    {
        throw NotImplementedException{};
    }

    if (auto* lambdaVar = get_if<RMember_LambdaVar>(&**e_o_rMember))
    {
        // class C<T> { void F<S> {
        //     List<T> x;      // 5) scopeContext.ResolveIdentifier(x, 0) => RMember
        //     var f = () => { // 4) funcContext.ResolveIdentifier(x, 0)
        //         // 3) scopeContext.ResolveIdentifier(x, 0)
        //     
        //         var g = () => { // 2) funcContext.ResolveIdentifier(x, 0)
        //
        //             // 1) 여기에서 scopeContext.ResolveIdentifier(x, 0) 호출
        //             x; 
        //     
        //         }
        //     }
        // } }

        auto openTypeArgs = MakeOpenTypeArgs();
        auto* lambdaVarDecl = mFactory->MakeMLoc<MLoc_LambdaVar>(lambdaVar->decl, openTypeArgs);
        auto* loadExp = mFactory->MakeMExp<MExp_Load>(lambdaVarDecl);
        MArgument_Exp initArg{loadExp};

        auto* newLambdaVar = StageLambdaVar(lambdaVar->decl->GetUnboundDeclType(), lambdaVar->decl->GetName(), move(initArg));
        return RMember_LambdaVar{openTypeArgs, newLambdaVar};
    }

    if (auto* thisVar = get_if<RMember_ThisVar>(&**e_o_rMember))
    {
        if (auto structType = dynamic_cast<RType_Struct*>(thisVar->type))
            throw NotImplementedException{};

        auto* thisLoc = mFactory->MakeMLoc<MLoc_This>(thisVar->type);
        auto* initExp = mFactory->MakeMExp<MExp_Load>(thisLoc);
        auto initArg = MArgument_Exp{initExp};

        auto* lambdaVar = StageLambdaVar(thisVar->type, RNames::_this, move(initArg));
        auto* openTypeArgs = MakeOpenTypeArgs();

        return RMember_LambdaVar(openTypeArgs, lambdaVar);
    }

    // 나머지는 그대로 리턴
    return e_o_rMember;
}

RFuncReturn FuncContext_Lambda::GetUnboundFuncReturn()
{
    return funcReturn;
}

void FuncContext_Lambda::SetOpenFuncReturn(RType* retType)
{
    assert(holds_alternative<RFuncReturn_NotSet>(funcReturn));
    funcReturn = RFuncReturn_Set{retType};
}

RTypeArguments* FuncContext_Lambda::MakeOpenTypeArgs()
{
    return outerFunc->MakeOpenTypeArgs();
}

bool FuncContext_Lambda::IsSeqFunc()
{
    return bSeqFunc;
}

MLoc_This* FuncContext_Lambda::MakeThisLoc()
{
    return outerFunc->MakeThisLoc();
}

} // namespace Citron