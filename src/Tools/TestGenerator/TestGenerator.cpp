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
    filesystem::create_directories(filePath.parent_path());

    ofstream ofs(filePath);
    ofs.write(contents.c_str(), contents.size());
    ofs.close();
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
        wcout << L"Usage: " << argv[0] << ' ' << L"[Input Directory] [Output Directory]" << endl;
        wcout << L"   ex: " << argv[0] << ' ' << L". ..\\.." << endl;
        return 1;
    }
    
    auto testsPath = [argv]() {
        path path1 = argv[1];

        if (path1.is_absolute())
            return path1;

        return absolute(path1
            .append("Inputs")
            .append("ScriptParserTests"));

    }();

    auto outputPath = [argv]() {
        path path2 = argv[2];

        if (path2.is_absolute())
            return path2;

        return absolute(path2.append("TextAnalysis.Tests"));
    }();

    // TestAnalysis.Tests
    path resultPath = outputPath;
    resultPath.append("ScriptParserTests.g.cpp");

    wcout << L"Input Directory: " << testsPath << endl;

    if (!exists(testsPath))
    {
        wcout << L"디렉토리가 존재하지 않습니다";
        return 1;
    }

    // 1. ScriptParserTests.g.cpp만들기
    auto templ = R"----(TEST({}, {})
{{
    auto [buffer, lexer] = Prepare(UR"---({})---");

    auto oScript = ParseScript(&lexer);

    auto expected = R"---({})---";

    EXPECT_SYNTAX_EQ(oScript, expected);
}})----";

    const string inExt = ".in.txt";
    const string outExt = ".out.txt";
    size_t extLength = inExt.length();

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
    std::filesystem::directory_iterator di(testsPath);
    vector<directory_entry> entries(begin(di), end(di));
    sort(entries.begin(), entries.end());

    for (auto& dir_entry : entries)
    {
        if (!dir_entry.is_regular_file()) continue;

        auto inFilePath = dir_entry.path();
        auto filename = inFilePath.filename().string();

        // ".in.txt" 7글자
        if (filename.length() <= extLength) continue;

        size_t lengthWithoutExt = filename.length() - extLength;
        if (!boost::iequals(string_view(filename).substr(lengthWithoutExt), inExt)) continue;

        auto name = filename.substr(0, lengthWithoutExt);
        auto outFilePath = dir_entry.path().parent_path().append(name + outExt);

        if (!exists(outFilePath)) continue;

        auto inContents = readAll(inFilePath);
        auto outContents = readAll(outFilePath);

        auto testContents = fmt::format(templ, "ScriptParser", name.c_str(), inContents, outContents);
        oss << testContents << endl << endl;
    }

    auto resultContents = oss.str();

    if (exists(resultPath))
    {
        auto prevResultContents = readAll(resultPath);
        if (resultContents == prevResultContents) return 0;
    }

    writeAll(resultPath, resultContents);
    return 0;

    // wcout << L"Hello World!\n";
}