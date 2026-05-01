#include "IR0IR1Translator.h"

#include <ranges>

#include "Infra/Expected.h"
#include "Infra/Exceptions.h"
#include "Infra/Ptr.h"

#include "Logging/Diag.h"

#include "RSymbol/RFuncParameter.h"
#include "RSymbol/RFactory.h"
#include "RSymbol/RFuncDecl.h"

#include "NSymbol/NModule.h"
#include "NSymbol/NFuncDecl.h"

#include "MIR/MData.h"
#include "QIR/QFactory.h"
#include "QIR/QFuncBody.h"

#include "MStmtToQInsts.h"
#include "QBodyContext.h"
#include "QScopeGuard.h"
#include "QTranslationContexts.h"
#include "QEmitState.h"
#include "QAbi_Citron_X64.h"
#include "MqFactory.h"

using namespace std;

namespace Citron {
namespace {

expected<QFuncBody, DiagPtr> TranslateMFuncBodyToQFuncBody(MFuncBody& mFuncBody, const RFactoryPtr& rFactory, const QFactoryPtr& qFactory)
{   
    // TODO: generics
    auto* rFuncDecl = mFuncBody.nFuncDecl->GetRFuncDecl();

    auto rFuncReturn = rFuncDecl->GetUnboundFuncReturn();
    auto* rRetType = visit([&rFactory](auto& rFuncReturn) -> RType*
    {
        using T = remove_cvref_t<decltype(rFuncReturn)>;
        if constexpr (same_as<T, RFuncReturn_Set>)
            return rFuncReturn.type;
        else if constexpr (same_as<T, RFuncReturn_ForCtor>)
            return rFactory->MakeVoidType();
        else if constexpr (same_as<T, RFuncReturn_NotSet>)
            throw NotImplementedException{};
        else static_assert(false);
    }, rFuncReturn);

    QBodyContext bodyContext{rFactory, qFactory, rRetType};
    MqFactoryPtr mqFactory = MakePtr<MqFactory>(rFactory);
    QAbiPtr qAbi = MakePtr<QAbi_Citron_X64>(rFactory);
    QTranslationContexts contexts{rFactory, qFactory, mqFactory, qAbi, bodyContext};

    {
        QScopeGuard mainGuard{std::nullopt, bodyContext};

        auto* rFuncDecl = mFuncBody.nFuncDecl->GetRFuncDecl();
        auto funcInfo = qAbi->GetFuncInfo(rFuncDecl, rFuncDecl->GetRDecl()->MakeOpenTypeArgs(*rFactory));

        // NOTICE: 인자 index는 parameter index랑 다르다
        auto rThisKind = rFuncDecl->GetThisKind();
        switch (rThisKind)
        {
        case RThisKind::None: break;
        case RThisKind::Ptr:
        {
            RType* ptrType = bodyContext.GetPtrType();
            size_t slotIndex = bodyContext.AddThis(ptrType);
            break;
        }
        case RThisKind::Handle:
            throw NotImplementedException{};
        }

        // parameter 세팅
        // TODO: 일단 generics없이 진행
        auto unboundParams = rFuncDecl->GetUnboundFuncParams();
        for (size_t i = 0, count = unboundParams.size(); i < count; i++)
        {
            auto& unboundParam = unboundParams[i];

            if (unboundParam.IsRef())
            {   
                bodyContext.AddRefArgument(unboundParam.type, unboundParam.name, i);
            }
            else
            {
                

                // 새 local 변수 추가
                bodyContext.AddArgument(unboundParam.type, unboundParam.name, i);
            }
        }

        auto e_s_bodyResult = TranslateMStmt_ScopeToQInsts_Default(mFuncBody.body, contexts);
        RETURN_ON_ERROR(e_s_bodyResult);

        // Done으로 끝나는지 검사하고, 아니라면 void라면 return
        if (*e_s_bodyResult) // Ready 상태라면
        {
            if (bodyContext.IsVoidType(rRetType))
            {
                bodyContext.EmitJumpToCleanUpBlock(QCleanUpKind_Return{});
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
        mFuncBody.nFuncDecl, 
        bodyContext.GetStackSlotInfos() | ranges::to<vector>(), 
        bodyContext.GetBlocks() | ranges::to<vector>() };
}

} // namespace

expected<QData*, DiagPtr> TranslateMDataToQData(MData* mData, const RFactoryPtr& rFactory, const QFactoryPtr& qFactory)
{
    std::vector<QFuncBody> qFuncBodies;
    auto mFuncBodies = mData->GetAllFuncBodies();

    qFuncBodies.reserve(mFuncBodies.size());
    for (auto& mFuncBody : mFuncBodies)
    {
        auto e_qFuncBody = TranslateMFuncBodyToQFuncBody(mFuncBody, rFactory, qFactory);
        RETURN_ON_ERROR(e_qFuncBody);

        qFuncBodies.push_back(move(*e_qFuncBody));
    }

    return qFactory->MakeQData(move(qFuncBodies));
}

} // namespace Citron