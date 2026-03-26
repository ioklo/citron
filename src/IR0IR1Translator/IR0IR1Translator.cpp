#include "IR0IR1Translator.h"

#include <ranges>

#include "Infra/Expected.h"
#include "Infra/Exceptions.h"

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
#include "ScopeGuard.h"

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

    {
        ScopeGuard mainGuard{bodyContext};

        size_t curArgSlotIndex = 0;
        auto* rFuncDecl = mFuncBody.nFuncDecl->GetRFuncDecl();

        // NOTICE: 인자 index는 parameter index랑 다르다
        auto rThisKind = rFuncDecl->GetThisKind();
        switch (rThisKind)
        {
        case RThisKind::None: break;
        case RThisKind::Ptr:
        {
            RType* ptrType = bodyContext.GetPtrType();
            size_t slotIndex = bodyContext.NewSlot(ptrType, curArgSlotIndex++);
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
                RType* ptrType = bodyContext.GetPtrType();
                size_t slotIndex = bodyContext.NewSlot(ptrType, curArgSlotIndex++);
                bodyContext.AddLocalRef_Ptr(unboundParam.type, unboundParam.name, slotIndex);
            }
            else
            {
                // 새 local 변수 추가
                bodyContext.AddLocalVar(unboundParam.type, unboundParam.name, curArgSlotIndex++);
            }
        }

        for (auto* mStmt : mFuncBody.stmts)
        {
            auto e_result = TranslateMStmtToQInsts(mStmt, bodyContext);
            RETURN_ON_ERROR(e_result);
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