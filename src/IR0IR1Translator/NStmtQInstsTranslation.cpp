#include "NStmtQInstsTranslation.h"

#include "Infra/Exceptions.h"
#include "IR0/NStmt.h"
#include "IR1/QFactory.h"
#include "IR1/QBlock.h"
#include "IR1/QInsts.h"

#include "NExpQInstsTranslation.h"
#include "QBodyContext.h"

using namespace std;

namespace Citron::IR0IR1Translator {

class NStmtQInstsTranslator
{
public:
    using ResultType = expected<QBlock*, DiagPtr>;
    
private:
    QBlock* block;
    QBodyContext* bodyContext;
    QFactory* factory;

public:
    NStmtQInstsTranslator(QBlock* block, QBodyContext* bodyContext, QFactory* factory)
        : block{block}, bodyContext{bodyContext}, factory{factory} { }

    // command는 일단 넘깁시다
    ResultType Visit(NStmt_Command* stmt)
    {
        throw NotImplementedException{};
    }
    ResultType Visit(NStmt_LocalVarDecl* stmt)
    {
        throw NotImplementedException{};
    }
    ResultType Visit(NStmt_If* stmt)
    {
        // 1. stmt.cond
        auto eCondV = TranslateNExpToQInsts(stmt->cond, block, bodyContext, factory);
        if (!eCondV) return unexpected{eCondV.error()};

        if (!stmt->elseBody.empty())
        {
            // 2. add three blocks
            auto* trueBlock = bodyContext->AddBlock("if_true"); // debug label, b23_if_true
            auto* falseBlock = bodyContext->AddBlock("if_false");
            auto* endBlock = bodyContext->AddBlock("if_end");

            // 3. add conditional jump
            auto* condJumpInst = factory->MakeQJumpInst_CondJump(*eCondV, trueBlock, falseBlock);
            block->SetTerminator(condJumpInst);

            // 4. fill trueBlock
            auto eNewTrueBlock = TranslateNStmtsToQInsts(stmt->body, trueBlock, bodyContext, factory);
            if (!eNewTrueBlock) return unexpected{eNewTrueBlock.error()};
            auto* trueTerminator = factory->MakeQJumpInst_Jump(endBlock);
            (*eNewTrueBlock)->SetTerminator(trueTerminator);

            // 5. fill falseBlock
            auto eNewFalseBlock = TranslateNStmtsToQInsts(stmt->elseBody, falseBlock, bodyContext, factory);
            if (!eNewFalseBlock) return unexpected{eNewFalseBlock.error()};
            auto* falseTerminator = factory->MakeQJumpInst_Jump(endBlock);
            (*eNewFalseBlock)->SetTerminator(falseTerminator);

            return endBlock;
        }
        else
        {
            // 2. add three blocks
            auto* trueBlock = bodyContext->AddBlock("if_true"); // debug label, b23_if_true
            auto* endBlock = bodyContext->AddBlock("if_end");

            // 3. add conditional jump
            auto* condJumpInst = factory->MakeQJumpInst_CondJump(*eCondV, trueBlock, endBlock);
            block->SetTerminator(condJumpInst);

            // 4. fill trueBlock
            auto eNewTrueBlock = TranslateNStmtsToQInsts(stmt->body, trueBlock, bodyContext, factory);
            if (!eNewTrueBlock) return unexpected{eNewTrueBlock.error()};
            auto* trueTerminator = factory->MakeQJumpInst_Jump(endBlock);
            (*eNewTrueBlock)->SetTerminator(trueTerminator);

            return endBlock;
        }
    }
    ResultType Visit(NStmt_IfNullableRefTest* stmt)
    {
        throw NotImplementedException{};
    }
    ResultType Visit(NStmt_IfNullableValueTest* stmt)
    {
        throw NotImplementedException{};
    }
    ResultType Visit(NStmt_For* stmt)
    {
        throw NotImplementedException{};
    }
    ResultType Visit(NStmt_Continue* stmt)
    {
        throw NotImplementedException{};
    }
    ResultType Visit(NStmt_Break* stmt)
    {
        throw NotImplementedException{};
    }
    ResultType Visit(NStmt_Return* stmt)
    {
        throw NotImplementedException{};
    }
    ResultType Visit(NStmt_Block* stmt)
    {
        throw NotImplementedException{};
    }
    ResultType Visit(NStmt_Blank* stmt)
    {
        throw NotImplementedException{};
    }
    ResultType Visit(NStmt_Exp* stmt)
    {
        throw NotImplementedException{};
    }
    ResultType Visit(NStmt_Task* stmt)
    {
        throw NotImplementedException{};
    }
    ResultType Visit(NStmt_Await* stmt)
    {
        throw NotImplementedException{};
    }
    ResultType Visit(NStmt_Async* stmt)
    {
        throw NotImplementedException{};
    }
    ResultType Visit(NStmt_Foreach* stmt)
    {
        throw NotImplementedException{};
    }
    ResultType Visit(NStmt_ForeachCast* stmt)
    {
        throw NotImplementedException{};
    }
    ResultType Visit(NStmt_Yield* stmt)
    {
        throw NotImplementedException{};
    }
    ResultType Visit(NStmt_CallClassCtor* stmt)
    {
        throw NotImplementedException{};
    }
    ResultType Visit(NStmt_CallStructCtor* stmt)
    {
        throw NotImplementedException{};
    }
    ResultType Visit(NStmt_NullDirective* stmt)
    {
        throw NotImplementedException{};
    }
    ResultType Visit(NStmt_NotNullDirective* stmt)
    {
        throw NotImplementedException{};
    }
    ResultType Visit(NStmt_StaticNullDirective* stmt)
    {
        throw NotImplementedException{};
    }
    ResultType Visit(NStmt_StaticNotNullDirective* stmt)
    {
        throw NotImplementedException{};
    }
    ResultType Visit(NStmt_StaticUnknownNullDirective* stmt)
    {
        throw NotImplementedException{};
    }
};

expected<QBlock*, DiagPtr> TranslateNStmtToQInsts(NStmt* nStmt, QBlock* block, QBodyContext* bodyContext, QFactory* factory)
{
    NStmtQInstsTranslator translator{block, bodyContext, factory};
    return Accept(translator, nStmt);
}
} // namespace Citron::IIR0IR1Translator