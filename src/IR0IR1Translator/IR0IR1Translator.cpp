#include "IR0IR1Translator.h"

#include <ranges>

#include "Infra/Expected.h"
#include "Logging/Diag.h"

#include "RSymbol/RFuncParameter.h"

#include "NSymbol/NModule.h"
#include "NSymbol/NFuncDecl.h"

#include "MIR/MData.h"
#include "QIR/QFactory.h"
#include "QIR/QFuncBody.h"

#include "MStmtQInstsTranslation.h"
#include "QBodyContext.h"

using namespace std;
using namespace Citron::IR0IR1Translator;

namespace Citron {
namespace {

expected<QFuncBody, DiagPtr> TranslateMFuncBodyToQFuncBody(MFuncBody& mFuncBody, const RFactoryPtr& rFactory, const QFactoryPtr& qFactory)
{   
    QBodyContext bodyContext{rFactory, qFactory};

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

    bodyContext.CompleteFunc();
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

        qFuncBodies.push_back(std::move(*eQFuncBody));
    }

    return qFactory->MakeQData(move(qFuncBodies));
}

} // namespace Citron