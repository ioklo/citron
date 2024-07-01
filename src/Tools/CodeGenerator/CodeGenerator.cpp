// CodeGenerator.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>
#include <locale>
#include <filesystem>
#include <string>
#include <vector>
#include "SyntaxCodeGenerator.h"
#include "SymbolCodeGenerator.h"

using namespace std;
using namespace std::filesystem;

// 윈도우에서 잘 돌아가던 프로그램
int wmain(int argc, wchar_t* argv[])
{
    // locale::global(locale(".utf8"));
    locale::global(locale(""));

    if (argc < 2)
    {
        wcout << L"Usage: " << argv[0] << ' ' << L"[Source Directory]" << endl;
        wcout << L"   ex: " << argv[0] << ' ' << L"..\\..   (relative to working directory)" << endl;
        return 1;
    }
    
    auto srcPath = absolute(argv[1]);
    wcout << L"Source Directory: " << srcPath << endl;

    // Syntax Generation
    // variant 만들기
    GenerateSyntax(srcPath);
    // GenerateSymbol(srcPath);

    return 0;
}

#if defined(__clang__)
int main(int argc, char* argv[])
{
    vector<std::wstring> wsargvs;
    wsargvs.reserve(argc);

    setlocale(LC_ALL, "");

    for(int i = 0; i < argc; i++)
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
    for(int i = 0; i < argc; i++)
        wargvs.push_back(wsargvs[i].data());

    return wmain(argc, wargvs.data());
}
#endif


