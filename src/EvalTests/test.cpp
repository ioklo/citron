#include <gtest/gtest.h>
#include <string>
#include <sstream>

#include "Infra/Ptr.h"

#include "Logging/Logger.h"

#include "TextAnalysis/ScriptParser.h"
#include "TextAnalysis/Buffer.h"

#include "SyntaxIR0Translator/SyntaxIR0Translator.h"
#include "IR0IR1Translator/IR0IR1Translator.h"

#include "RSymbol/RFactory.h"
#include "RSymbol/RDecl.h"
#include "RSymbol/RModule.h"

#include "NSymbol/NFactory.h"
#include "NSymbol/NFuncDecl.h"
#include "NSymbol/NModule.h"
#include "NSymbol/NGlobalFuncDecl.h"

#include "MIR/MFactory.h"

#include "QIR/QFactory.h"
#include "QIR/QBlock.h"

#include "QEvaluator/QEvaluation.h"

using namespace std;
using namespace Citron;

class CommandHandler : public IEvalQDataCommandHandler
{
    ostringstream output;

public:
    void Execute(const std::string& command) override
    {
        output << command;
    }

    string GetOutput()
    {
        return output.str();
    }
};

TEST(TestCaseName, TestName) 
{
    // 파일을 하나 읽어서 
    auto code = R"--(
void Main()
{
    if (1 < 2) @good

    if (1 > 2)
    { 
        @bad
    }
}
)--";

    string result = "good";

    // 1. TextAnalysis
    auto buffer = MakePtr<Buffer>(code);
    BufferPosition pos = buffer->MakeStartPosition();    
    Lexer lexer{pos};
    SFactory sFactory;

    auto* sScript = ParseScript(&lexer, sFactory);
    EXPECT_TRUE(sScript);

    string moduleName = "MyModule";
    RFactoryPtr rFactory = MakePtr<RFactory>();
    NFactoryPtr nFactory = MakePtr<NFactory>(rFactory);

    auto logger = MakePtr<Logger>();
    auto mFactory = MakePtr<MFactory>();

    auto eNModuleMData = TranslateSyntaxToNModuleMData(moduleName, {sScript}, {}, logger, rFactory, nFactory, mFactory);
    EXPECT_TRUE(eNModuleMData);
    auto& [nModule, mData] = *eNModuleMData;

    QFactoryPtr qFactory = MakePtr<QFactory>();
    auto eQData = TranslateMDataToQData(mData, qFactory);
    EXPECT_TRUE(eQData);
    auto* qData = *eQData;

    // "Main" 찾기
    NGlobalFuncDecl* nEntry = nullptr;
    for(auto& body : qData->GetAllBodies())
    {
        if (NGlobalFuncDecl* globalFuncDecl = dynamic_cast<NGlobalFuncDecl*>(body.nFuncDecl))
        {
            auto id = body.nFuncDecl->GetNDecl()->GetRDecl()->GetIdentifier();
            if (id == RIdentifier{RName_Normal("Main"), 0, {}})
                nEntry = globalFuncDecl;
        }
    }
    EXPECT_TRUE(nEntry);

    auto commandHandler = MakePtr<CommandHandler>();
    vector<RModule*> rModules{nModule};
    auto eResult = EvaluateQData(rModules, qData, nEntry, commandHandler);
    EXPECT_TRUE(eResult);

    // 
    EXPECT_EQ(commandHandler->GetOutput(), "good");
}