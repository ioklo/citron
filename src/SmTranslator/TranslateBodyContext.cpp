#include "TranslateBodyContext.h"
#include <variant>

#include "Infra/Exceptions.h"
#include "Infra/Expected.h"
#include "Infra/Variants.h"

#include "RSymbol/RFactory.h"
#include "RSymbol/RFuncDecl.h"

#include "MIR/MFuncBody.h"
#include "MIR/MStmt.h"
#include "MIR/MFactory.h"

#include "SStmtToMStmt.h"
#include "TranslationContexts.h"


using namespace std;

namespace Citron {

TranslateBodyContext::TranslateBodyContext(
    TakeRef<LoggerPtr> logger, TakeRef<RFactoryPtr> rFactory, TakeRef<MFactoryPtr> mFactory,
    TakeRef<SRTFactoryPtr> srtFactory, TakeRef<BinOpQueryServicePtr> binOpQueryService)
    : logger{logger.Take()}, rFactory{rFactory.Take()}, mFactory{mFactory.Take()}
    , srtFactory{srtFactory.Take()}, binOpQueryService{binOpQueryService.Take()}
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

CheckEndReturnResult CheckEndReturn(MStmt* stmt, RFactory* rFactory)
{
    struct Checker
    {
        using ResultType = CheckEndReturnResult;
        RFactory* rFactory;

        ResultType Visit(MStmt_Scope* stmt)
        {
            CheckEndReturn(stmt->stmts.back(), rFactory);
        }

        ResultType Visit(MStmt_Command* stmt) { }
        ResultType Visit(MStmt_LocalVarDecl* stmt) { }
        ResultType Visit(MStmt_LocalRefDecl* stmt) { }
        ResultType Visit(MStmt_If* stmt) { }
        ResultType Visit(MStmt_For* stmt) { }
        ResultType Visit(MStmt_While* stmt) { }
        ResultType Visit(MStmt_Continue* stmt) { }
        ResultType Visit(MStmt_Break* stmt) { }
        ResultType Visit(MStmt_Leave* stmt) { }
        ResultType Visit(MStmt_Return* stmt) { }
        ResultType Visit(MStmt_Blank* stmt) { }
        ResultType Visit(MStmt_Exp* stmt) { }
        ResultType Visit(MStmt_Task* stmt) { }
        ResultType Visit(MStmt_Await* stmt) { }
        ResultType Visit(MStmt_Async* stmt) { }
        ResultType Visit(MStmt_Foreach* stmt) { }
        ResultType Visit(MStmt_Yield* stmt) { }
        ResultType Visit(MStmt_Directive* stmt) { }
        ResultType Visit(MStmt_Call* stmt) { }
        ResultType Visit(MStmt_Assign* stmt) { }
        ResultType Visit(MStmt_Do* stmt) { }
    };

    return Accept(Checker{rFactory}, stmt);
}

CheckEndReturnResult CheckEndReturn(RFuncDecl* rFuncDecl, vector<MStmt*>& mStmts, RFactory& rFactory)
{
    // 1. 함수에 Body가 있고, return으로 끝날때
    bool stmtEndsWithReturn = [&mStmts]{
        if (mStmts.empty()) return false;
        return dynamic_cast<MStmt_Return*>(mStmts.back()) != nullptr;
    }();

    if (stmtEndsWithReturn) return CheckEndReturnResult::Valid;

    // 2. 시그니처가 void를 리턴하는지 확인
    bool signatureReturnVoid = [&rFuncDecl, &rFactory] {
        auto rFuncReturn = rFuncDecl->GetUnboundFuncReturn();
        return rFuncReturn.Visit([&rFactory](auto& rFuncReturn) -> bool {
            using T = remove_cvref_t<decltype(rFuncReturn)>;

            if constexpr (same_as<T, RFuncReturn_Normal>)
            {
                auto* rVoidType = rFactory.MakeVoidType();
                return rFuncReturn.type == rVoidType;
            }
            else if constexpr (same_as<T, RFuncReturn_None>) return true;
            else if constexpr (same_as<T, RFuncReturn_NotSet>) return true; // lambda에서 return으로 끝나지 않으면 리턴타입을 void로 보면 된다
            else static_assert(false);

        });
    }();

    return signatureReturnVoid 
        ? CheckEndReturnResult::PutReturnVoid 
        : CheckEndReturnResult::Error;
}

}

expected<MFuncBody, DiagPtr> TranslateBodyContext::Translate(RFuncDecl* rFuncDecl, bool bSeqFunc, std::span<SStmt*> sStmts)
{   
    auto tContext = MakeTranslationContexts(rFuncDecl, bSeqFunc, logger, rFactory, mFactory, srtFactory, binOpQueryService);
    auto e_scope = TranslateScopedSStmtsToMStmt_Scope(sStmts, tContext);
    RETURN_ON_ERROR(e_scope);

    auto* scope = *e_scope;

    // 함수가 return이나 never를 리턴하는 함수로 끝맺지 않았을 경우, 리턴인자가 void인 경우 Return을 추가한다. 나머지는 에러
    auto checkEndReturnResult = CheckEndReturn(rFuncDecl, scope->stmts, *rFactory);

    switch(checkEndReturnResult)
    {
    case CheckEndReturnResult::PutReturnVoid:
    {
        auto* mReturnStmt = mFactory->MakeMStmt<MStmt_Return>(nullopt);
        scope->stmts.push_back(mReturnStmt);
        break;
    }

    case CheckEndReturnResult::Error:
        throw NotImplementedException{};

    case CheckEndReturnResult::Valid:
        break;
    }

    return MFuncBody_Decl{rFuncDecl, scope};
}

void TranslateBodyContext::MarkFailed()
{
    throw NotImplementedException{};
}

}