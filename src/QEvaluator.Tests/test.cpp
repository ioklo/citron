#include <gtest/gtest.h>

#include "RSymbol/RModule.h"
#include "RSymbol/RFactory.h"
#include "NSymbol/NFactory.h"
#include "NSymbol/NGlobalFuncDecl.h"

#include "QEvaluator/QEvaluation.h"
#include "QIR/QFuncBody.h"
#include "QIR/QData.h"
#include "QIR/QFactory.h"
#include "QIR/QBlock.h"
#include "QIR/QValues.h"
#include "QIR/QInsts.h"

using namespace std;
using namespace Citron;

TEST(TestCaseName, TestName) {
  EXPECT_EQ(1, 1);
  EXPECT_TRUE(true);
}

// Scenario_Result
TEST(QEvaluator, DebugPrint_PrintWell)
{
    RFactory rFactory;
    NFactory nFactory{&rFactory};
    QFactory qFactory;

    auto* nRootNamespace = nFactory.MakeRootNamespaceDecl();
    auto* nEntry = nFactory.MakeNDecl<NGlobalFuncDecl>(
        nRootNamespace, RAccessor::Public, 
        /*bStatic*/false, /*bSeqFunc*/false, 
        RName_Normal{"main"}, 
        /*typeParams*/vector<string>{});

    auto* qEntryBlock = qFactory.MakeQBlock("entry");
    std::vector<QValue> args{QValue_ConstInteger{1}};

    QInst_Intrinsic inst{QInst_IntrinsicKind::DebugPrint_Items, nullopt, move(args)};
    qEntryBlock->AddInst(inst);
    qEntryBlock->AddInst(QInst_Return{});

    std::vector<QFuncBody> funcBodies;
    funcBodies.emplace_back(nEntry, qEntryBlock);
    QData qData{move(funcBodies)};

    auto eResult = Evaluate({}, qData, nEntry);
    EXPECT_TRUE(eResult);
}

TEST(QEvaluator, Translate)
{
}