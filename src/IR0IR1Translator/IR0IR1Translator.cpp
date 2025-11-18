#include "IR0IR1Translator.h"

#include "Infra/Expected.h"
#include "Logging/Diag.h"

#include "NSymbol/NModule.h"
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

    for (auto* mStmt : mFuncBody.stmts)
    {   
        auto eResult = TranslateMStmtToQInsts(mStmt, bodyContext);
        RETURN_ON_ERROR(eResult);
    }

    bodyContext.CompleteFunc();
    return QFuncBody{mFuncBody.nFuncDecl, bodyContext.GetEntryBlock()};
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