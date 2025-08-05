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
    QBlock* block;
    QBodyContext* bodyContext;
    QFactory* factory;

public:
    NStmtQInstsTranslator(QBlock* block, QBodyContext* bodyContext, QFactory* factory)
        : block{block}, bodyContext{bodyContext}, factory{factory} { }

    // command는 일단 넘깁시다
    expected<QBlock*, DiagPtr> Translate(NStmt_Command& stmt)
    {
        throw NotImplementedException{};
    }
    expected<QBlock*, DiagPtr> Translate(NStmt_LocalVarDecl& stmt)
    {
        throw NotImplementedException{};
    }
    expected<QBlock*, DiagPtr> Translate(NStmt_If& stmt)
    {
        // 1. stmt.cond
        auto eCondV = TranslateNExpToQInsts(stmt.cond.get(), block, bodyContext, factory);
        if (!eCondV) return unexpected{eCondV.error()};

        if (!stmt.elseBody.empty())
        {
            // 2. add three blocks
            auto* trueBlock = bodyContext->AddBlock("if_true"); // debug label, b23_if_true
            auto* falseBlock = bodyContext->AddBlock("if_false");
            auto* endBlock = bodyContext->AddBlock("if_end");

            // 3. add conditional jump
            auto* condJumpInst = factory->MakeQJumpInst_CondJump(*eCondV, trueBlock, falseBlock);
            block->SetTerminator(condJumpInst);

            // 4. fill trueBlock
            auto eNewTrueBlock = TranslateNStmtsToQInsts(stmt.body, trueBlock, bodyContext, factory);
            if (!eNewTrueBlock) return unexpected{eNewTrueBlock.error()};
            auto* trueTerminator = factory->MakeQJumpInst_Jump(endBlock);
            (*eNewTrueBlock)->SetTerminator(trueTerminator);

            // 5. fill falseBlock
            auto eNewFalseBlock = TranslateNStmtsToQInsts(stmt.elseBody, falseBlock. bodyContext, factory);
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
            auto eNewTrueBlock = TranslateNStmtsToQInsts(stmt.body, trueBlock, bodyContext, factory);
            if (!eNewTrueBlock) return unexpected{eNewTrueBlock.error()};
            auto* trueTerminator = factory->MakeQJumpInst_Jump(endBlock);
            (*eNewTrueBlock)->SetTerminator(trueTerminator);

            return endBlock;
        }
    }
    expected<QBlock*, DiagPtr> Translate(NStmt_IfNullableRefTest& stmt)
    {
        throw NotImplementedException{};
    }
    expected<QBlock*, DiagPtr> Translate(NStmt_IfNullableValueTest& stmt)
    {
        throw NotImplementedException{};
    }
    expected<QBlock*, DiagPtr> Translate(NStmt_For& stmt)
    {
        throw NotImplementedException{};
    }
    expected<QBlock*, DiagPtr> Translate(NStmt_Continue& stmt)
    {
        throw NotImplementedException{};
    }
    expected<QBlock*, DiagPtr> Translate(NStmt_Break& stmt)
    {
        throw NotImplementedException{};
    }
    expected<QBlock*, DiagPtr> Translate(NStmt_Return& stmt)
    {
        throw NotImplementedException{};
    }
    expected<QBlock*, DiagPtr> Translate(NStmt_Block& stmt)
    {
        throw NotImplementedException{};
    }
    expected<QBlock*, DiagPtr> Translate(NStmt_Blank& stmt)
    {
        throw NotImplementedException{};
    }
    expected<QBlock*, DiagPtr> Translate(NStmt_Exp& stmt)
    {
        throw NotImplementedException{};
    }
    expected<QBlock*, DiagPtr> Translate(NStmt_Task& stmt)
    {
        throw NotImplementedException{};
    }
    expected<QBlock*, DiagPtr> Translate(NStmt_Await& stmt)
    {
        throw NotImplementedException{};
    }
    expected<QBlock*, DiagPtr> Translate(NStmt_Async& stmt)
    {
        throw NotImplementedException{};
    }
    expected<QBlock*, DiagPtr> Translate(NStmt_Foreach& stmt)
    {
        throw NotImplementedException{};
    }
    expected<QBlock*, DiagPtr> Translate(NStmt_ForeachCast& stmt)
    {
        throw NotImplementedException{};
    }
    expected<QBlock*, DiagPtr> Translate(NStmt_Yield& stmt)
    {
        throw NotImplementedException{};
    }
    expected<QBlock*, DiagPtr> Translate(NStmt_CallClassCtor& stmt)
    {
        throw NotImplementedException{};
    }
    expected<QBlock*, DiagPtr> Translate(NStmt_CallStructCtor& stmt)
    {
        throw NotImplementedException{};
    }
    expected<QBlock*, DiagPtr> Translate(NStmt_NullDirective& stmt)
    {
        throw NotImplementedException{};
    }
    expected<QBlock*, DiagPtr> Translate(NStmt_NotNullDirective& stmt)
    {
        throw NotImplementedException{};
    }
    expected<QBlock*, DiagPtr> Translate(NStmt_StaticNullDirective& stmt)
    {
        throw NotImplementedException{};
    }
    expected<QBlock*, DiagPtr> Translate(NStmt_StaticNotNullDirective& stmt)
    {
        throw NotImplementedException{};
    }
    expected<QBlock*, DiagPtr> Translate(NStmt_StaticUnknownNullDirective& stmt)
    {
        throw NotImplementedException{};
    }
};

class NStmtQInstsTranslatorWrpper : public NStmtVisitor
{
    expected<QBlock*, DiagPtr>* result;
    NStmtQInstsTranslator translator;

public:
    NStmtQInstsTranslatorWrpper(expected<QBlock*, DiagPtr>* result, QBlock* block, QBodyContext* bodyContext, QFactory* factory)
        : result{result}, translator{block, bodyContext, factory}
    { }

public:
    void Visit(NStmt_Command& stmt) override { *result = translator.Translate(stmt); }
    void Visit(NStmt_LocalVarDecl& stmt) override { *result = translator.Translate(stmt); }
    void Visit(NStmt_If& stmt) override { *result = translator.Translate(stmt); }
    void Visit(NStmt_IfNullableRefTest& stmt) override { *result = translator.Translate(stmt); }
    void Visit(NStmt_IfNullableValueTest& stmt) override { *result = translator.Translate(stmt); }
    void Visit(NStmt_For& stmt) override { *result = translator.Translate(stmt); }
    void Visit(NStmt_Continue& stmt) override { *result = translator.Translate(stmt); }
    void Visit(NStmt_Break& stmt) override { *result = translator.Translate(stmt); }
    void Visit(NStmt_Return& stmt) override { *result = translator.Translate(stmt); }
    void Visit(NStmt_Block& stmt) override { *result = translator.Translate(stmt); }
    void Visit(NStmt_Blank& stmt) override { *result = translator.Translate(stmt); }
    void Visit(NStmt_Exp& stmt) override { *result = translator.Translate(stmt); }
    void Visit(NStmt_Task& stmt) override { *result = translator.Translate(stmt); }
    void Visit(NStmt_Await& stmt) override { *result = translator.Translate(stmt); }
    void Visit(NStmt_Async& stmt) override { *result = translator.Translate(stmt); }
    void Visit(NStmt_Foreach& stmt) override { *result = translator.Translate(stmt); }
    void Visit(NStmt_ForeachCast& stmt) override { *result = translator.Translate(stmt); }
    void Visit(NStmt_Yield& stmt) override { *result = translator.Translate(stmt); }
    void Visit(NStmt_CallClassCtor& stmt) override { *result = translator.Translate(stmt); }
    void Visit(NStmt_CallStructCtor& stmt) override { *result = translator.Translate(stmt); }
    void Visit(NStmt_NullDirective& stmt) override { *result = translator.Translate(stmt); }
    void Visit(NStmt_NotNullDirective& stmt) override { *result = translator.Translate(stmt); }
    void Visit(NStmt_StaticNullDirective& stmt) override { *result = translator.Translate(stmt); }
    void Visit(NStmt_StaticNotNullDirective& stmt) override { *result = translator.Translate(stmt); }
    void Visit(NStmt_StaticUnknownNullDirective& stmt) override { *result = translator.Translate(stmt); }
};

expected<QBlock*, DiagPtr> TranslateNStmtToQInsts(NStmt* nStmt, QBlock* block, QBodyContext* bodyContext, QFactory* factory)
{
    expected<QBlock*, DiagPtr> result;
    NStmtQInstsTranslatorWrpper wrapper{&result, block, bodyContext, factory};
    nStmt->Accept(wrapper);
    return result;
}
} // namespace Citron::IIR0IR1Translator