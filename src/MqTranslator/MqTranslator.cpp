#include "MqTranslator.h"

#include <ranges>

#include "Infra/Expected.h"
#include "Infra/Exceptions.h"
#include "Infra/Ptr.h"

#include "Logging/Diag.h"

#include "RSymbol/RFuncParameter.h"
#include "RSymbol/RFactory.h"
#include "RSymbol/RFuncDecl.h"
#include "RSymbol/RDecl.h"

#include "RSymbol/RModule.h"

#include "MIR/MData.h"
#include "QIR/QFactory.h"
#include "QIR/QFuncBody.h"

#include "MStmtToQInsts.h"
#include "MqBodyContext.h"
#include "MqScopeGuard.h"
#include "MqTranslationContexts.h"
#include "MqEmitState.h"
#include "MqAbi_Citron_X64.h"
#include "MqFactory.h"

using namespace std;

namespace Citron {
namespace {

expected<QFuncBody, DiagPtr> TranslateMFuncBodyToQFuncBody(MFuncBody& mFuncBody, TakeRef<RFactoryPtr> rFactory, TakeRef<QFactoryPtr> qFactory)
{   
    // TODO: generics
    auto rFuncReturn = mFuncBody.rFuncDecl->GetUnboundFuncReturn();
    auto* rRetType = rFuncReturn.Visit([&rFactory](auto& rFuncReturn) -> RType*
    {
        using T = remove_cvref_t<decltype(rFuncReturn)>;
        if constexpr (same_as<T, RFuncReturn_Normal>)
            return rFuncReturn.type;
        else if constexpr (same_as<T, RFuncReturn_None>)
            return (*rFactory)->MakeVoidType();
        else if constexpr (same_as<T, RFuncReturn_NotSet>)
            throw NotImplementedException{};
        else static_assert(false);
    });

    MqBodyContext bodyContext{*rFactory, *qFactory, rRetType};
    MqFactoryPtr mqFactory = MakePtr<MqFactory>(*rFactory);
    MqAbiPtr abi = MakePtr<MqAbi_Citron_X64>(*rFactory);
    MqTranslationContexts contexts{*rFactory, *qFactory, mqFactory, abi, bodyContext};

    {
        MqScopeGuard mainGuard{std::nullopt, bodyContext};

        auto funcInfo = abi->GetFuncInfo(mFuncBody.rFuncDecl, mFuncBody.rFuncDecl->RFuncDecl_GetDecl()->MakeOpenTypeArgs(**rFactory));
        auto thisKind = mFuncBody.rFuncDecl->GetThisKind();

        visit([rRetType, &bodyContext](auto& returnPassingMode) {
            using T = remove_cvref_t<decltype(returnPassingMode)>;
            if constexpr (same_as<T, MqReturnPassingMode_Void>)
            {
                // 리턴값이 없으므로 아무것도 세팅할 필요 없음
            }
            else if constexpr (same_as<T, MqReturnPassingMode_Direct>)
            {
                // Direct도 딱히 할건 없다
            }
            else if constexpr (same_as<T, MqReturnPassingMode_Indirect>)
            {
                bodyContext.AddIndirectReturn(rRetType);
            }
            else static_assert(false);

        }, funcInfo.returnPassingMode);

        // this 세팅
        visit([&thisKind, &bodyContext](auto& thisPassingMode) {
            using T = remove_cvref_t<decltype(thisPassingMode)>;
            if constexpr (same_as<T, MqThisPassingMode_None>)
            {
                // this가 없으므로 아무것도 세팅할 필요 없음
            }
            else if constexpr(same_as<T, MqThisPassingMode_Handle>)
            {
                auto* thisType_handle = thisKind.TryGetHandle();
                assert(thisType_handle);

                bodyContext.AddThis(thisType_handle->type);
            }
            else if constexpr (same_as<T, MqThisPassingMode_Ptr>)
            {
                auto* thisType_ref = thisKind.TryGetRef();
                assert(thisType_ref);
                auto* thisType = thisType_ref->type;
                auto* ptrThisType = bodyContext.GetPtrType(thisType);

                bodyContext.AddThis(ptrThisType);
            }
        }, funcInfo.thisPassingMode);

        // parameter 세팅
        // TODO: 일단 generics없이 진행
        auto unboundParams = mFuncBody.rFuncDecl->GetUnboundFuncParams();
        for (size_t i = 0, count = unboundParams.size(); i < count; i++)
        {
            auto& unboundParam = unboundParams[i];

            if (unboundParam.IsRef())
            {   
                bodyContext.AddArgument_Ref(unboundParam.type, unboundParam.name, i);
            }
            else
            {
                switch (funcInfo.paramPassingModes[i])
                {
                    case MqParamPassingMode::Direct:
                        bodyContext.AddArgument_Direct(unboundParam.type, unboundParam.name, i);
                        break;
                    case MqParamPassingMode::Indirect:
                        bodyContext.AddArgument_Indirect(unboundParam.type, unboundParam.name, i, &*abi);
                        break;
                    case MqParamPassingMode::Ref:
                        assert(false);

                    case MqParamPassingMode::Forward:
                        throw NotImplementedException{};

                    case MqParamPassingMode::Params:
                        throw NotImplementedException{};
                }
            }
        }

        auto e_s_bodyResult = TranslateMStmt_ScopeToQInsts_Default(mFuncBody.body, contexts);
        RETURN_ON_ERROR(e_s_bodyResult);

        // Done으로 끝나는지 검사하고, 아니라면 void라면 return
        if (*e_s_bodyResult) // Ready 상태라면
        {
            if (bodyContext.IsVoidType(rRetType))
            {
                bodyContext.EmitJumpToCleanUpBlock(MqCleanUpKind_Return{});
                mainGuard.SetDontNeedCleanUp();
            }
            else
                return Error<Error_FuncBody_ShouldEndWithReturn>();
        }
        else // Done 상태라면, guard 해제
        { 
            mainGuard.SetDontNeedCleanUp();
        }
    }

    bodyContext.VerifyBlocks();
    return QFuncBody{
        mFuncBody.rFuncDecl, 
        bodyContext.GetStackSlotInfos() | ranges::to<vector>(), 
        bodyContext.GetBlocks() | ranges::to<vector>() };
}

} // namespace

expected<QData*, DiagPtr> TranslateMDataToQData(MData* mData, TakeRef<RFactoryPtr> rFactory, TakeRef<QFactoryPtr> qFactory)
{
    std::vector<QFuncBody> qFuncBodies;
    auto mFuncBodies = mData->GetAllFuncBodies();

    qFuncBodies.reserve(mFuncBodies.size());
    for (auto& mFuncBody : mFuncBodies)
    {
        auto e_qFuncBody = TranslateMFuncBodyToQFuncBody(mFuncBody, *rFactory, *qFactory);
        RETURN_ON_ERROR(e_qFuncBody);

        qFuncBodies.push_back(move(*e_qFuncBody));
    }

    return (*qFactory)->MakeQData(move(qFuncBodies));
}

} // namespace Citron