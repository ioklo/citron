#include <gtest/gtest.h>

#include "Infra/Ptr.h"

#include "RSymbol/RModule.h"
#include "RSymbol/RFactory.h"
#include "NSymbol/NFactory.h"
#include "NSymbol/NGlobalFuncDecl.h"

#include "QEvaluator/QEvaluation.h"
#include "QIR/QFuncBody.h"
#include "QIR/QData.h"
#include "QIR/QFactory.h"
#include "QIR/QBlock.h"
#include "QIR/QArgs.h"
#include "QIR/QInsts.h"

using namespace std;
using namespace Citron;

TEST(TestCaseName, TestName) {
  EXPECT_EQ(1, 1);
  EXPECT_TRUE(true);
}

class NullCommandHandler : public IEvalQDataCommandHandler
{
    // Inherited via IEvalQDataCommandHandler
    void Execute(const std::string& str) override
    {
    }
};

// Scenario_Result
TEST(QEvaluator, DebugPrint_PrintWell)
{
    auto rFactory = MakePtr<RFactory>();
    NFactory nFactory{rFactory};
    QFactoryPtr qFactory = MakePtr<QFactory>();

    auto* nRootNamespace = nFactory.MakeRootNamespaceDecl();
    auto* nEntry = nFactory.MakeNDecl<NGlobalFuncDecl>(
        nRootNamespace, RAccessor::Public, 
        /*bSeqFunc*/false, 
        RName_Normal{"main"}, 
        /*typeParams*/vector<string>{});

    auto* qEntryBlock = qFactory->MakeQBlock("entry");
    std::vector<QArg> args{QArg_ConstInt32{1}};

    QInst_Intrinsic inst{QInst_IntrinsicKind::DebugPrint_Items, nullopt, move(args)};
    qEntryBlock->EmitInst(inst);
    qEntryBlock->EmitInst(QInst_ReturnVoid{});

    std::vector<QFuncBody> funcBodies;
    funcBodies.emplace_back(nEntry, vector<QSlotInfo>{}, qEntryBlock, 0);
    QData* qData = qFactory->MakeQData(move(funcBodies));
    
    auto eResult = EvaluateQData({}, qData, nEntry, MakePtr<NullCommandHandler>(), qFactory);
    EXPECT_TRUE(eResult);
}

TEST(QEvaluator, Translate)
{
}