#include "pch.h"

#include "TestMisc.h"
#include <TextAnalysis/Buffer.h>
#include <TextAnalysis/BufferPosition.h>
#include <TextAnalysis/Lexer.h>

using namespace std;
using namespace Citron;

tuple<shared_ptr<Buffer>, Lexer> Prepare(u32string str)
{
    auto buffer = make_shared<Buffer>(str);
    BufferPosition pos = buffer->MakeStartPosition();
    return { std::move(buffer), Lexer(pos) };
}
