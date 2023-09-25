using Citron.LexicalAnalysis;
using System;

namespace Citron;

static class ParserMisc
{
    public static bool Accept(Token token, Lexer lexer, ref ParserContext context)
    {
        var lexResult = lexer.LexNormalMode(context.LexerContext, true);
        if (lexResult.HasValue && lexResult.Token == token)
        {
            context = context.Update(lexResult.Context);
            return true;
        }

        return false;
    }

    public static (bool bOut, bool bParams) AcceptParseOutAndParams(Lexer lexer, ref ParserContext context)
    {
        bool bOut = false, bParams = false;

        while (true)
        {
            if (!bOut && Accept(Tokens.Out, lexer, ref context)) { bOut = true; continue; }
            if (!bParams && Accept(Tokens.Params, lexer, ref context)) { bParams = true; continue; }
            break;
        }

        if (bOut && bParams)
        {
            // TODO: [25] out과 params를 같이 쓰면 에러 처리
            throw new NotImplementedException();
        }

        return (bOut, bParams);
    }
}