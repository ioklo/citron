module Citron.TestMisc;

import Citron.Ptr;
import Citron.Buffer;
import Citron.Lexer;
import Citron.TestMisc;

using namespace std;

namespace Citron {

tuple<shared_ptr<Buffer>, Lexer> Prepare(u32string str)
{
    auto buffer = MakePtr<Buffer>(str);
    BufferPosition pos = buffer->MakeStartPosition();
    return { std::move(buffer), Lexer(pos) };
}

}