#include "TestMisc.h"

#include "Infra/Ptr.h"

#include "TextAnalysis/Buffer.h"
#include "TextAnalysis/Lexer.h"

using namespace std;

namespace Citron {

tuple<shared_ptr<Buffer>, Lexer> Prepare(u32string str)
{
    auto buffer = MakePtr<Buffer>(str);
    BufferPosition pos = buffer->MakeStartPosition();
    return { move(buffer), Lexer(pos) };
}

}