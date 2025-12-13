#include "IR0IR1Translator.h"

#include <ranges>

#include "Infra/Expected.h"
#include "Infra/Exceptions.h"

#include "Logging/Diag.h"

#include "RSymbol/RFuncParameter.h"
#include "RSymbol/RFactory.h"

#include "NSymbol/NModule.h"
#include "NSymbol/NFuncDecl.h"

#include "MIR/MData.h"
#include "QIR/QFactory.h"
#include "QIR/QFuncBody.h"

#include "MStmtQInstsTranslation.h"
#include "QBodyContext.h"
#include "ScopeGuard.h"

using namespace std;

namespace Citron {
namespace {

expected<QFuncBody, DiagPtr> TranslateMFuncBodyToQFuncBody(MFuncBody& mFuncBody, const RFactoryPtr& rFactory, const QFactoryPtr& qFactory)
{   
    // TODO: generics
    auto rFuncReturn = mFuncBody.nFuncDecl->GetUnboundFuncReturn();
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

        // parameter 세팅
        // TODO: 일단 generics없이 진행
        auto unboundParams = mFuncBody.nFuncDecl->GetUnboundFuncParams();
        for (size_t i = 0, count = unboundParams.size(); i < count; i++)
        {
            auto& unboundParam = unboundParams[i];

            // 새 local 변수 추가
            bodyContext.AddLocalVar(unboundParam.type, unboundParam.name, i);
        }

        for (auto* mStmt : mFuncBody.stmts)
        {
            auto eResult = TranslateMStmtToQInsts(mStmt, bodyContext);
            RETURN_ON_ERROR(eResult);
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
        auto eQFuncBody = TranslateMFuncBodyToQFuncBody(mFuncBody, rFactory, qFactory);
        RETURN_ON_ERROR(eQFuncBody);

        qFuncBodies.push_back(move(*eQFuncBody));
    }

    return qFactory->MakeQData(move(qFuncBodies));
}

} // namespace Citron