#include <gtest/gtest.h>
#include <string>
#include <sstream>

#include "Infra/Ptr.h"
#include "Infra/StringWriter.h"

#include "Logging/Logger.h"

#include "TextAnalysis/ScriptParser.h"
#include "TextAnalysis/Buffer.h"

#include "SmTranslator/SmTranslator.h"
#include "MqTranslator/MqTranslator.h"

#include "RSymbol/RFactory.h"
#include "RSymbol/RDecl.h"
#include "RSymbol/RModule.h"

#include "NSymbol/NFactory.h"
#include "RSymbol/RGlobalFuncDecl.h"

#include "MIR/MFactory.h"
#include "MIR/MPrinter.h"

#include "QIR/QFactory.h"
#include "QIR/QBlock.h"
#include "QIR/QPrinter.h"

#include "QEvaluator/QEvaluation.h"
// #include "QlTranslator/QlTranslator.h"

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
    auto rFactory = RFactory::Make();
    auto nFactory = MakePtr<NFactory>(rFactory);
    auto logger = MakePtr<Logger>();
    auto mFactory = MakePtr<MFactory>();

    auto e_rModuleMData = TranslateSyntax(moduleName, {sScript}, {}, logger, rFactory, mFactory);
    ASSERT_TRUE(e_rModuleMData);
    auto& [rModule, mData] = *e_rModuleMData;

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
    RGlobalFuncDecl* rEntry = nullptr;
    for (auto& body : qData->GetAllBodies())
    {
        if (auto* rGlobalFuncDecl = dynamic_cast<RGlobalFuncDecl*>(body.rFuncDecl))
        {   
            auto* name = body.rFuncDecl->RFuncDecl_GetDecl()->TryGetName();
            if (name && *name == RName::Normal("Main"))
                rEntry = rGlobalFuncDecl;
        }
    }
    ASSERT_TRUE(rEntry);

    auto commandHandler = MakePtr<CommandHandler>();
    vector<RModule*> rModules{rModule};
    auto e_result = EvaluateQData(rModules, qData, rEntry, commandHandler, rFactory);
    ASSERT_TRUE(e_result);

    // 
    ASSERT_EQ(commandHandler->GetOutput(), expected); 
}
