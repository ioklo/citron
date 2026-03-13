#include "ParserMisc.h"
#include "Syntax/Syntaxes.g.h"

#include "Lexer.h"

using namespace std;

namespace Citron {

optional<SParamModifier> ParseParamModifier(Lexer* lexer)
{
    Lexer curLexer = *lexer;
    if (!Accept<LBracketToken>(&curLexer)) return nullopt;

    SParamModifier modifier;

    if (Accept<InToken>(&curLexer))
    {
        modifier = SParamModifier::In;
    }
    else if (Accept<MoveToken>(&curLexer))
    {
        modifier = SParamModifier::Move;
    }
    else if (Accept<OutToken>(&curLexer))
    {
        modifier = SParamModifier::Out;
    }
    else if (Accept<ParamsToken>(&curLexer))
    {
        modifier = SParamModifier::Params;
    }
    else if (auto o_idToken = Accept<IdentifierToken>(&curLexer))
    {
        if (o_idToken->text == "forward")
        {
            modifier = SParamModifier::Forward;
        }
        else return nullopt;
    }
    else return nullopt;

    if (!Accept<RBracketToken>(&curLexer)) return nullopt;

    *lexer = move(curLexer);
    return modifier;
}

}