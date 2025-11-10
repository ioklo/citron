#include "MStmtQInstsTranslation.h"

#include "Infra/Exceptions.h"
#include "MIR/MStmt.h"
#include "QIR/QFactory.h"
#include "QIR/QBlock.h"
#include "QIR/QInsts.h"

#include "MExpQInstsTranslation.h"
#include "QBodyContext.h"

using namespace std;

namespace Citron::IR0IR1Translator {

class MStmtQInstsTranslator
{
public:
    using ResultType = expected<void, DiagPtr>;
    
private:
    QBodyContext& qBodyContext;

public:
    MStmtQInstsTranslator(QBodyContext& qBodyContext)
        : qBodyContext{qBodyContext} { }

    // command는 일단 넘깁시다
    ResultType Visit(MStmt_Command* stmt)
    {
        throw NotImplementedException{};
    }
    
    // 스택에 변수를 둔다.
    ResultType Visit(MStmt_LocalVarDecl* stmt)
    {
        throw NotImplementedException{};
    }

    ResultType Visit(MStmt_If* stmt)
    {
        // 1. stmt.cond
        auto eCondV = TranslateMExpToQInsts(stmt->cond, qBodyContext);
        if (!eCondV) return unexpected{eCondV.error()};

        if (!stmt->elseBody.empty())
        {
            // 2. add three blocks
            auto* trueBlock = qBodyContext.AddBlock("if_true"); // debug label, b23_if_true
            auto* falseBlock = qBodyContext.AddBlock("if_false");
            auto* endBlock = qBodyContext.AddBlock("if_end");

            // 3. add conditional jump
            qBodyContext.CompleteBlock(QInst_CondJump{*eCondV, trueBlock, falseBlock});

            // 4. fill trueBlock
            qBodyContext.SetCurBlock(trueBlock);
            auto eTrueResult = TranslateMStmtsToQInsts(stmt->body, qBodyContext);
            if (!eTrueResult) return unexpected{eTrueResult.error()};
            qBodyContext.CompleteBlock(QInst_Jump{endBlock});

            // 5. fill falseBlock
            qBodyContext.SetCurBlock(falseBlock);
            auto eFalseResult = TranslateMStmtsToQInsts(stmt->elseBody, qBodyContext);
            if (!eFalseResult) return unexpected{eFalseResult.error()};
            qBodyContext.CompleteBlock(QInst_Jump{endBlock});

            qBodyContext.SetCurBlock(endBlock);
            return {};
        }
        else
        {
            // 2. add three blocks
            auto* trueBlock = qBodyContext.AddBlock("if_true"); // debug label, b23_if_true
            auto* endBlock = qBodyContext.AddBlock("if_end");

            // 3. add conditional jump
            qBodyContext.CompleteBlock(QInst_CondJump{*eCondV, trueBlock, endBlock});

            // 4. fill trueBlock
            qBodyContext.SetCurBlock(trueBlock);
            auto eNewTrueBlock = TranslateMStmtsToQInsts(stmt->body, qBodyContext);
            if (!eNewTrueBlock) return unexpected{eNewTrueBlock.error()};
            qBodyContext.CompleteBlock(QInst_Jump{endBlock});

            qBodyContext.SetCurBlock(endBlock);
            return {};
        }
    }
    ResultType Visit(MStmt_IfNullableRefTest* stmt)
    {
        throw NotImplementedException{};
    }
    ResultType Visit(MStmt_IfNullableValueTest* stmt)
    {
        throw NotImplementedException{};
    }
    ResultType Visit(MStmt_For* stmt)
    {
        throw NotImplementedException{};
    }
    ResultType Visit(MStmt_Continue* stmt)
    {
        throw NotImplementedException{};
    }
    ResultType Visit(MStmt_Break* stmt)
    {
        throw NotImplementedException{};
    }
    ResultType Visit(MStmt_Return* stmt)
    {
        throw NotImplementedException{};
    }
    ResultType Visit(MStmt_Block* stmt)
    {
        throw NotImplementedException{};
    }
    ResultType Visit(MStmt_Blank* stmt)
    {
        throw NotImplementedException{};
    }
    ResultType Visit(MStmt_Exp* stmt)
    {
        throw NotImplementedException{};
    }
    ResultType Visit(MStmt_Task* stmt)
    {
        throw NotImplementedException{};
    }
    ResultType Visit(MStmt_Await* stmt)
    {
        throw NotImplementedException{};
    }
    ResultType Visit(MStmt_Async* stmt)
    {
        throw NotImplementedException{};
    }
    ResultType Visit(MStmt_Foreach* stmt)
    {
        throw NotImplementedException{};
    }
    ResultType Visit(MStmt_ForeachCast* stmt)
    {
        throw NotImplementedException{};
    }
    ResultType Visit(MStmt_Yield* stmt)
    {
        throw NotImplementedException{};
    }
    ResultType Visit(MStmt_CallClassCtor* stmt)
    {
        throw NotImplementedException{};
    }
    ResultType Visit(MStmt_CallStructCtor* stmt)
    {
        throw NotImplementedException{};
    }
    ResultType Visit(MStmt_NullDirective* stmt)
    {
        throw NotImplementedException{};
    }
    ResultType Visit(MStmt_NotNullDirective* stmt)
    {
        throw NotImplementedException{};
    }
    ResultType Visit(MStmt_StaticNullDirective* stmt)
    {
        throw NotImplementedException{};
    }
    ResultType Visit(MStmt_StaticNotNullDirective* stmt)
    {
        throw NotImplementedException{};
    }
    ResultType Visit(MStmt_StaticUnknownNullDirective* stmt)
    {
        throw NotImplementedException{};
    }
};

expected<void, DiagPtr> TranslateMStmtsToQInsts(std::vector<MStmt*>& mStmts, QBodyContext& qBodyContext)
{
    for(auto* mStmt : mStmts)
    {
        auto eResult = TranslateMStmtToQInsts(mStmt, qBodyContext);
        if (!eResult) return unexpected{eResult.error()};
    }

    return {};
}

expected<void, DiagPtr> TranslateMStmtToQInsts(MStmt* mStmt, QBodyContext& qBodyContext)
{
    MStmtQInstsTranslator translator{qBodyContext};
    return Accept(translator, mStmt);
}
} // namespace Citron::IIR0IR1Translator