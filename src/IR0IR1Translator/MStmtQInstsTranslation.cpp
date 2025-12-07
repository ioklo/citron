#include "MStmtQInstsTranslation.h"

#include "Infra/Exceptions.h"
#include "Infra/Expected.h"
#include "Infra/Variants.h"

#include "MIR/MStmt.h"
#include "QIR/QFactory.h"
#include "QIR/QBlock.h"
#include "QIR/QInsts.h"

#include "CommonQInstsTranslation.h"
#include "MExpQInstsTranslation.h"
#include "QBodyContext.h"
#include "ScopeGuard.h"


using namespace std;

namespace Citron {

class MStmtQInstsTranslator
{
public:
    using ResultType = expected<void, DiagPtr>;
    
private:
    QBodyContext& bodyContext;

public:
    MStmtQInstsTranslator(QBodyContext& bodyContext)
        : bodyContext{bodyContext} {}

    // command는 일단 넘깁시다
    ResultType Visit(MStmt_Command* stmt)
    {
        auto* qStringType = bodyContext.GetStringQType();
        vector<QArg_Input> values;
        for(auto* command : stmt->commands)
        {
            auto slot = bodyContext.NewSlot(qStringType);
            auto eResult = TranslateMExp_StringToQInsts(command, slot, bodyContext);
            RETURN_ON_ERROR(eResult);

            values.push_back(slot);
        }

        bodyContext.EmitIntrinsic(QInst_IntrinsicKind::Command_Items, nullopt, move(values));
        return {};
    }
    
    // 스택에 변수를 둔다.
    ResultType Visit(MStmt_LocalVarDecl* stmt)
    {
        auto slotIndex = bodyContext.AddLocalVar(stmt->type, RName_Normal{stmt->name}, nullopt);

        if (stmt->initExp)
        {   
            auto eInitResult = TranslateMExpToQInsts(stmt->initExp, QArg_Slot{slotIndex}, bodyContext);
            RETURN_ON_ERROR(eInitResult);
        }
        return {};
    }

    // stmt는 qblock으로 변환을 하고 나면, 꼭 포인터가 다음 실행 위치로 가 있어야 한다
    // 근데 둘다 unreachable일 수가 있다. if (cond) return; else return;
    // 1. 
    ResultType Visit(MStmt_If* stmt)
    {
        {
            ScopeGuard mainGuard{bodyContext};
            // 최종 condSlot은 branch에 필요하기 때문에 정리하지 않음
            auto condSlot = bodyContext.NewSlot(bodyContext.GetBoolQType());

            // 1. stmt.cond
            auto eCondResult = TranslateMExpToQInstsWithNewScope(stmt->cond, condSlot, bodyContext);
            RETURN_ON_ERROR(eCondResult);

            if (!stmt->elseBody.empty())
            {
                // 2. add three blocks
                auto* trueBlock = bodyContext.AddBlock("if_true"); // debug label, b23_if_true
                auto* falseBlock = bodyContext.AddBlock("if_false");
                QBlock* endBlock = nullptr; // lazy init

                // 3. add conditional jump
                bodyContext.EmitInst(QInst_CondJump{condSlot, trueBlock, falseBlock});

                // 4. fill trueBlock
                bodyContext.SetCurBlock(trueBlock);
                
                auto eTrueResult = TranslateMStmtsToQInstsWithNewScope(stmt->body, bodyContext);
                RETURN_ON_ERROR(eTrueResult);

                if (!bodyContext.CurBlockEndsWithTermInst())
                {
                    if (!endBlock) endBlock = bodyContext.AddBlock("if_end");
                    bodyContext.EmitInst(QInst_Jump{endBlock});
                }

                // 5. fill falseBlock
                bodyContext.SetCurBlock(falseBlock);
                
                auto eFalseResult = TranslateMStmtsToQInstsWithNewScope(stmt->elseBody, bodyContext);
                RETURN_ON_ERROR(eFalseResult);

                if (!bodyContext.CurBlockEndsWithTermInst())
                {
                    if (!endBlock) endBlock = bodyContext.AddBlock("if_end");
                    bodyContext.EmitInst(QInst_Jump{endBlock});
                }

                // 만약 endBlock이 없으면 unreachable인데..
                bodyContext.SetCurBlock(endBlock);
            }
            else
            {
                // 2. add three blocks
                auto* trueBlock = bodyContext.AddBlock("if_true"); // debug label, b23_if_true
                auto* endBlock = bodyContext.AddBlock("if_end");

                // 3. add conditional jump
                bodyContext.EmitInst(QInst_CondJump{condSlot, trueBlock, endBlock});

                // 4. fill trueBlock
                bodyContext.SetCurBlock(trueBlock);
                auto eTrueBlockResult = TranslateMStmtsToQInstsWithNewScope(stmt->body, bodyContext);
                RETURN_ON_ERROR(eTrueBlockResult);
                
                if (!bodyContext.CurBlockEndsWithTermInst())
                    bodyContext.EmitInst(QInst_Jump{endBlock});

                bodyContext.SetCurBlock(endBlock);
            }
        }

        return {};
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
        if (stmt->exp)
        {
            auto* qType = bodyContext.GetMExpQType(stmt->exp);
            auto eResult = TranslateMExpToQInstsWithNewScope(stmt->exp, bodyContext.GetRetSlot(), bodyContext);
            RETURN_ON_ERROR(eResult);

            bodyContext.EmitJumpToCleanUpForReturnBlock();
            bodyContext.MarkReturnHandledOnCurScope();
        }
        else
        {
            bodyContext.EmitJumpToCleanUpForReturnBlock();
            bodyContext.MarkReturnHandledOnCurScope();
        }

        return {};
    }

    ResultType Visit(MStmt_Block* stmt)
    {
        ScopeGuard mainGuard{bodyContext};

        auto eResult = TranslateMStmtsToQInsts(stmt->stmts, bodyContext);
        RETURN_ON_ERROR(eResult);

        return {};
    }

    ResultType Visit(MStmt_Blank* stmt)
    {
        return {};
    }

    ResultType Visit(MStmt_Exp* stmt)
    {
        ScopeGuard mainGuard{bodyContext};
        auto eResult = TranslateMExpToQInsts(stmt->exp, nullopt, bodyContext);
        RETURN_ON_ERROR(eResult);

        return {};
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

std::expected<void, DiagPtr> TranslateMStmtsToQInstsWithNewScope(std::vector<MStmt*>& mStmts, QBodyContext& qBodyContext)
{
    ScopeGuard guard{qBodyContext};
    return TranslateMStmtsToQInsts(mStmts, qBodyContext);
}

expected<void, DiagPtr> TranslateMStmtToQInsts(MStmt* mStmt, QBodyContext& qBodyContext)
{
    MStmtQInstsTranslator translator{qBodyContext};
    return Accept(translator, mStmt);
}
} // namespace Citron