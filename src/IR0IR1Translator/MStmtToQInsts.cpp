#include "MStmtToQInsts.h"

#include "Infra/Exceptions.h"
#include "Infra/Expected.h"
#include "Infra/Variants.h"

#include "Logging/Diag.h"
#include "RSymbol/RTypes.h"
#include "MIR/MStmt.h"
#include "MIR/MExp.h"
#include "MIR/MLoc.h"
#include "MIR/MCreate.h"
#include "QIR/QFactory.h"
#include "QIR/QBlock.h"
#include "QIR/QInsts.h"

#include "CommonQInstsTranslation.h"
#include "MLocToQInsts.h"
#include "MqBodyContext.h"
#include "MqScopeGuard.h"
#include "MReadToQInsts.h"
#include "MqTranslationContexts.h"
#include "MCreateToQInsts.h"
#include "MqEmitState.h"
#include "MqJumpBlockInfo.h"
#include "MqLazyBlock.h"

using namespace std;

namespace Citron {

struct MStmtQInstsTranslator
{
    using ResultType = expected<MqEmitState<void>, DiagPtr>;
    
    MqTranslationContexts& contexts;

    expected<MqEmitState<QReadResult>, DiagPtr> TranslateMTopLevel_ReadToQInsts(MTopLevel_Read& mTopLevelRead)
    {
        MqScopeGuard guard{std::nullopt, contexts.bodyContext};
        return TranslateMReadToQInsts(mTopLevelRead.read, contexts);
    }

    expected<MqEmitState<void>, DiagPtr> TranslateMTopLevel_CreateToQInsts(MTopLevel_Create& mTopLevelCreate, MqCreateTarget createTarget)
    {
        MqScopeGuard scopeGuard{std::nullopt, contexts.bodyContext};
        return TranslateMCreateToQInsts(mTopLevelCreate.create, createTarget, contexts);
    }

    expected<MqEmitState<QLocResult>, DiagPtr> TranslateMTopLevel_LocToQInsts(MTopLevel_Loc& mTopLevelLoc)
    {
        MqScopeGuard scopeGuard{std::nullopt, contexts.bodyContext};
        return TranslateMLocToQInsts(mTopLevelLoc.loc, contexts);
    }

    ResultType Visit(MStmt* mStmt) { throw NotImplementedException{}; }

    ResultType Visit(MStmt_Scope* mStmt)
    {
        return TranslateMStmt_ScopeToQInsts_Default(mStmt, contexts);
    }

    ResultType HandleCommand(MTopLevel_Command& topLevelCommand)
    {   
        for (auto& mCommand : topLevelCommand.commands)
        {
            // Read니까. 이미 있는 slot을 돌려 받는다.
            auto e_s_readResult = TranslateMRead_LocToQInsts(mCommand, contexts);
            RETURN_ON_ERROR_OR_DONE(e_s_readResult);

            // NBC이기 때문에 (string) 인자로 넘겨줄 때는 pointer가 되어야 한다
            visit([this](auto& readResult){
                using T = remove_cvref_t<decltype(readResult)>;
                if constexpr (same_as<T, QReadResult_Slot>)
                {   
                    contexts.bodyContext.EmitIntrinsic(QInst_IntrinsicKind::Command_Item, nullopt, {QArg_CallArg_AddrOfSlot{readResult.slotIndex}});
                }
                else if constexpr (same_as<T, QReadResult_Ptr>)
                {
                    contexts.bodyContext.EmitIntrinsic(QInst_IntrinsicKind::Command_Item, nullopt, {QArg_CallArg_Slot{readResult.slotIndex}});
                }
                else if constexpr (same_as<T, QReadResult_ConstInt32>) throw RuntimeFatalException{};
                else if constexpr (same_as<T, QReadResult_ConstBool>) throw RuntimeFatalException{};
                else static_assert(false);
            }, **e_s_readResult);
        }

        
        return MqEmitState_Ready{};
    }
    
    ResultType Visit(MStmt_Command* mStmt) 
    {  
        MqScopeGuard guard{std::nullopt, contexts.bodyContext}; // for MTopLevel_Command
        return HandleCommand(mStmt->command);
    }

    ResultType Visit(MStmt_LocalVarDecl* mStmt) 
    {
        auto slotIndex = contexts.bodyContext.AddLocalVar(mStmt->type, mStmt->name);

        return visit([this, slotIndex](auto& init) -> ResultType {
            using T = remove_cvref_t<decltype(init)>;
            if constexpr (same_as<T, MStmt_LocalVarDeclInit_Uninit>) { return MqEmitState_Ready{}; }
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
                return TranslateMTopLevel_CreateToQInsts(init.create, MqCreateTarget_Slot{slotIndex});
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

        return MqEmitState_Ready{};
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
                return MqEmitState_Ready{};
            }
            else
            {
                return MqEmitState_Done{};
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
            return MqEmitState_Ready{};
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
                size_t newSlot = bodyContext.AddTemp(boolType, "if_cond");
                bodyContext.EmitInst(QInst_Load{.type = boolType, .dest = QArg_Dest_Slot{newSlot}, .src = condResult.slotIndex});

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

                return MqEmitState_Ready{};
            }
            else if constexpr (same_as<T, QReadResult_ConstInt32>)
                throw RuntimeFatalException{}; // SyntaxIR0 Translation에서 이미 체크가 되었어야 한다
            else static_assert(false);
        }, **e_s_condResult);
    }

    // label: for(initStmts; o_cond; contStmt) body
    ResultType Visit(MStmt_For* mStmt) // init이 빠진 형태. init은 MStmt_For가 있는 scope에 따로 있고, 여기에는 cond, cont, body만 있다
    {
        MqBodyContext& bodyContext = contexts.bodyContext;
        
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
                    size_t newSlot = bodyContext.AddTemp(boolType, "if_cond");
                    bodyContext.EmitInst(QInst_Load{.type = boolType, .dest = QArg_Dest_Slot{newSlot}, .src = condResult.slotIndex});
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
                return MqEmitState_Ready{};
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
            return MqEmitState_Ready{};
        }
                
        bodyContext.EmitTermInst(QInst_Jump{contBlock});

        bodyContext.SetCurBlock(exitBlock);
        return MqEmitState_Ready{};
    }

    ResultType Visit(MStmt_Continue* mStmt) 
    {
        contexts.bodyContext.EmitJumpToCleanUpBlock(MqCleanUpKind_Continue{mStmt->labelId});
        return MqEmitState_Done{};
    }

    ResultType Visit(MStmt_Break* mStmt)
    {
        contexts.bodyContext.EmitJumpToCleanUpBlock(MqCleanUpKind_Break{mStmt->labelId});
        return MqEmitState_Done{};
    }

    ResultType Visit(MStmt_Leave* mStmt) 
    {
        auto& bodyContext = contexts.bodyContext;
        
        auto o_leaveSlotIndex = bodyContext.GetLeaveSlotIndex(mStmt->labelId);
        auto createTarget = o_leaveSlotIndex ? MqCreateTarget{MqCreateTarget_Slot{*o_leaveSlotIndex}} : MqCreateTarget_Discard{};
        auto e_s_result = TranslateMTopLevel_CreateToQInsts(mStmt->create, std::move(createTarget));
        RETURN_ON_ERROR_OR_DONE(e_s_result);

        bodyContext.EmitJumpToCleanUpBlock(MqCleanUpKind_Leave{mStmt->labelId});
        return MqEmitState_Done{};
    }

    ResultType Visit(MStmt_Return* mStmt) 
    { 
        auto& bodyContext = contexts.bodyContext;

        if (mStmt->create)
        {
            auto* createType = GetType(mStmt->create->create, &*contexts.rFactory);

            switch (createType->GetCopyStrategy())
            {
            case RCopyStrategy::Void: assert(false);
            case RCopyStrategy::Bitwise:
            {
                auto e_s_result = TranslateMTopLevel_CreateToQInsts(*mStmt->create, MqCreateTarget_DirectReturn{});
                RETURN_ON_ERROR_OR_DONE(e_s_result);
                break;
            }
            case RCopyStrategy::NonBitwise:
            {
                assert(std::holds_alternative<QSlotRole_IndirectReturn>(bodyContext.GetSlotRole(0)));

                auto e_s_result = TranslateMTopLevel_CreateToQInsts(*mStmt->create, MqCreateTarget_Slot{0});
                RETURN_ON_ERROR_OR_DONE(e_s_result);
                break;
            }
            }

            bodyContext.EmitJumpToCleanUpBlock(MqCleanUpKind_Return{});
            return MqEmitState_Done{};
        }
        else
        {
            bodyContext.EmitJumpToCleanUpBlock(MqCleanUpKind_Return{});
            return MqEmitState_Done{};
        }
    }
    
    ResultType Visit(MStmt_Blank* mStmt) 
    { 
        return MqEmitState_Ready{};
    }

    ResultType Visit(MStmt_Exp* mStmt) 
    {
        return TranslateMTopLevel_CreateToQInsts(mStmt->create, MqCreateTarget_Discard{});
    }
    // ResultType Visit(MStmt_Task* mStmt) { }
    // ResultType Visit(MStmt_Await* mStmt) { }
    // ResultType Visit(MStmt_Async* mStmt) { }
    // ResultType Visit(MStmt_Foreach* mStmt) { }
    // ResultType Visit(MStmt_Yield* mStmt) { }
    // ResultType Visit(MStmt_Directive* mStmt) { }

    ResultType HandleCall(MTopLevel_Call& call)
    {
        auto e_s_o_retSlotIndex = Citron::HandleCall(call.callable.decl, call.callable.typeArgs, MqCreateTarget_Discard{}, call.callable.o_instance, call.args, contexts);
        RETURN_ON_ERROR_OR_DONE(e_s_o_retSlotIndex);

        assert(!**e_s_o_retSlotIndex); // void 함수이므로, slot이 나와서는 안 된다
        return MqEmitState_Ready{};
    }

    ResultType Visit(MStmt_Call* mStmt) 
    {
        MqScopeGuard scopeGuard{std::nullopt, contexts.bodyContext};
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

        auto destArg = MakeAddrCallArg(**e_s_dest, contexts);
        return visit([this, destArg](auto& assignKind) -> ResultType {
            using T = remove_cvref_t<decltype(assignKind)>;
            if constexpr (same_as<T, MStmt_AssignKind_Copy>)
            {
                auto e_s_src = TranslateMLocToQInsts(assignKind.src.loc, contexts);
                RETURN_ON_ERROR_OR_DONE(e_s_src);

                auto srcArg = MakeAddrCallArg(**e_s_src, contexts);
                contexts.bodyContext.EmitIntrinsic(QInst_IntrinsicKind::CopyAssign_Void_StringRef_StringInRef, nullopt, {destArg, srcArg});

                return MqEmitState_Ready{};
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
        MqScopeGuard scopeGuard{std::nullopt, contexts.bodyContext};
        return HandleAssign(mStmt->assign);
    }
    // ResultType Visit(MStmt_Do* mStmt) { }
};

expected<MqEmitState<void>, DiagPtr> TranslateMStmtsToQInsts(std::vector<MStmt*>& mStmts, MqTranslationContexts& bodyContext)
{   
    for(auto* mStmt : mStmts)
    {
        // unreachable 처리는 Emit이 실제로 일어날때 처리해야 한다. 여기서 처리하지 않는다
        auto e_s_result = TranslateMStmtToQInsts(mStmt, bodyContext);
        RETURN_ON_ERROR_OR_DONE(e_s_result);
    }

    return MqEmitState_Ready{};
}

expected<MqEmitState<void>, DiagPtr> TranslateMStmt_ScopeToQInsts_Default(MStmt_Scope* scope, MqTranslationContexts& contexts)
{
    MqScopeGuard guard{std::nullopt, contexts.bodyContext};
    return TranslateMStmtsToQInsts(scope->stmts, contexts);
}

expected<MqEmitState<void>, DiagPtr> TranslateMStmt_ScopeToQInsts_Loop(MStmt_Scope* scope, QBlock* contBlock, QBlock* breakBlock, MqTranslationContexts& contexts)
{
    auto& scopeKind = get<MScopeKind_Loop>(scope->scopeKind);
    MqScopeGuard guard{scopeKind.labelId, contexts.bodyContext};
    MqJumpBlockScopeGuard jumpBlockGuard{MqJumpBlockInfo_Loop{scopeKind.labelId, contBlock, breakBlock}, contexts.bodyContext};
    return TranslateMStmtsToQInsts(scope->stmts, contexts);
}

expected<MqEmitState<void>, DiagPtr> TranslateMStmt_ScopeToQInsts_Switch(MStmt_Scope* scope, QBlock* breakBlock, MqTranslationContexts& contexts)
{
    auto& scopeKind = get<MScopeKind_Switch>(scope->scopeKind);
    MqScopeGuard guard{scopeKind.labelId, contexts.bodyContext};
    MqJumpBlockScopeGuard jumpBlockGuard{MqJumpBlockInfo_Switch{scopeKind.labelId, breakBlock}, contexts.bodyContext};
    return TranslateMStmtsToQInsts(scope->stmts, contexts);
}

expected<MqEmitState<void>, DiagPtr> TranslateMStmt_ScopeToQInsts_Inline(MStmt_Scope* scope, const MqLazyBlockPtr& leaveBlock, size_t destSlotIndex, MqTranslationContexts& contexts)
{
    auto& scopeKind = get<MScopeKind_Inline>(scope->scopeKind);
    MqScopeGuard guard{scopeKind.labelId, contexts.bodyContext};
    MqJumpBlockScopeGuard jumpBlockGuard{MqJumpBlockInfo_Inline{scopeKind.labelId, leaveBlock, destSlotIndex}, contexts.bodyContext};
    return TranslateMStmtsToQInsts(scope->stmts, contexts);
}

expected<MqEmitState<void>, DiagPtr> TranslateMStmtToQInsts(MStmt* mStmt, MqTranslationContexts& contexts)
{
    MStmtQInstsTranslator translator{contexts};
    return Accept(translator, mStmt);
}

} // namespace Citron