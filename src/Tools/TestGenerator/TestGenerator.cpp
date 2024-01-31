// TestGenerator.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>
#include <fstream>
#include <sstream>
#include <filesystem>
#include <clocale>
#include <utf8.h>
#include <Windows.h>
#include <boost/algorithm/string.hpp>
#include <fmt/core.h>

using namespace std;
using namespace std::filesystem;

template<typename Facet>
struct deletable_facet : Facet
{
    template<class... Args>
    deletable_facet(Args&&... args) : Facet(std::forward<Args>(args)...) {}
    ~deletable_facet() {}
};

// u8string
std::string readAll(path filePath)
{
    ifstream ifs(filePath);
    ostringstream oss;
    oss << ifs.rdbuf();

    return oss.str();


    //ifs.seekg(0, ifs.end);
    //ifstream::pos_type length = ifs.tellg();
    //ifs.seekg(0, ifs.beg);

    //std::string s;
    //s.resize(length);

    //ifs.read(s.data(), length);

    // return s;
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

// name, inFilePath, outFilePath
vector<tuple<string, path, path>> GetFiles(path p)
{
    const string inExt = ".in.txt";
    const string outExt = ".out.txt";
    size_t extLength = inExt.length();

    vector<tuple<string, path, path>> results;
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

        if (!exists(outFilePath)) continue;
        results.emplace_back(name, inFilePath, outFilePath);
    }

    sort(results.begin(), results.end(), [](auto& x, auto& y) { return get<0>(x) < get<0>(y); });
    return results;
}

void GenerateScriptParserTests(path inputPath, path srcPath)
{
    // input/ScriptParserTests
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
    oss << R"---(#include "pch.h"

#include <Syntax/Syntax.h>
#include <TextAnalysis/ScriptParser.h>

#include <Syntax/ExpSyntaxes.h>

#include "TestMisc.h"

using namespace std;
using namespace Citron;

)---";

    auto templ = R"----(TEST({}, {})
{{
    auto [buffer, lexer] = Prepare(UR"---({})---");

    auto oScript = ParseScript(&lexer);

    auto expected = R"---({})---";

    EXPECT_SYNTAX_EQ(oScript, expected);
}})----";

    for (auto& [name, inFilePath, outFilePath] : GetFiles(testsPath))
    {	
        auto inContents = readAll(inFilePath);
        auto outContents = readAll(outFilePath);

        auto testContents = fmt::format(templ, "ScriptParser", name.c_str(), inContents, outContents);
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
    oss << R"---(#include "pch.h"

#include <Syntax/Syntax.h>
#include <TextAnalysis/StmtParser.h>

#include <Syntax/ExpSyntaxes.h>

#include "TestMisc.h"

using namespace std;
using namespace Citron;

)---";

    auto templ = R"----(TEST({}, {})
{{
    auto [buffer, lexer] = Prepare(UR"---({})---");

    auto oStmt = ParseStmt(&lexer);

    auto expected = R"---({})---";

    EXPECT_SYNTAX_EQ(oStmt, expected);
}})----";

    for (auto& [name, inFilePath, outFilePath] : GetFiles(testsPath))
    {
        auto inContents = readAll(inFilePath);
        auto outContents = readAll(outFilePath);

        auto testContents = fmt::format(templ, "StmtParser", name.c_str(), inContents, outContents);
        oss << testContents << endl << endl;
    }

    writeAll(resultPath, oss.str());
}

// argv는 프로그램 포함
// 소스가 utf8로 고정되어서(모든 char*리터럴은 utf-8이다)
// 일반 main을 쓰면, cout쓸때 utf8로 나가게 된다. 그럼 현재 locale로 conversion하기 귀찮아 져서 wmain을 쓰도록 한다
int wmain(int argc, wchar_t* argv[])
{   
    locale::global(locale(""));

    // wcout << L"abc 안녕하세요 abc";
    if (argc < 3)
    {
        wcout << L"Usage: " << argv[0] << ' ' << L"[Input Directory] [Source Directory]" << endl;
        wcout << L"   ex: " << argv[0] << ' ' << L"Inputs ..\\..   (relative to working directory)" << endl;
        return 1;
    }
    
    auto inputsPath = absolute(argv[1]);
    auto srcPath = absolute(argv[2]);

    wcout << L"Input Directory: " << inputsPath << endl;
    wcout << L"Source Directory: " << srcPath << endl;
        
    GenerateScriptParserTests(inputsPath, srcPath);
    GenerateStmtParserTests(inputsPath, srcPath);

    return 0;
}