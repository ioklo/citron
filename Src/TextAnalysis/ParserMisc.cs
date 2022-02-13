using Citron.LexicalAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;

namespace Citron
{
    static class ParserMisc
    {
        public static bool Accept<TToken>(LexResult lexResult, ref ParserContext context)
        {
            if (lexResult.HasValue && lexResult.Token is TToken)
            {
                context = context.Update(lexResult.Context);
                return true;
            }

            return false;
        }

        public static bool Accept<TToken>(LexResult lexResult, ref ParserContext context, [NotNullWhen(true)] out TToken? token) where TToken : Token
        {
            if (lexResult.HasValue && lexResult.Token is TToken resultToken)
            {
                context = context.Update(lexResult.Context);
                token = resultToken;
                return true;
            }

            token = null;
            return false;
        }

        public static bool Peek<TToken>(LexResult lexResult) where TToken : Token
        {
            return lexResult.HasValue && lexResult.Token is TToken;
        }

        public static bool Parse<TSyntaxElem>(
            ParseResult<TSyntaxElem> parseResult,
            ref ParserContext context,
            [MaybeNullWhen(false)] out TSyntaxElem elem)
        {
            if (!parseResult.HasValue)
            {
                elem = default;
                return false;
            }
            else
            {
                elem = parseResult.Elem;
                context = parseResult.Context;
                return true;
            }
        }

        public static async ValueTask<ParseResult<TSyntaxElem>> Try<TSyntaxElem>(
            Func<ParserContext, ValueTask<ParseResult<TSyntaxElem>>> parseFunc, ParserContext context)
        {
            try
            {
                return await parseFunc.Invoke(context);
            }
            catch (ParseFatalException)
            {
                return ParseResult<TSyntaxElem>.Invalid;
            }
        }

    }
}
