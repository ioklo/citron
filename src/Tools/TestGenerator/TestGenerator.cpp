// TestGenerator.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>
#include <fstream>
#include <sstream>
#include <filesystem>
#include <clocale>
#include <variant>
#include <format>
#include <regex>
#include <boost/algorithm/string.hpp>

#include "Infra/Variants.h"

using namespace std;
using namespace std::filesystem;

// u8string
std::string readAll(path filePath)
{
    ifstream ifs(filePath);
    ostringstream oss;
    oss << ifs.rdbuf();

    return oss.str();
}

void writeAll(path filePath, string contents)
{
    if (exists(filePath))
    {
        auto prevContents = readAll(filePath);
        if (contents == prevContents) return;
    }

    filesystem::create_directories(filePath.parent_path());

    ofstream ofs(filePath);
    ofs.write(contents.c_str(), contents.size());
    ofs.close();
}

// name, inFilePath, isFail, outFilePath | failFilePath
vector<tuple<string, path, bool, path>> GetFiles(path p)
{
    const string inExt = ".in.txt";
    const string outExt = ".out.txt";
    const string failExt = ".fail.txt";
    size_t extLength = inExt.length();

    vector<tuple<string, path, bool, path>> results;
    for (auto& dir_entry : std::filesystem::directory_iterator(p))
    {
        if (!dir_entry.is_regular_file()) continue;

        auto inFilePath = dir_entry.path();
        auto filename = inFilePath.filename().string();
        
        if (filename.length() <= extLength) continue;

        size_t lengthWithoutExt = filename.length() - extLength;
        if (!boost::iequals(string_view(filename).substr(lengthWithoutExt), inExt)) continue;

        auto name = filename.substr(0, lengthWithoutExt);
        auto outFilePath = dir_entry.path().parent_path().append(name + outExt);

        if (exists(outFilePath))
        {
            results.emplace_back(name, inFilePath, false, outFilePath);
            continue;
        }

        auto failFilePath = dir_entry.path().parent_path().append(name + failExt);

        if (exists(failFilePath))
        {
            results.emplace_back(name, inFilePath, true, outFilePath);
            continue;
        }

        wcout << inFilePath << L": neither .out.txt nor .fail.txt exists";
    }

    sort(results.begin(), results.end(), [](auto& x, auto& y) { return get<0>(x) < get<0>(y); });
    return results;
}

struct CTInfoResult_Error {};
struct CTInfoResult_Text { string text; };
using CTInfoResult = variant<CTInfoResult_Error, CTInfoResult_Text>;
struct CTInfo { string category; string name; string code; CTInfoResult result; };
vector<CTInfo> ReadCTFiles(path p)
{
    vector<CTInfo> results;

    string ctExt = ".ct";
    size_t extLength = ctExt.length();

    for (auto& dir_entry : std::filesystem::directory_iterator(p))
    {
        if (!dir_entry.is_regular_file()) continue;

        auto ctFilePath = dir_entry.path();
        auto filename = ctFilePath.filename().string();

        if (filename.length() <= extLength) continue;

        size_t lengthWithoutExt = filename.length() - extLength;
        if (!boost::iequals(string_view(filename).substr(lengthWithoutExt), ctExt)) continue;

        auto text = readAll(ctFilePath);

        // 파일 이름 카테고리
        regex r{R"-(([\w_]+)_(\w+))-"};
        auto stem = ctFilePath.stem().string();
        smatch match;
        if (!regex_match(stem, match, r))
        {
            wcout << L"file name doesn't match TestCategory_Name.ct: " << ctFilePath << endl;
            continue;
        }

        string category = match.str(1);
        string name = match.str(2);

        // 첫째줄 (\n 나올때)
        //@ 10
        auto s = text.find_first_of("\r\n");
        if (s == string::npos) s = text.find_first_of("\n");
        if (s == string::npos || !text.starts_with("//@ ")) // 에러
        {
            wcout << "doesn't have header: " << ctFilePath << endl;
            continue;
        }

        // 
        string_view header{text.data() + 4, s - 4};
        string_view code{text.data() + s + 1};

        if (header.starts_with("$Error"))
            results.emplace_back(category, name, string{code}, CTInfoResult_Error{});
        else
            results.emplace_back(category, name, string{code}, CTInfoResult_Text{string{header}});
    }
    return results;
}



void GenerateScriptParserTests(path inputPath, path srcPath)
{
    // TestData/ScriptParserTests
    path testsPath = inputPath;
    testsPath.append("ScriptParserTests");

    // src/TestAnalysis.Tests/ScriptParserTests.g.cpp
    path resultPath = srcPath;

    resultPath
        .append("TextAnalysis.Tests")
        .append("ScriptParserTests.g.cpp");

    if (!exists(testsPath))
        return;
    
    ostringstream oss;

    // insert header
    oss << R"---(#include <gtest/gtest.h>

#include "Syntax/Syntax.h"
#include "TextAnalysis/ScriptParser.h"

#include "TestMisc.h"

using namespace std;
using namespace Citron;

)---";

    constexpr auto templ = R"----(TEST({}, {})
{{
    auto [buffer, lexer] = Prepare(UR"---({})---");
    SFactory factory;

    auto* script = ParseScript(&lexer, factory);

    auto expected = R"---({})---";

    EXPECT_SYNTAX_EQ(script, expected);
}})----";

    for (auto& [name, inFilePath, bFail, outFilePath] : GetFiles(testsPath))
    {
        if (bFail)
            wcout << inFilePath << L": fail case not supported" << endl;

        auto inContents = readAll(inFilePath);
        auto outContents = readAll(outFilePath);

        auto testContents = format(templ, "ScriptParser", name.c_str(), inContents, outContents);
        oss << testContents << endl << endl;
    }

    writeAll(resultPath, oss.str());
}

void GenerateStmtParserTests(path inputPath, path srcPath)
{
    // input/StmtParserTests
    path testsPath = inputPath;
    testsPath.append("StmtParserTests");

    // src/TestAnalysis.Tests/StmtParserTests.g.cpp
    path resultPath = srcPath;

    resultPath
        .append("TextAnalysis.Tests")
        .append("StmtParserTests.g.cpp");

    if (!exists(testsPath))
        return;

    ostringstream oss;

    // insert header
    oss << R"---(#include <gtest/gtest.h>

#include "Syntax/Syntax.h"
#include "TextAnalysis/StmtParser.h"

#include "TestMisc.h"

using namespace std;
using namespace Citron;

)---";

    constexpr auto templ = R"----(TEST({}, {})
{{
    auto [buffer, lexer] = Prepare(UR"---({})---");
    SFactory factory;

    auto* stmt = ParseStmt(&lexer, factory);

    auto expected = R"---({})---";

    EXPECT_SYNTAX_EQ(stmt, expected);
}})----";

    for (auto& [name, inFilePath, bFail, outFilePath] : GetFiles(testsPath))
    {
        if (bFail)
            wcout << inFilePath << L": fail case not supported" << endl;

        auto inContents = readAll(inFilePath);
        auto outContents = readAll(outFilePath);

        auto testContents = format(templ, "StmtParser", name.c_str(), inContents, outContents);
        oss << testContents << endl << endl;
    }

    writeAll(resultPath, oss.str());
}

void GenerateExpParserTests(path inputPath, path srcPath)
{
    // input/ExpParserTests
    path testsPath = inputPath;
    testsPath.append("ExpParserTests");

    // src/TestAnalysis.Tests/ExpParserTests.g.cpp
    path resultPath = srcPath;

    resultPath
        .append("TextAnalysis.Tests")
        .append("ExpParserTests.g.cpp");

    if (!exists(testsPath))
        return;

    ostringstream oss;

    // insert header
    oss << R"---(#include <gtest/gtest.h>

#include "Syntax/Syntax.h"
#include "TextAnalysis/ExpParser.h"

#include "TestMisc.h"

using namespace std;
using namespace Citron;

)---";

    constexpr auto templ = R"----(TEST({}, {})
{{
    auto [buffer, lexer] = Prepare(UR"---({})---");
    SFactory factory;

    auto* exp = ParseExp(&lexer, factory);

    auto expected = R"---({})---";

    EXPECT_SYNTAX_EQ(exp, expected);
}})----";

    for (auto& [name, inFilePath, bFail, outFilePath] : GetFiles(testsPath))
    {
        if (bFail)
            wcout << inFilePath << L": fail case not supported" << endl;

        auto inContents = readAll(inFilePath);
        auto outContents = readAll(outFilePath);

        auto testContents = format(templ, "ExpParser", name.c_str(), inContents, outContents);
        oss << testContents << endl << endl;
    }

    writeAll(resultPath, oss.str());
}

void GenerateTypeExpParserTests(path inputPath, path srcPath)
{
    // input/TypeExpParserTests
    path testsPath = inputPath;
    testsPath.append("TypeExpParserTests");

    // src/TestAnalysis.Tests/TypeExpParserTests.g.cpp
    path resultPath = srcPath;

    resultPath
        .append("TextAnalysis.Tests")
        .append("TypeExpParserTests.g.cpp");

    if (!exists(testsPath))
        return;

    ostringstream oss;

    // insert header
    oss << R"---(#include <gtest/gtest.h>

#include "Syntax/Syntax.h"
#include "TextAnalysis/TypeExpParser.h"

#include "TestMisc.h"

using namespace std;
using namespace Citron;

)---";

    constexpr auto succTempl = R"----(TEST({}, {})
{{
    auto [buffer, lexer] = Prepare(UR"---({})---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}})----";

    constexpr auto failTempl = R"----(TEST({}, {})
{{
    auto [buffer, lexer] = Prepare(UR"---({})---");
    SFactory factory;

    ParseTypeExp(&lexer, factory);

    EXPECT_TRUE(!lexer.IsReachedEnd());
}})----";

    for (auto& [name, inFilePath, bFail, outFilePath] : GetFiles(testsPath))
    {
        auto inContents = readAll(inFilePath);

        if (!bFail)
        {
            auto outContents = readAll(outFilePath);

            auto testContents = format(succTempl, "TypeExpParser", name.c_str(), inContents, outContents);
            oss << testContents << endl << endl;
        }
        else
        {
            auto testContents = format(failTempl, "TypeExpParser", name.c_str(), inContents);
            oss << testContents << endl << endl;
        }
    }

    writeAll(resultPath, oss.str());
}

void GenerateEvalTests(path basePath)
{
    // EvalTests
    path testsPath = basePath / "data/TestData/EvalTests";

    // src/TestAnalysis.Tests/TypeExpParserTests.g.cpp
    path resultPath = basePath / "src/EvalTests/EvalTests.g.cpp";

    if (!exists(testsPath))
        return;

    ostringstream oss;

    // insert header
    oss << R"---(#include <gtest/gtest.h>
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

    auto eNModuleMData = TranslateSyntaxToNModuleMData(moduleName, {sScript}, {}, logger, rFactory, nFactory, mFactory);
    ASSERT_TRUE(eNModuleMData);
    auto& [nModule, mData] = *eNModuleMData;

    QFactoryPtr qFactory = MakePtr<QFactory>();
    auto eQData = TranslateMDataToQData(mData, rFactory, qFactory);
    ASSERT_TRUE(eQData);
    auto* qData = *eQData;

    StringWriter writer;
    PrintQData(qData, writer, *qFactory);
    auto out = writer.ToString();

    // LLVM
    Citron::LContext lContext{rFactory, qFactory};
    auto lData = TranslateQDataToLData(qData, lContext);

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
    auto eResult = EvaluateQData(rModules, qData, nEntry, commandHandler, qFactory);
    ASSERT_TRUE(eResult);

    // 
    ASSERT_EQ(commandHandler->GetOutput(), expected);


    // 
}
)---";

    constexpr auto succTempl = R"----(TEST({}, {}) 
{{
    auto code = R"---({})---";
    string expected = R"---({})---";

    DoTest(code, expected);
}})----";
    
    for (auto& info : ReadCTFiles(testsPath))
    {
        visit(overloaded{
            [&oss, &succTempl, &info](CTInfoResult_Text& textResult) -> void {
                oss << format(succTempl, info.category, info.name, info.code, textResult.text) << endl << endl;
            },
            [](CTInfoResult_Error& errorResult) -> void {
                // NotImplemented
            }
        }, info.result);
    }

    writeAll(resultPath, oss.str());
}

// argv는 프로그램 포함
// 소스가 utf-8로 고정되어서(모든 char*리터럴은 utf-8이다)
// 일반 main을 쓰면, cout쓸때 utf-8로 나가게 된다. 그럼 현재 locale로 conversion하기 귀찮아 져서 wmain을 쓰도록 한다
int wmain(int argc, wchar_t* argv[])
{   
    locale::global(locale("")); // locale을 C (0~127 ASCII)에서 현재 국가, ansi code page로 변경

    // wcout << L"abc 안녕하세요 abc";
    if (argc < 2)
    {
        wcout << L"Usage: " << argv[0] << ' ' << L"[Base Directory]" << endl;
        wcout << L"   ex: " << argv[0] << ' ' << L"..\\..\\..   (relative to working directory)" << endl;
        return 1;
    }

    auto basePath = canonical(argv[1]);
    wcout << L"Base Directory: " << basePath << endl;

    auto inputsPath = basePath / "data" / "TestData";
    auto srcPath = basePath / "src";
        
    GenerateScriptParserTests(inputsPath, srcPath);
    GenerateStmtParserTests(inputsPath, srcPath);
    GenerateExpParserTests(inputsPath, srcPath);
    GenerateTypeExpParserTests(inputsPath, srcPath);
    GenerateEvalTests(basePath);

    return 0;
}


#ifndef _WIN32
int main(int argc, char* argv[])
{
    vector<std::wstring> wsargvs;
    wsargvs.reserve(argc);

    setlocale(LC_ALL, "");

    for (int i = 0; i < argc; i++)
    {
        size_t requiredSize = mbstowcs(nullptr, argv[i], 0);
        if (requiredSize == (size_t)-1)
            return 1;

        wsargvs.emplace_back(requiredSize, L' ');
        size_t ret = mbstowcs(wsargvs.back().data(), argv[i], requiredSize + 1);
        if (ret == (size_t)-1)
            return 1;
    }

    vector<wchar_t*> wargvs;
    wargvs.reserve(argc);
    for (int i = 0; i < argc; i++)
        wargvs.push_back(wsargvs[i].data());

    return wmain(argc, wargvs.data());
}
#endif