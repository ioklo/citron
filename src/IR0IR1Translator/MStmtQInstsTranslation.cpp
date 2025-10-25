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
    using ResultType = expected<QBlock*, DiagPtr>;
    
private:
    QBlock* block;
    QBodyContext* bodyContext;
    QFactory* factory;

public:
    MStmtQInstsTranslator(QBlock* block, QBodyContext* bodyContext, QFactory* factory)
        : block{block}, bodyContext{bodyContext}, factory{factory} { }

    // command는 일단 넘깁시다
    ResultType Visit(MStmt_Command* stmt)
    {
        throw NotImplementedException{};
    }
    ResultType Visit(MStmt_LocalVarDecl* stmt)
    {
        throw NotImplementedException{};
    }
    ResultType Visit(MStmt_If* stmt)
    {
        // 1. stmt.cond
        auto eCondV = TranslateMExpToQInsts(stmt->cond, block, bodyContext, factory);
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
            auto eNewTrueBlock = TranslateMStmtsToQInsts(stmt->body, trueBlock, bodyContext, factory);
            if (!eNewTrueBlock) return unexpected{eNewTrueBlock.error()};
            auto* trueTerminator = factory->MakeQJumpInst_Jump(endBlock);
            (*eNewTrueBlock)->SetTerminator(trueTerminator);

            // 5. fill falseBlock
            auto eNewFalseBlock = TranslateMStmtsToQInsts(stmt->elseBody, falseBlock, bodyContext, factory);
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
            auto eNewTrueBlock = TranslateMStmtsToQInsts(stmt->body, trueBlock, bodyContext, factory);
            if (!eNewTrueBlock) return unexpected{eNewTrueBlock.error()};
            auto* trueTerminator = factory->MakeQJumpInst_Jump(endBlock);
            (*eNewTrueBlock)->SetTerminator(trueTerminator);

            return endBlock;
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

expected<QBlock*, DiagPtr> TranslateMStmtToQInsts(MStmt* nStmt, QBlock* block, QBodyContext* bodyContext, QFactory* factory)
{
    MStmtQInstsTranslator translator{block, bodyContext, factory};
    return Accept(translator, nStmt);
}
} // namespace Citron::IIR0IR1Translator