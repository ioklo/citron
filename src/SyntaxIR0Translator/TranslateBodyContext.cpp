#include "TranslateBodyContext.h"
#include <variant>

#include "Infra/Exceptions.h"
#include "Infra/Expected.h"
#include "Infra/Variants.h"

#include "NSymbol/NFuncDecl.h"

#include "MIR/MFuncBody.h"
#include "MIR/MStmt.h"
#include "TranslationContext.h"
#include "SStmtToMStmtTranslation.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

TranslateBodyContext::TranslateBodyContext(
    const LoggerPtr& logger, const RFactoryPtr& rFactory, const MFactoryPtr& mFactory,
    const SRTFactoryPtr& srtFactory, const BinOpQueryServicePtr& binOpQueryService)
    : logger{logger}, rFactory{rFactory}, mFactory{mFactory}
    , srtFactory{srtFactory}, binOpQueryService{binOpQueryService}
{
}

TranslateBodyContext::~TranslateBodyContext() = default;

namespace {

enum class CheckEndReturnResult
{
    Valid,
    PutReturnVoid,
    Error
};

CheckEndReturnResult CheckEndReturn(NFuncDecl* nFuncDecl, vector<MStmt*>& mStmts, RFactory& rFactory)
{
    // 1. 함수에 Body가 있고, return으로 끝날때
    bool stmtEndsWithReturn = [&mStmts]{
        if (mStmts.empty()) return false;
        return dynamic_cast<MStmt_Return*>(mStmts.back()) != nullptr;
    }();

    if (stmtEndsWithReturn) return CheckEndReturnResult::Valid;

    // 2. 시그니처가 void를 리턴하는지 확인
    bool signatureReturnVoid = [nFuncDecl, &rFactory] {
        auto rFuncReturn = nFuncDecl->GetUnboundFuncReturn();
        return visit(overloaded{
            [&rFactory](RFuncReturn_Set& rFuncReturn) {
                auto* rVoidType = rFactory.MakeVoidType();
                return rFuncReturn.type == rVoidType;
            },
            [](RFuncReturn_ForCtor&) { return true; },
            [](RFuncReturn_NotSet&) { return true; } // lambda에서 return으로 끝나지 않으면 리턴타입을 void로 보면 된다
        }, rFuncReturn);
    }();

    return signatureReturnVoid 
        ? CheckEndReturnResult::PutReturnVoid 
        : CheckEndReturnResult::Error;
}

}

expected<MFuncBody, DiagPtr> TranslateBodyContext::Translate(NFuncDecl* nFuncDecl, std::span<SStmt*> sStmts)
{   
    auto tContext = TranslationContext::Make(nFuncDecl, logger, rFactory, mFactory, srtFactory, binOpQueryService);
    auto eMStmts = TranslateSBodyToMStmts(sStmts, tContext);
    RETURN_ON_ERROR(eMStmts);

    // 함수가 return이나 never를 리턴하는 함수로 끝맺지 않았을 경우, 리턴인자가 void인 경우 Return을 추가한다. 나머지는 에러
    auto checkEndReturnResult = CheckEndReturn(nFuncDecl, *eMStmts, *rFactory);

    switch(checkEndReturnResult)
    {
    case CheckEndReturnResult::PutReturnVoid:
    {
        auto* mReturnStmt = mFactory->MakeMStmt<MStmt_Return>(nullptr);
        eMStmts->push_back(mReturnStmt);
        break;
    }

    case CheckEndReturnResult::Error:
        throw NotImplementedException{};

    case CheckEndReturnResult::Valid:
        break;
    }

    return MFuncBody{nFuncDecl, *eMStmts};
}

void TranslateBodyContext::MarkFailed()
{
    throw NotImplementedException{};
}

TranslationContext TranslateBodyContext::MakeTranslationContext()
{
    throw NotImplementedException{};
}

}