#include "MStmtToQInsts.h"

#include "Infra/Exceptions.h"
#include "Infra/Expected.h"
#include "Infra/Variants.h"

#include "Logging/Diag.h"

#include "MIR/MStmt.h"
#include "MIR/MExp.h"
#include "QIR/QFactory.h"
#include "QIR/QBlock.h"
#include "QIR/QInsts.h"

#include "CommonQInstsTranslation.h"
#include "MLocToQInsts.h"
#include "QBodyContext.h"
#include "ScopeGuard.h"
#include "MReadToQInsts.h"
#include "QTranslationContexts.h"
#include "MCreateToQInsts.h"

using namespace std;

namespace Citron {


struct MStmtQInstsTranslator
{
    using ResultType = expected<void, DiagPtr>;
    
    QTranslationContexts& contexts;

    ResultType Visit(MStmt* mStmt) { throw NotImplementedException{}; }

    ResultType Visit(MStmt_Command* mStmt) 
    { 
        ScopeGuard guard{contexts.bodyContext};
        
        vector<QArg_Input> values;
        for (auto& mCommand : mStmt->commands)
        {
            // Read니까. 이미 있는 slot을 돌려 받는다.
            auto e_readResult = TranslateMRead_LocToQInsts(mCommand, contexts);
            RETURN_ON_ERROR(e_readResult);

            // NBC이기 때문에 (string) 인자로 넘겨줄 때는 pointer가 되어야 한다
            auto e_result = visit([this, &values](auto& readResult) -> ResultType {
                using T = remove_cvref_t<decltype(readResult)>;
                if constexpr (same_as<T, QReadResult_Slot>)
                {
                    size_t ptrSlotIndex = contexts.bodyContext.NewSlot(contexts.bodyContext.GetPtrType());
                    contexts.bodyContext.EmitInst(QInst_AddrOf{QArg_Slot{ptrSlotIndex}, QArg_Slot{readResult.slotIndex}});
                    values.push_back(QArg_Slot{readResult.slotIndex});
                    return {};
                }
                else if constexpr (same_as<T, QReadResult_Ptr>)
                {
                    values.push_back(QArg_Slot{readResult.slotIndex});
                    return {};
                }
                else static_assert(false);
            }, * e_readResult);
            RETURN_ON_ERROR(e_result);
        }

        contexts.bodyContext.EmitIntrinsic(QInst_IntrinsicKind::Command_Items, nullopt, move(values));

        return {};
    }

    ResultType Visit(MStmt_LocalVarDecl* mStmt) 
    {
        auto slotIndex = contexts.bodyContext.AddLocalVar(mStmt->type, mStmt->name, /*o_argIndex*/nullopt);

        return visit([this, slotIndex](auto& init) -> ResultType {
            using T = remove_cvref_t<decltype(init)>;
            if constexpr (same_as<T, MStmt_LocalVarDeclInit_Uninit>) { return {}; }
            else if constexpr (same_as<T, MStmt_LocalVarDeclInit_Create>)
            {
                // LocalVarDecl
                // var s = S();
                // var s = s1; => var s = S(s1); 복사             
                // rvalue 인데 두가지 경우로 나눠진다
                // var s = move s2; => var s = S(move s2); 이동
                // var s = F();

                // primitive의 경우
                // var i = 1; => 1은 rvalue

                // var s = expr;
                // expr이 lvalue인 경우, 복사 (복사가 지원 가능할때)
                // expr이 rvalue인 경우, 이동 

                ScopeGuard guard{contexts.bodyContext};
                auto e_initResult = TranslateMCreateToQInsts(init.create, slotIndex, contexts);
                RETURN_ON_ERROR(e_initResult);
                return {};
            }
            else static_assert(false);

        }, mStmt->init);
    }

    ResultType Visit(MStmt_LocalRefDecl* mStmt) 
    {
        // auto slotIndex = bodyContext.AddLocalRef(stmt->type, stmt->name, nullopt);
        auto e_locResult = TranslateMLocToQInsts(mStmt->loc, contexts);
        RETURN_ON_ERROR(e_locResult);

        visit([this, mStmt](auto& locResult) {

            using T = remove_cvref_t<decltype(locResult)>;

            if constexpr (same_as<T, QLocResult_Slot>)
            {
                contexts.bodyContext.AddLocalRef_Alias(mStmt->name, locResult.slotIndex);
            }
            else if constexpr (same_as<T, QLocResult_Ptr>)
            {
                // ptr slot을 로컬 ref로 선언
                contexts.bodyContext.AddLocalRef_Ptr(mStmt->type, mStmt->name, locResult.slotIndex);
            }
            else static_assert(false);
        }, *e_locResult);

        return {};
    }

    expected<QReadResult, DiagPtr> TranslateMReadToQInstsWithNewScope(MRead& mRead)
    {
        ScopeGuard guard{contexts.bodyContext};
        return TranslateMReadToQInsts(mRead, contexts);
    }

    ResultType HandleIf(MStmt_If* mStmt, size_t condSlotIndex)
    {
        auto& bodyContext = contexts.bodyContext;

        if (!mStmt->elseBody.empty())
        {
            // 2. add three blocks
            auto* trueBlock = bodyContext.AddBlock("if_true"); // debug label, b23_if_true
            auto* falseBlock = bodyContext.AddBlock("if_false");
            QBlock* endBlock = nullptr; // lazy init

            // 3. add conditional jump
            bodyContext.EmitTermInst(QInst_CondJump{condSlotIndex, trueBlock, falseBlock});

            // 4. fill trueBlock
            bodyContext.SetCurBlock(trueBlock);

            auto e_trueResult = TranslateMStmtsToQInstsWithNewScope(mStmt->body, contexts);
            RETURN_ON_ERROR(e_trueResult);

            if (!contexts.bodyContext.IsUnreachable())
            {
                if (!endBlock) endBlock = contexts.bodyContext.AddBlock("if_end");
                bodyContext.EmitTermInst(QInst_Jump{endBlock});
            }

            // 5. fill falseBlock
            bodyContext.SetCurBlock(falseBlock);

            auto e_falseResult = TranslateMStmtsToQInstsWithNewScope(mStmt->elseBody, contexts);
            RETURN_ON_ERROR(e_falseResult);

            if (!bodyContext.IsUnreachable())
            {
                if (!endBlock) endBlock = bodyContext.AddBlock("if_end");
                bodyContext.EmitTermInst(QInst_Jump{endBlock});
            }

            // 만약 endBlock이 없으면 unreachable상태이고, 그럼 그냥 둔다
            if (endBlock)
                bodyContext.SetCurBlock(endBlock);
        }
        else
        {
            // 2. add two blocks
            auto* trueBlock = bodyContext.AddBlock("if_true"); // debug label, b23_if_true
            auto* endBlock = bodyContext.AddBlock("if_end");

            // 3. add conditional jump
            bodyContext.EmitTermInst(QInst_CondJump{condSlotIndex, trueBlock, endBlock});

            // 4. fill trueBlock
            bodyContext.SetCurBlock(trueBlock);
            auto e_trueBlockResult = TranslateMStmtsToQInstsWithNewScope(mStmt->body, contexts);
            RETURN_ON_ERROR(e_trueBlockResult);

            if (!bodyContext.IsUnreachable())
            {
                bodyContext.EmitTermInst(QInst_Jump{endBlock});
            }

            bodyContext.SetCurBlock(endBlock);
        }

        return {};
    }

    ResultType Visit(MStmt_If* mStmt) 
    {
        {
            ScopeGuard mainGuard{contexts.bodyContext};

            // 1. stmt.cond
            auto e_condResult = TranslateMReadToQInstsWithNewScope(mStmt->cond);
            RETURN_ON_ERROR(e_condResult);

            // readResult to slotIndex
            return visit([this, mStmt](auto& condResult) -> ResultType {
                using T = remove_cvref_t<decltype(condResult)>;

                auto& bodyContext = contexts.bodyContext;
                if constexpr (same_as<T, QReadResult_Slot>)
                {
                    return HandleIf(mStmt, condResult.slotIndex);
                }
                else if constexpr (same_as<T, QReadResult_Ptr>)
                {
                    auto* boolType = bodyContext.GetBoolType();
                    size_t newSlot = bodyContext.NewSlot(boolType);
                    bodyContext.EmitInst(QInst_Load{.type = boolType, .dest = newSlot, .src = condResult.slotIndex});

                    return HandleIf(mStmt, newSlot);
                }
                else if constexpr (same_as<T, QReadResult_ConstBool>)
                {
                    if (condResult.value)
                    {
                        // true block을 만들 필요도 없다                        
                        auto e_trueResult = TranslateMStmtsToQInstsWithNewScope(mStmt->body, contexts);
                        RETURN_ON_ERROR(e_trueResult);
                    }
                    else
                    {
                        if (!mStmt->elseBody.empty())
                        {
                            // false block을 만들 필요도 없다
                            auto e_falseResult = TranslateMStmtsToQInstsWithNewScope(mStmt->elseBody, contexts);
                            RETURN_ON_ERROR(e_falseResult);
                        }
                    }

                    return {};
                }
                else if constexpr (same_as<T, QReadResult_ConstInt32>)
                    throw RuntimeFatalException{}; // SyntaxIR0 Translation에서 이미 체크가 되었어야 한다
                else static_assert(false);
            }, *e_condResult);
        }
    }

    // for(initStmts; cond; contStmt) body
    ResultType Visit(MStmt_For* mStmt)
    {
        QBodyContext& bodyContext = contexts.bodyContext;

        {
            ScopeGuard mainGuard{bodyContext};

            // QBlock* initBlock = bodyContext.AddBlock("for_init");
            QBlock* bodyBlock = bodyContext.AddBlock("for_body");
            QBlock* contBlock = bodyContext.AddBlock("for_cont");
            QBlock* exitBlock = bodyContext.AddBlock("for_exit");
            // bodyContext.SetCurBlock(initBlock);

            for (auto* mInitStmt : mStmt->initStmts)
            {
                auto e_result = TranslateMStmtToQInsts(mInitStmt, contexts);
                RETURN_ON_ERROR(e_result);
            }

            QBlock* condBlock;
            // cond와 body가 모두 들어가는
            if (mStmt->cond)
            {
                condBlock = bodyContext.AddBlock("for_cond");
                bodyContext.EmitTermInst(QInst_Jump{condBlock});
                bodyContext.SetCurBlock(condBlock);

                auto e_condResult = TranslateMReadToQInstsWithNewScope(*mStmt->cond);
                RETURN_ON_ERROR(e_condResult);
                
                visit([this, mStmt, bodyBlock, exitBlock](auto& condResult) {
                    auto& bodyContext = contexts.bodyContext;
                    using T = remove_cvref_t<decltype(condResult)>;
                    if constexpr (same_as<T, QReadResult_Slot>)
                    {
                        bodyContext.EmitTermInst(QInst_CondJump{condResult.slotIndex, bodyBlock, exitBlock});
                    }
                    else if constexpr (same_as<T, QReadResult_Ptr>)
                    {
                        auto* boolType = bodyContext.GetBoolType();
                        size_t newSlot = bodyContext.NewSlot(boolType);
                        bodyContext.EmitInst(QInst_Load{.type = boolType, .dest = newSlot, .src = condResult.slotIndex});
                        bodyContext.EmitTermInst(QInst_CondJump{newSlot, bodyBlock, exitBlock});
                    }
                    else if constexpr (same_as<T, QReadResult_ConstBool>)
                    {
                        if (condResult.value)
                            bodyContext.EmitTermInst(QInst_Jump{bodyBlock});
                        else
                            bodyContext.EmitTermInst(QInst_Jump{exitBlock});
                    }
                    else if constexpr (same_as<T, QReadResult_ConstInt32>)
                        throw RuntimeFatalException{}; // SyntaxIR0 Translation에서 이미 체크가 되었어야 한다
                    else static_assert(false);
                }, *e_condResult);
            }
            else
            {
                condBlock = bodyBlock;
                // cond가 없으면 무조건 jump한다
                bodyContext.EmitTermInst(QInst_Jump{bodyBlock});
            }

            bodyContext.SetCurBlock(bodyBlock);

            // continue는 contBlock, break는 exitBlock으로 지정해준다
            auto e_bodyResult = TranslateMStmtsToQInsts(mStmt->body, contexts);
            RETURN_ON_ERROR(e_bodyResult);

            bodyContext.EmitTermInst(QInst_Jump{contBlock});

            bodyContext.SetCurBlock(contBlock);
            auto e_contResult = TranslateMStmtToQInsts(mStmt->contStmt, contexts);
            RETURN_ON_ERROR(e_contResult);

            bodyContext.EmitTermInst(QInst_Jump{condBlock});
            bodyContext.SetCurBlock(exitBlock);
        }

        return {};
    }
    // ResultType Visit(MStmt_Continue* mStmt) { }
    // ResultType Visit(MStmt_Break* mStmt) { }
    ResultType Visit(MStmt_Return* mStmt) 
    { 
        auto& bodyContext = contexts.bodyContext;

        if (mStmt->create)
        {
            {
                ScopeGuard guard{bodyContext};
                auto e_result = TranslateMCreateToQInsts(*mStmt->create, bodyContext.GetRetSlotIndex(), contexts);
                RETURN_ON_ERROR(e_result);
            }

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
    ResultType Visit(MStmt_Block* mStmt) 
    { 
        ScopeGuard mainGuard{contexts.bodyContext};

        auto e_result = TranslateMStmtsToQInsts(mStmt->stmts, contexts);
        RETURN_ON_ERROR(e_result);

        return {};
    }
    ResultType Visit(MStmt_Blank* mStmt) 
    { 
        return {};
    }
    ResultType Visit(MStmt_Exp* mStmt) 
    { 
        ScopeGuard mainGuard{contexts.bodyContext};

        auto e_result = TranslateMCreateToQInsts(mStmt->create, /*o_destSlotIndex*/nullopt, contexts);
        RETURN_ON_ERROR(e_result);

        return {};
    }
    // ResultType Visit(MStmt_Task* mStmt) { }
    // ResultType Visit(MStmt_Await* mStmt) { }
    // ResultType Visit(MStmt_Async* mStmt) { }
    // ResultType Visit(MStmt_Foreach* mStmt) { }
    // ResultType Visit(MStmt_Yield* mStmt) { }
    // ResultType Visit(MStmt_CallBaseClassCtor* mStmt) { }
    // ResultType Visit(MStmt_CallBaseStructCtor* mStmt) { }
    // ResultType Visit(MStmt_Directive* mStmt) { }
    // ResultType Visit(MStmt_Call* mStmt) { }
    // ResultType Visit(MStmt_Assign* mStmt) { }
    // ResultType Visit(MStmt_Do* mStmt) { }
};

expected<void, DiagPtr> TranslateMStmtsToQInsts(std::vector<MStmt*>& mStmts, QTranslationContexts& bodyContext)
{   
    for(auto* mStmt : mStmts)
    {
        // unreachable 처리는 Emit이 실제로 일어날때 처리해야 한다. 여기서 처리하지 않는다
        auto e_result = TranslateMStmtToQInsts(mStmt, bodyContext);
        RETURN_ON_ERROR(e_result);
    }

    return {};
}

expected<void, DiagPtr> TranslateMStmtsToQInstsWithNewScope(std::vector<MStmt*>& mStmts, QTranslationContexts& contexts)
{
    ScopeGuard guard{contexts.bodyContext};
    return TranslateMStmtsToQInsts(mStmts, contexts);
}

expected<void, DiagPtr> TranslateMStmtToQInsts(MStmt* mStmt, QTranslationContexts& contexts)
{
    MStmtQInstsTranslator translator{contexts};
    return Accept(translator, mStmt);
}

} // namespace Citron