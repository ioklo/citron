#include "TranslateBodyContext.h"
#include <variant>

#include "Infra/Exceptions.h"
#include "Infra/Expected.h"
#include "Infra/Variants.h"

#include "RSymbol/RFactory.h"
#include "RSymbol/RFuncDecl.h"
#include "NSymbol/NFuncDecl.h"

#include "MIR/MFuncBody.h"
#include "MIR/MStmt.h"
#include "MIR/MFactory.h"

#include "SStmtToMStmtTranslation.h"
#include "TranslationContexts.h"


using namespace std;

namespace Citron {

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
        auto rFuncReturn = nFuncDecl->GetRFuncDecl()->GetUnboundFuncReturn();
        return visit([&rFactory](auto& rFuncReturn) -> bool {
            using T = remove_cvref_t<decltype(rFuncReturn)>;

            if constexpr (same_as<T, RFuncReturn_Set>)
            {
                auto* rVoidType = rFactory.MakeVoidType();
                return rFuncReturn.type == rVoidType;
            }
            else if constexpr (same_as<T, RFuncReturn_ForCtor>) return true;
            else if constexpr (same_as<T, RFuncReturn_NotSet>) return true; // lambda에서 return으로 끝나지 않으면 리턴타입을 void로 보면 된다
            else static_assert(false);

        }, rFuncReturn);
    }();

    return signatureReturnVoid 
        ? CheckEndReturnResult::PutReturnVoid 
        : CheckEndReturnResult::Error;
}

}

expected<MFuncBody, DiagPtr> TranslateBodyContext::Translate(NFuncDecl* nFuncDecl, std::span<SStmt*> sStmts)
{   
    auto tContext = MakeTranslationContexts(nFuncDecl, logger, rFactory, mFactory, srtFactory, binOpQueryService);
    auto e_mStmts = TranslateSBodyToMStmts(sStmts, tContext);
    RETURN_ON_ERROR(e_mStmts);

    // 함수가 return이나 never를 리턴하는 함수로 끝맺지 않았을 경우, 리턴인자가 void인 경우 Return을 추가한다. 나머지는 에러
    auto checkEndReturnResult = CheckEndReturn(nFuncDecl, *e_mStmts, *rFactory);

    switch(checkEndReturnResult)
    {
    case CheckEndReturnResult::PutReturnVoid:
    {
        auto* mReturnStmt = mFactory->MakeMStmt<MStmt_Return>(nullptr);
        e_mStmts->push_back(mReturnStmt);
        break;
    }

    case CheckEndReturnResult::Error:
        throw NotImplementedException{};

    case CheckEndReturnResult::Valid:
        break;
    }

    return MFuncBody{nFuncDecl, *e_mStmts};
}

void TranslateBodyContext::MarkFailed()
{
    throw NotImplementedException{};
}

}