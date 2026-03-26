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
TEST(QEvaluator, CommandInst_DoingWell)
{
    auto rFactory = MakePtr<RFactory>();
    NFactory nFactory{rFactory};
    QFactoryPtr qFactory = MakePtr<QFactory>();

    auto* nRootNamespace = nFactory.MakeRootNamespaceDecl();
    auto* nEntry = nFactory.MakeNDecl<NGlobalFuncDecl>(
        nRootNamespace, RAccessor::Public,
        /*bSeqFunc*/false,
        RName_Normal{"main"});

    nEntry->InitTypeParams({});

    vector<QBlock*> blocks;
    auto* qEntryBlock = qFactory->MakeQBlock(blocks.size(), "entry");
    blocks.push_back(qEntryBlock);

    vector<QSlotInfo> slotInfos;
    slotInfos.push_back(QSlotInfo{rFactory->MakeStringType(), "s0", /*oArgIndex*/nullopt});

    // 1을 문자열로 변환
    QInst_Intrinsic toStringInst{QInst_IntrinsicKind::ToString_Int_String, QArg_Slot{0}, {QArg_ConstInt32{1}}};
    qEntryBlock->EmitInst(move(toStringInst));

    std::vector<QArg_Input> args{QArg_Slot{0}};
    QInst_Intrinsic inst{QInst_IntrinsicKind::Command_Items, nullopt, move(args)};
    qEntryBlock->EmitInst(move(inst));

    qEntryBlock->EmitInst(QInst_Return{nullopt});

    std::vector<QFuncBody> funcBodies;
    funcBodies.emplace_back(nEntry, move(slotInfos), move(blocks));
    QData* qData = qFactory->MakeQData(move(funcBodies));
    
    auto e_result = EvaluateQData({}, qData, nEntry, MakePtr<NullCommandHandler>(), rFactory);
    EXPECT_TRUE(e_result);
}

TEST(QEvaluator, Translate)
{
}