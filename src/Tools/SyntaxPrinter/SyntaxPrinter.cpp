// SyntaxPrinter.cpp : This file contains the 'main' function. Program execution begins and ends there.
//
#include "pch.h"

#include <Infra/Json.h>
#include <Infra/StringWriter.h>

#include <Syntax/Syntax.h>
#include <TextAnalysis/Lexer.h>
#include <TextAnalysis/Buffer.h>
#include <TextAnalysis/BufferPosition.h>

#include <TextAnalysis/ScriptParser.h>
#include <TextAnalysis/TypeExpParser.h>
#include <TextAnalysis/ExpParser.h>
#include <TextAnalysis/StmtParser.h>

using namespace std;
using namespace Citron;

enum class Mode { Script, Stmt, Exp, TypeExp };

template<typename T>
void Print(T t)
{
    StringWriter stringWriter;
    ToString(ToJson(t), stringWriter);
    cout << stringWriter.ToString();
}

int main(int argc, char* argv[])
{
    auto mode = Mode::Script;

    for (int i = 1; i < argc; i++)
    {
        auto s = string(argv[i]);

        if (s == "--exp")
            mode = Mode::Exp;
        else if (s == "--script")
            mode = Mode::Script;
        else if (s == "--stmt")
            mode = Mode::Stmt;
        else if (s == "--typeexp")
            mode = Mode::TypeExp;
        else if (s == "--help")
        {
            cout << "options: --script, --stmt, --exp, --typeexp (using last option)" << endl;
            return 1;
        }
    }

    ostringstream oss;
    oss << cin.rdbuf();

    auto buffer = make_shared<Buffer>(oss.str());
    Lexer lexer(buffer->MakeStartPosition());

    switch (mode)
    {
    case Mode::Script:
        Print(ParseScript(&lexer));
        break;

    case Mode::TypeExp:
        Print(ParseTypeExp(&lexer));
        break;

    case Mode::Stmt:
        Print(ParseStmt(&lexer));
        break;

    case Mode::Exp:
        Print(ParseExp(&lexer));
        break;
    }
    
}