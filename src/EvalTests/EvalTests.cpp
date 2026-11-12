#include <gtest/gtest.h>
#include <gtest/gtest-spi.h>
#include <string>
#include <sstream>

#include "Infra/Json.h"
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
    SCOPED_TRACE("=== Source ===\n" + code);

    // 1. TextAnalysis
    auto buffer = MakePtr<Buffer>(code);
    BufferPosition pos = buffer->MakeStartPosition();
    Lexer lexer{pos};
    SFactory sFactory;

    auto* sScript = ParseScript(&lexer, sFactory);
    ASSERT_TRUE(sScript);

    StringWriter syntaxWriter;
    ToString(ToJson(sScript), syntaxWriter);
    SCOPED_TRACE("=== Syntax ===\n" + syntaxWriter.ToString());

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
    SCOPED_TRACE("=== MIR ===\n" + mOut);

    QFactoryPtr qFactory = MakePtr<QFactory>();
    auto e_qData = TranslateMDataToQData(mData, rFactory, qFactory);
    ASSERT_TRUE(e_qData);
    auto* qData = *e_qData;

    StringWriter qWriter;
    PrintQData(qData, qWriter, *rFactory);
    auto qOut = qWriter.ToString();
    SCOPED_TRACE("=== QIR ===\n" + qOut);

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

TEST(EvalTestDiagnostics, ParseFailureIncludesSource)
{
    EXPECT_FATAL_FAILURE(DoTest("}", ""), "=== Source ===");
}

TEST(EvalTestDiagnostics, OutputMismatchIncludesAllStages)
{
    testing::TestPartResultArray results;
    {
        testing::ScopedFakeTestPartResultReporter reporter(
            testing::ScopedFakeTestPartResultReporter::INTERCEPT_ONLY_CURRENT_THREAD, &results);
        DoTest("void Main() { int a = 0; a = 10; @$a\n}", "wrong output");
    }

    ASSERT_EQ(results.size(), 1);
    auto& result = results.GetTestPartResult(0);
    EXPECT_TRUE(result.fatally_failed());
    string message = result.message();
    EXPECT_NE(message.find("=== Source ==="), string::npos);
    EXPECT_NE(message.find("=== Syntax ==="), string::npos);
    EXPECT_NE(message.find("=== MIR ==="), string::npos);
    EXPECT_NE(message.find("=== QIR ==="), string::npos);
    EXPECT_NE(message.find("wrong output"), string::npos);
}
