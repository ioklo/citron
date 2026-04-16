#include "MStmtToQInsts.h"

#include "Infra/Exceptions.h"
#include "Infra/Expected.h"
#include "Infra/Variants.h"

#include "Logging/Diag.h"

#include "MIR/MStmt.h"
#include "MIR/MExp.h"
#include "MIR/MLoc.h"
#include "QIR/QFactory.h"
#include "QIR/QBlock.h"
#include "QIR/QInsts.h"

#include "CommonQInstsTranslation.h"
#include "MLocToQInsts.h"
#include "QBodyContext.h"
#include "QScopeGuard.h"
#include "MReadToQInsts.h"
#include "QTranslationContexts.h"
#include "MCreateToQInsts.h"
#include "QEmitState.h"
#include "QJumpBlockInfo.h"
#include "QLazyBlock.h"

using namespace std;

namespace Citron {

struct MStmtQInstsTranslator
{
    using ResultType = expected<QEmitState<void>, DiagPtr>;
    
    QTranslationContexts& contexts;

    expected<QEmitState<QReadResult>, DiagPtr> TranslateMTopLevel_ReadToQInsts(MTopLevel_Read& mTopLevelRead)
    {
        QScopeGuard guard{std::nullopt, contexts.bodyContext};
        return TranslateMReadToQInsts(mTopLevelRead.read, contexts);
    }

    expected<QEmitState<void>, DiagPtr> TranslateMTopLevel_CreateToQInsts(MTopLevel_Create& mTopLevelCreate, optional<size_t> o_destSlotIndex)
    {
        QScopeGuard scopeGuard{std::nullopt, contexts.bodyContext};
        return TranslateMCreateToQInsts(mTopLevelCreate.create, o_destSlotIndex, contexts);
    }

    expected<QEmitState<QLocResult>, DiagPtr> TranslateMTopLevel_LocToQInsts(MTopLevel_Loc& mTopLevelLoc)
    {
        QScopeGuard scopeGuard{std::nullopt, contexts.bodyContext};
        return TranslateMLocToQInsts(mTopLevelLoc.loc, contexts);
    }

    ResultType Visit(MStmt* mStmt) { throw NotImplementedException{}; }

    ResultType Visit(MStmt_Scope* mStmt)
    {
        return TranslateMStmt_ScopeToQInsts_Default(mStmt, contexts);
    }

    ResultType HandleCommand(MTopLevel_Command& topLevelCommand)
    {
        vector<QArg_Input> values;
        for (auto& mCommand : topLevelCommand.commands)
        {
            // Read니까. 이미 있는 slot을 돌려 받는다.
            auto e_s_readResult = TranslateMRead_LocToQInsts(mCommand, contexts);
            RETURN_ON_ERROR_OR_DONE(e_s_readResult);

            // NBC이기 때문에 (string) 인자로 넘겨줄 때는 pointer가 되어야 한다
            visit([this, &values](auto& readResult){
                using T = remove_cvref_t<decltype(readResult)>;
                if constexpr (same_as<T, QReadResult_Slot>)
                {
                    size_t ptrSlotIndex = contexts.bodyContext.NewSlot(contexts.bodyContext.GetPtrType());
                    contexts.bodyContext.EmitInst(QInst_AddrOf{QArg_Slot{ptrSlotIndex}, QArg_Slot{readResult.slotIndex}});
                    values.push_back(QArg_Slot{ptrSlotIndex});
                }
                else if constexpr (same_as<T, QReadResult_Ptr>)
                {
                    values.push_back(QArg_Slot{readResult.slotIndex});
                }
                else if constexpr (same_as<T, QReadResult_ConstInt32>) throw RuntimeFatalException{};
                else if constexpr (same_as<T, QReadResult_ConstBool>) throw RuntimeFatalException{};
                else static_assert(false);
            }, **e_s_readResult);
        }

        contexts.bodyContext.EmitIntrinsic(QInst_IntrinsicKind::Command_Items, nullopt, move(values));
        return QEmitState_Ready{};
    }
    
    ResultType Visit(MStmt_Command* mStmt) 
    {  
        QScopeGuard guard{std::nullopt, contexts.bodyContext}; // for MTopLevel_Command
        return HandleCommand(mStmt->command);
    }

    ResultType Visit(MStmt_LocalVarDecl* mStmt) 
    {
        auto slotIndex = contexts.bodyContext.AddLocalVar(mStmt->type, mStmt->name, /*o_argIndex*/nullopt);

        return visit([this, slotIndex](auto& init) -> ResultType {
            using T = remove_cvref_t<decltype(init)>;
            if constexpr (same_as<T, MStmt_LocalVarDeclInit_Uninit>) { return QEmitState_Ready{}; }
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
                return TranslateMTopLevel_CreateToQInsts(init.create, slotIndex);
            }
            else static_assert(false);

        }, mStmt->init);
    }

    ResultType Visit(MStmt_LocalRefDecl* mStmt) 
    {
        // auto slotIndex = bodyContext.AddLocalRef(stmt->type, stmt->name, nullopt);
        auto e_s_locResult = TranslateMTopLevel_LocToQInsts(mStmt->loc);
        RETURN_ON_ERROR_OR_DONE(e_s_locResult);

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
        }, **e_s_locResult);

        return QEmitState_Ready{};
    }
    
    ResultType HandleIf(MStmt_If* mStmt, size_t condSlotIndex)
    {
        auto& bodyContext = contexts.bodyContext;

        if (mStmt->falseBody)
        {
            // 2. add three blocks
            auto* trueBlock = bodyContext.AddBlock("if_true"); // debug label, b23_if_true
            auto* falseBlock = bodyContext.AddBlock("if_false");
            QBlock* endBlock = nullptr; // lazy init

            // 3. add conditional jump
            bodyContext.EmitTermInst(QInst_CondJump{condSlotIndex, trueBlock, falseBlock});

            // 4. fill trueBlock
            bodyContext.SetCurBlock(trueBlock);

            auto e_s_trueResult = TranslateMStmt_ScopeToQInsts_Default(mStmt->trueBody, contexts);
            RETURN_ON_ERROR(e_s_trueResult);

            if (*e_s_trueResult)
            {
                if (!endBlock) endBlock = contexts.bodyContext.AddBlock("if_end");
                bodyContext.EmitTermInst(QInst_Jump{endBlock});
            }

            // 5. fill falseBlock
            bodyContext.SetCurBlock(falseBlock);

            auto e_falseResult = TranslateMStmt_ScopeToQInsts_Default(mStmt->falseBody, contexts);
            RETURN_ON_ERROR(e_falseResult);

            if (*e_falseResult)
            {
                if (!endBlock) endBlock = bodyContext.AddBlock("if_end");
                bodyContext.EmitTermInst(QInst_Jump{endBlock});
            }

            // 만약 endBlock이 없으면 unreachable상태이고, 그럼 그냥 둔다
            if (endBlock)
            {
                bodyContext.SetCurBlock(endBlock);
                return QEmitState_Ready{};
            }
            else
            {
                return QEmitState_Done{};
            }
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
            auto e_trueBlockResult = TranslateMStmt_ScopeToQInsts_Default(mStmt->trueBody, contexts);
            RETURN_ON_ERROR(e_trueBlockResult);

            if (*e_trueBlockResult)
                bodyContext.EmitTermInst(QInst_Jump{endBlock});

            bodyContext.SetCurBlock(endBlock);
            return QEmitState_Ready{};
        }
    }

    ResultType Visit(MStmt_If* mStmt) 
    {
        // 1. stmt.o_cond
        auto e_s_condResult = TranslateMTopLevel_ReadToQInsts(mStmt->cond);
        RETURN_ON_ERROR_OR_DONE(e_s_condResult);
        
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
                    return TranslateMStmt_ScopeToQInsts_Default(mStmt->trueBody, contexts);
                }
                else
                {
                    if (mStmt->falseBody)
                    {
                        // false block을 만들 필요도 없다
                        return TranslateMStmt_ScopeToQInsts_Default(mStmt->falseBody, contexts);
                    }
                }

                return QEmitState_Ready{};
            }
            else if constexpr (same_as<T, QReadResult_ConstInt32>)
                throw RuntimeFatalException{}; // SyntaxIR0 Translation에서 이미 체크가 되었어야 한다
            else static_assert(false);
        }, **e_s_condResult);
    }

    // label: for(initStmts; o_cond; contStmt) body
    ResultType Visit(MStmt_For* mStmt) // init이 빠진 형태. init은 MStmt_For가 있는 scope에 따로 있고, 여기에는 cond, cont, body만 있다
    {
        QBodyContext& bodyContext = contexts.bodyContext;
        
        QBlock* bodyBlock = bodyContext.AddBlock("for_body");
        QBlock* exitBlock = bodyContext.AddBlock("for_exit");

        // cond용 block
        QBlock* condBlock;
        if (mStmt->o_cond)
        {
            condBlock = bodyContext.AddBlock("for_cond");
            bodyContext.EmitTermInst(QInst_Jump{condBlock});
            bodyContext.SetCurBlock(condBlock);

            auto e_s_condResult = TranslateMTopLevel_ReadToQInsts(*mStmt->o_cond);
            RETURN_ON_ERROR_OR_DONE(e_s_condResult);
                
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
            }, **e_s_condResult);
        }
        else
        {
            condBlock = bodyBlock;
            // cond가 없으면 무조건 jump한다
            bodyContext.EmitTermInst(QInst_Jump{bodyBlock});
        }
        
        // cont block 만들기
        QBlock* contBlock; 
        if (mStmt->contStmt)
        {
            contBlock = bodyContext.AddBlock("for_cont");
            bodyContext.SetCurBlock(contBlock);
            auto e_s_contResult = TranslateMStmtToQInsts(mStmt->contStmt, contexts);
            RETURN_ON_ERROR(e_s_contResult);

            if (!*e_s_contResult)
            {
                bodyContext.SetCurBlock(exitBlock);
                return QEmitState_Ready{};
            }

            bodyContext.EmitTermInst(QInst_Jump{condBlock});
        }
        else
            contBlock = condBlock;

        // body block 채우기
        bodyContext.SetCurBlock(bodyBlock);
        // continue는 contBlock, break는 exitBlock으로 지정해준다
        auto e_s_bodyResult = TranslateMStmt_ScopeToQInsts_Loop(mStmt->body, contBlock, exitBlock, contexts);
        RETURN_ON_ERROR(e_s_bodyResult);

        // body가 unreachable상태이면, 그 상태 그대로 두고, curBlock을 for_exit블록으로 만들고 ready를 리턴한다
        // cond에 따라서 body로 들어가지 않을수도 있기 때문에 계속 진행한다
        if (!*e_s_bodyResult)
        {
            bodyContext.SetCurBlock(exitBlock);
            return QEmitState_Ready{};
        }
                
        bodyContext.EmitTermInst(QInst_Jump{contBlock});

        bodyContext.SetCurBlock(exitBlock);
        return QEmitState_Ready{};
    }

    ResultType Visit(MStmt_Continue* mStmt) 
    {
        contexts.bodyContext.EmitJumpToCleanUpBlock(QCleanUpInfoKey_Continue{mStmt->labelId});
        return QEmitState_Done{};
    }

    ResultType Visit(MStmt_Break* mStmt)
    {
        contexts.bodyContext.EmitJumpToCleanUpBlock(QCleanUpInfoKey_Break{mStmt->labelId});
        return QEmitState_Done{};
    }

    ResultType Visit(MStmt_Leave* mStmt) 
    {
        auto& bodyContext = contexts.bodyContext;
        
        auto e_s_result = TranslateMTopLevel_CreateToQInsts(mStmt->create, bodyContext.GetLeaveSlotIndex(mStmt->labelId));
        RETURN_ON_ERROR_OR_DONE(e_s_result);

        bodyContext.EmitJumpToCleanUpBlock(QCleanUpInfoKey_Leave{mStmt->labelId});
        return QEmitState_Done{};
    }

    ResultType Visit(MStmt_Return* mStmt) 
    { 
        auto& bodyContext = contexts.bodyContext;

        if (mStmt->create)
        {
            auto e_s_result = TranslateMTopLevel_CreateToQInsts(*mStmt->create, bodyContext.GetRetSlotIndex());
            RETURN_ON_ERROR_OR_DONE(e_s_result);

            bodyContext.EmitJumpToCleanUpBlock(QCleanUpInfoKey_Return{});
            bodyContext.MarkReturnHandledOnCurScope();
            return QEmitState_Done{};
        }
        else
        {
            bodyContext.EmitJumpToCleanUpBlock(QCleanUpInfoKey_Return{});
            bodyContext.MarkReturnHandledOnCurScope();
            return QEmitState_Done{};
        }
    }
    
    ResultType Visit(MStmt_Blank* mStmt) 
    { 
        return QEmitState_Ready{};
    }

    ResultType Visit(MStmt_Exp* mStmt) 
    {
        return TranslateMTopLevel_CreateToQInsts(mStmt->create, /*o_destSlotIndex*/nullopt);
    }
    // ResultType Visit(MStmt_Task* mStmt) { }
    // ResultType Visit(MStmt_Await* mStmt) { }
    // ResultType Visit(MStmt_Async* mStmt) { }
    // ResultType Visit(MStmt_Foreach* mStmt) { }
    // ResultType Visit(MStmt_Yield* mStmt) { }
    // ResultType Visit(MStmt_Directive* mStmt) { }

    ResultType HandleCall(MTopLevel_Call& call)
    {
        vector<QArg_Input> args;

        // 1. 인자를 args에 넣는다
        auto e_s_args = TranslateMArgumentsToQInsts(call.args, contexts);
        RETURN_ON_ERROR_OR_DONE(e_s_args);

        // 2. Emit처리
        auto* rFuncDecl = GetRFuncDecl(call.callable);
        auto* retType = GetType(call.callable);
        contexts.bodyContext.EmitInst(QInst_Call{rFuncDecl, nullopt, move(**e_s_args)});

        return QEmitState_Ready{};
    }

    ResultType Visit(MStmt_Call* mStmt) 
    {
        QScopeGuard scopeGuard{std::nullopt, contexts.bodyContext};
        return HandleCall(mStmt->call);
    }

    ResultType HandleAssign(MTopLevel_Assign& assign)
    {   
        // TODO: [61] 일반적인 struct ctor, dtor, copy/move ctor, copy/move assign 구현
        auto* type = GetType(assign.dest, &*contexts.rFactory);
        if (type != contexts.bodyContext.GetStringType())
            throw NotImplementedException{};

        auto e_s_dest = TranslateMLocToQInsts(assign.dest, contexts);
        RETURN_ON_ERROR_OR_DONE(e_s_dest);
        size_t destPtrSlotIndex = MakePtrSlot(**e_s_dest, contexts);

        return visit([this, destPtrSlotIndex](auto& assignKind) -> ResultType {
            using T = remove_cvref_t<decltype(assignKind)>;
            if constexpr (same_as<T, MStmt_AssignKind_Copy>)
            {
                auto e_s_src = TranslateMLocToQInsts(assignKind.src.loc, contexts);
                RETURN_ON_ERROR_OR_DONE(e_s_src);

                size_t srcPtrSlotIndex = MakePtrSlot(**e_s_src, contexts);
                contexts.bodyContext.EmitIntrinsic(QInst_IntrinsicKind::CopyAssign_StringPtr_StringPtr_Void, nullopt, {QArg_Slot{destPtrSlotIndex}, QArg_Slot{srcPtrSlotIndex}});

                return QEmitState_Ready{};
            }
            else if constexpr (same_as<T, MStmt_AssignKind_Move>)
            {
                throw NotImplementedException{};
            }
            else static_assert(false);
        }, assign.kind);
    }

    ResultType Visit(MStmt_Assign* mStmt) 
    {
        QScopeGuard scopeGuard{std::nullopt, contexts.bodyContext};
        return HandleAssign(mStmt->assign);
    }
    // ResultType Visit(MStmt_Do* mStmt) { }
};

expected<QEmitState<void>, DiagPtr> TranslateMStmtsToQInsts(std::vector<MStmt*>& mStmts, QTranslationContexts& bodyContext)
{   
    for(auto* mStmt : mStmts)
    {
        // unreachable 처리는 Emit이 실제로 일어날때 처리해야 한다. 여기서 처리하지 않는다
        auto e_s_result = TranslateMStmtToQInsts(mStmt, bodyContext);
        RETURN_ON_ERROR_OR_DONE(e_s_result);
    }

    return QEmitState_Ready{};
}

expected<QEmitState<void>, DiagPtr> TranslateMStmt_ScopeToQInsts_Default(MStmt_Scope* scope, QTranslationContexts& contexts)
{
    QScopeGuard guard{std::nullopt, contexts.bodyContext};
    return TranslateMStmtsToQInsts(scope->stmts, contexts);
}

expected<QEmitState<void>, DiagPtr> TranslateMStmt_ScopeToQInsts_Loop(MStmt_Scope* scope, QBlock* contBlock, QBlock* breakBlock, QTranslationContexts& contexts)
{
    auto& scopeKind = get<MScopeKind_Loop>(scope->scopeKind);
    QScopeGuard guard{scopeKind.labelId, contexts.bodyContext};
    QJumpBlockScopeGuard jumpBlockGuard{QJumpBlockInfo_Loop{scopeKind.labelId, contBlock, breakBlock}, contexts.bodyContext};
    return TranslateMStmtsToQInsts(scope->stmts, contexts);
}

expected<QEmitState<void>, DiagPtr> TranslateMStmt_ScopeToQInsts_Switch(MStmt_Scope* scope, QBlock* breakBlock, QTranslationContexts& contexts)
{
    auto& scopeKind = get<MScopeKind_Switch>(scope->scopeKind);
    QScopeGuard guard{scopeKind.labelId, contexts.bodyContext};
    QJumpBlockScopeGuard jumpBlockGuard{QJumpBlockInfo_Switch{scopeKind.labelId, breakBlock}, contexts.bodyContext};
    return TranslateMStmtsToQInsts(scope->stmts, contexts);
}

expected<QEmitState<void>, DiagPtr> TranslateMStmt_ScopeToQInsts_Inline(MStmt_Scope* scope, const QLazyBlockPtr& leaveBlock, size_t destSlotIndex, QTranslationContexts& contexts)
{
    auto& scopeKind = get<MScopeKind_Inline>(scope->scopeKind);
    QScopeGuard guard{scopeKind.labelId, contexts.bodyContext};
    QJumpBlockScopeGuard jumpBlockGuard{QJumpBlockInfo_Inline{scopeKind.labelId, leaveBlock, destSlotIndex}, contexts.bodyContext};
    return TranslateMStmtsToQInsts(scope->stmts, contexts);
}

expected<QEmitState<void>, DiagPtr> TranslateMStmtToQInsts(MStmt* mStmt, QTranslationContexts& contexts)
{
    MStmtQInstsTranslator translator{contexts};
    return Accept(translator, mStmt);
}

} // namespace Citron