#include "IR0IR1Translator.h"

#include "NSymbol/NModule.h"
#include "QIR/QFactory.h"
#include "Logging/Diag.h"

#include "MIR/MData.h"

#include "MStmtQInstsTranslation.h"

using namespace std;

namespace Citron {

expected<QFuncBody, DiagPtr> TranslateMFuncBodyToQFuncBody(MFuncBody& mFuncBody, QFactoryPtr& factory)
{   
    QBodyContext qBodyContext{};

    // BodyContext를 하나 만들고,
    QBodyContext qBodyContext{};

    for (auto* mStmt : body.stmts)
    {
        TranslateMStmtToQInsts(mStmt, )
    }

    return QFuncBody{};
}

expected<QData*, DiagPtr> TranslateMDataToQData(MData* mData, QFactoryPtr& qFactory)
{   
    QData* data = qFactory->MakeQData();

    for (auto& funcBody : mData->GetAllFuncBodies())
    {
        TranslateMFuncBodyToQFuncBody(funcBody, factory);
    }

    return nullptr;
}

} // namespace Citron