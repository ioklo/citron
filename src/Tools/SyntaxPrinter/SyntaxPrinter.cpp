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

using namespace std;
using namespace Citron;

int main(int argc, char* argv[])
{
    ostringstream oss;
    oss << cin.rdbuf();

    auto buffer = make_shared<Buffer>(oss.str());
    Lexer lexer(buffer->MakeStartPosition());

    auto oScript = ParseScript(&lexer);

    StringWriter stringWriter;
    auto json = ToJson(oScript);
    ToString(json, stringWriter);
    
    cout << stringWriter.ToString();
}