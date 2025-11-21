#include "MStmtQInstsTranslation.h"

#include "Infra/Exceptions.h"
#include "Infra/Expected.h"

#include "MIR/MStmt.h"
#include "QIR/QFactory.h"
#include "QIR/QBlock.h"
#include "QIR/QInsts.h"

#include "CommonQInstsTranslation.h"
#include "MExpQInstsTranslation.h"
#include "QBodyContext.h"


using namespace std;

namespace Citron::IR0IR1Translator {

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
        vector<QArg> values;
        for(auto* command : stmt->commands)
        {
            auto eQValue = TranslateMExp_StringToQInsts(command, bodyContext);
            RETURN_ON_ERROR(eQValue);

            values.push_back(move(*eQValue));
        }

        bodyContext.AddIntrinsicVoid(QInst_IntrinsicKind::Command_Items, move(values));
        return {};
    }
    
    // 스택에 변수를 둔다.
    ResultType Visit(MStmt_LocalVarDecl* stmt)
    {
        auto lv = bodyContext.AddLocalVar(stmt->type, RName_Normal{stmt->name});

        auto eInitValue = TranslateMExpToQInsts(stmt->initExp, bodyContext);
        RETURN_ON_ERROR(eInitValue);

        size_t size = bodyContext.GetMExpTypeSize(stmt->initExp);
        bodyContext.AddInst(QInst_Assign{lv, *eInitValue, size});
        return {};
    }

    ResultType Visit(MStmt_If* stmt)
    {
        // 1. stmt.cond
        auto eCondV = TranslateMExpToQInsts(stmt->cond, bodyContext);
        if (!eCondV) return unexpected{eCondV.error()};

        if (!stmt->elseBody.empty())
        {
            // 2. add three blocks
            auto* trueBlock = bodyContext.AddBlock("if_true"); // debug label, b23_if_true
            auto* falseBlock = bodyContext.AddBlock("if_false");
            auto* endBlock = bodyContext.AddBlock("if_end");

            // 3. add conditional jump
            bodyContext.CompleteBlock(QInst_CondJump{*eCondV, trueBlock, falseBlock});

            // 4. fill trueBlock
            bodyContext.SetCurBlock(trueBlock);
            auto eTrueResult = TranslateMStmtsToQInsts(stmt->body, bodyContext);
            if (!eTrueResult) return unexpected{eTrueResult.error()};
            bodyContext.CompleteBlock(QInst_Jump{endBlock});

            // 5. fill falseBlock
            bodyContext.SetCurBlock(falseBlock);
            auto eFalseResult = TranslateMStmtsToQInsts(stmt->elseBody, bodyContext);
            if (!eFalseResult) return unexpected{eFalseResult.error()};
            bodyContext.CompleteBlock(QInst_Jump{endBlock});

            bodyContext.SetCurBlock(endBlock);
            return {};
        }
        else
        {
            // 2. add three blocks
            auto* trueBlock = bodyContext.AddBlock("if_true"); // debug label, b23_if_true
            auto* endBlock = bodyContext.AddBlock("if_end");

            // 3. add conditional jump
            bodyContext.CompleteBlock(QInst_CondJump{*eCondV, trueBlock, endBlock});

            // 4. fill trueBlock
            bodyContext.SetCurBlock(trueBlock);
            auto eNewTrueBlock = TranslateMStmtsToQInsts(stmt->body, bodyContext);
            if (!eNewTrueBlock) return unexpected{eNewTrueBlock.error()};
            bodyContext.CompleteBlock(QInst_Jump{endBlock});

            bodyContext.SetCurBlock(endBlock);
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
        bodyContext.CompleteBlock(QInst_ReturnVoid{});
        return {};
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
        auto eQValue = TranslateMExpToQInsts(stmt->exp, bodyContext);
        RETURN_ON_ERROR(eQValue);

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

expected<void, DiagPtr> TranslateMStmtToQInsts(MStmt* mStmt, QBodyContext& qBodyContext)
{
    MStmtQInstsTranslator translator{qBodyContext};
    return Accept(translator, mStmt);
}
} // namespace Citron::IIR0IR1Translator