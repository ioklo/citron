#include "MStmtQInstsTranslation.h"

#include "Infra/Exceptions.h"
#include "Infra/Expected.h"
#include "Infra/Variants.h"

#include "Logging/Diag.h"

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
        ScopeGuard guard{bodyContext};

        auto* qStringType = bodyContext.GetStringQType();
        vector<QArg_Input> values;
        for(auto* command : stmt->commands)
        {
            size_t slotIndex = bodyContext.NewSlot(qStringType);
            auto eResult = TranslateMExp_StringToQInstsWithNewScope(command, slotIndex, bodyContext);
            RETURN_ON_ERROR(eResult);

            values.push_back(slotIndex);
        }

        return bodyContext.EmitIntrinsic(QInst_IntrinsicKind::Command_Items, nullopt, move(values));
    }
    
    // 스택에 변수를 둔다.
    ResultType Visit(MStmt_LocalVarDecl* stmt)
    {
        auto slotIndex = bodyContext.AddLocalVar(stmt->type, RName_Normal{stmt->name}, nullopt);

        // TODO: initExp가 없어도, default constructor가 불려야 한다. 어떤 Constructor를 부를지는 IR0에서 결정한다
        if (stmt->initExp)
        {   
            // var s = expr;
            // expr이 lvalue인 경우, 복사 (복사가 지원 가능할때)
            // expr이 rvalue인 경우, 이동 

            auto eInitResult = TranslateMExpToQInstsWithNewScope(stmt->initExp, slotIndex, bodyContext);
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
            auto condSlotIndex = bodyContext.NewSlot(bodyContext.GetBoolQType());

            // 1. stmt.cond
            auto eCondResult = TranslateMExpToQInstsWithNewScope(stmt->cond, condSlotIndex, bodyContext);
            RETURN_ON_ERROR(eCondResult);

            if (!stmt->elseBody.empty())
            {
                // 2. add three blocks
                auto* trueBlock = bodyContext.AddBlock("if_true"); // debug label, b23_if_true
                auto* falseBlock = bodyContext.AddBlock("if_false");
                QBlock* endBlock = nullptr; // lazy init

                // 3. add conditional jump
                auto eEmitResult = bodyContext.EmitTermInst(QInst_CondJump{condSlotIndex, trueBlock, falseBlock});
                RETURN_ON_ERROR(eEmitResult);

                // 4. fill trueBlock
                bodyContext.SetCurBlock(trueBlock);
                
                auto eTrueResult = TranslateMStmtsToQInstsWithNewScope(stmt->body, bodyContext);
                RETURN_ON_ERROR(eTrueResult);

                if (!bodyContext.IsUnreachable())
                {
                    if (!endBlock) endBlock = bodyContext.AddBlock("if_end");
                    auto eEmitTermInstResult = bodyContext.EmitTermInst(QInst_Jump{endBlock});
                    RETURN_ON_ERROR(eEmitTermInstResult);
                }

                // 5. fill falseBlock
                bodyContext.SetCurBlock(falseBlock);
                
                auto eFalseResult = TranslateMStmtsToQInstsWithNewScope(stmt->elseBody, bodyContext);
                RETURN_ON_ERROR(eFalseResult);

                if (!bodyContext.IsUnreachable())
                {
                    if (!endBlock) endBlock = bodyContext.AddBlock("if_end");
                    auto eEmitTermInstResult = bodyContext.EmitTermInst(QInst_Jump{endBlock});
                    RETURN_ON_ERROR(eEmitTermInstResult);
                }

                // 만약 endBlock이 없으면 unreachable상태이고, 그럼 그냥 둔다
                if (endBlock)
                    bodyContext.SetCurBlock(endBlock);
            }
            else
            {
                // 2. add three blocks
                auto* trueBlock = bodyContext.AddBlock("if_true"); // debug label, b23_if_true
                auto* endBlock = bodyContext.AddBlock("if_end");

                // 3. add conditional jump
                auto eEmitTermInstResult = bodyContext.EmitTermInst(QInst_CondJump{condSlotIndex, trueBlock, endBlock});
                RETURN_ON_ERROR(eEmitTermInstResult);

                // 4. fill trueBlock
                bodyContext.SetCurBlock(trueBlock);
                auto eTrueBlockResult = TranslateMStmtsToQInstsWithNewScope(stmt->body, bodyContext);
                RETURN_ON_ERROR(eTrueBlockResult);
                
                if (!bodyContext.IsUnreachable())
                {
                    auto eEmitTermInstResult = bodyContext.EmitTermInst(QInst_Jump{endBlock});
                    RETURN_ON_ERROR(eEmitTermInstResult);
                }

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
            auto eResult = TranslateMExpToQInstsWithNewScope(stmt->exp, bodyContext.GetRetSlotIndex(), bodyContext);
            RETURN_ON_ERROR(eResult);

            auto eEmitResult = bodyContext.EmitJumpToCleanUpForReturnBlock();
            RETURN_ON_ERROR(eEmitResult);

            bodyContext.MarkReturnHandledOnCurScope();
        }
        else
        {
            auto eEmitResult = bodyContext.EmitJumpToCleanUpForReturnBlock();
            RETURN_ON_ERROR(eEmitResult);

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
        auto eResult = TranslateMExpToQInstsWithNewScope(stmt->exp, nullopt, bodyContext);
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

expected<void, DiagPtr> TranslateMStmtsToQInsts(std::vector<MStmt*>& mStmts, QBodyContext& bodyContext)
{   
    for(auto* mStmt : mStmts)
    {
        // unreachable 처리는 Emit이 실제로 일어날때 처리해야 한다. 여기서 처리하지 않는다
        auto eResult = TranslateMStmtToQInsts(mStmt, bodyContext);
        RETURN_ON_ERROR(eResult);
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