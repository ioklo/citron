#include <gtest/gtest.h>
#include <string>
#include <sstream>

#include "Infra/Ptr.h"
#include "Infra/StringWriter.h"

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
#include "MIR/MPrinter.h"

#include "QIR/QFactory.h"
#include "QIR/QBlock.h"
#include "QIR/QPrinter.h"

#include "QEvaluator/QEvaluation.h"
#include "QIrLLVMTranslator/QIrLLVMTranslator.h"

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

void DoTest(const string& code, const string& expected)
{
    // 1. TextAnalysis
    auto buffer = MakePtr<Buffer>(code);
    BufferPosition pos = buffer->MakeStartPosition();
    Lexer lexer{pos};
    SFactory sFactory;

    auto* sScript = ParseScript(&lexer, sFactory);
    ASSERT_TRUE(sScript);

    string moduleName = "MyModule";
    auto rFactory = MakePtr<RFactory>();
    auto nFactory = MakePtr<NFactory>(rFactory);
    auto logger = MakePtr<Logger>();
    auto mFactory = MakePtr<MFactory>();

    auto e_nModuleMData = TranslateSyntaxToNModuleMData(moduleName, {sScript}, {}, logger, rFactory, nFactory, mFactory);
    ASSERT_TRUE(e_nModuleMData);
    auto& [nModule, mData] = *e_nModuleMData;

    StringWriter mWriter;
    PrintMData(mData, mWriter, *rFactory);
    auto mOut = mWriter.ToString();

    QFactoryPtr qFactory = MakePtr<QFactory>();
    auto e_qData = TranslateMDataToQData(mData, rFactory, qFactory);
    ASSERT_TRUE(e_qData);
    auto* qData = *e_qData;

    StringWriter qWriter;
    PrintQData(qData, qWriter, *rFactory);
    auto qOut = qWriter.ToString();

    // LLVM
    /*Citron::LContext lContext{rFactory, qFactory};
    auto lData = TranslateQDataToLData(qData, lContext);*/

    // 실행

    // "Main" 찾기
    NGlobalFuncDecl* nEntry = nullptr;
    for (auto& body : qData->GetAllBodies())
    {
        if (NGlobalFuncDecl* globalFuncDecl = dynamic_cast<NGlobalFuncDecl*>(body.nFuncDecl))
        {
            auto id = body.nFuncDecl->GetNDecl()->GetRDecl()->GetIdentifier();
            if (id == RIdentifier{RName_Normal("Main"), 0, {}})
                nEntry = globalFuncDecl;
        }
    }
    ASSERT_TRUE(nEntry);

    auto commandHandler = MakePtr<CommandHandler>();
    vector<RModule*> rModules{nModule};
    auto e_result = EvaluateQData(rModules, qData, nEntry, commandHandler, rFactory);
    ASSERT_TRUE(e_result);

    // 
    ASSERT_EQ(commandHandler->GetOutput(), expected);


    // 
}
