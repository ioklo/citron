#include "pch.h"
#include "ParserMisc.h"
#include "Lexer.h"

using namespace std;

namespace Citron {

optional<OutAndParams> AcceptParseOutAndParams(Lexer* lexer)
{
    Lexer curLexer = *lexer;
    bool bOut = false, bParams = false;
    while (true)
    {
        if (!bOut && Accept<OutToken>(&curLexer)) { bOut = true; continue; }
        if (!bParams && Accept<ParamsToken>(&curLexer)) { bParams = true; continue; }
        break;
    }

    if (bOut && bParams)
    {
        // TODO: [25] out과 params를 같이 쓰면 에러 처리
        return nullopt;
    }

    *lexer = std::move(curLexer);
    return OutAndParams{ bOut, bParams };
}

}