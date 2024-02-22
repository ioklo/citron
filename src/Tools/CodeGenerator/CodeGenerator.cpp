// CodeGenerator.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>
#include <locale>
#include <filesystem>
#include "SyntaxCodeGenerator.h"

using namespace std;
using namespace std::filesystem;

int wmain(int argc, wchar_t* argv[])
{
    locale::global(locale(".utf8"));

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
}
