using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Citron.LexicalAnalysis;
using Citron.Syntax;

namespace Citron
{
    public struct LexResult
    {
        public static LexResult Invalid { get; }
        static LexResult()
        {
            Invalid = new LexResult();
        }

        public bool HasValue { get; }
        public Token Token { get; }
        public LexerContext Context { get; }
        public LexResult(Token token, LexerContext context) { HasValue = true; Token = token; Context = context; }
    }

    public class Lexer
    {
        public Lexer()
        {
        }

        bool IsIdentifierStartLetter(BufferPosition curPos)
        {
            if (curPos.Equals('_')) return true; // only allowed among ConnectorPunctuation category

            var category = curPos.GetUnicodeCategory();

            return category == UnicodeCategory.UppercaseLetter ||
                category == UnicodeCategory.LowercaseLetter ||
                category == UnicodeCategory.TitlecaseLetter ||
                category == UnicodeCategory.ModifierLetter ||
                category == UnicodeCategory.OtherLetter ||
                category == UnicodeCategory.NonSpacingMark ||
                category == UnicodeCategory.LetterNumber ||
                category == UnicodeCategory.DecimalDigitNumber;
        }

        bool IsIdentifierLetter(BufferPosition curPos)
        {
            if (curPos.Equals('_')) return true; // only allowed among ConnectorPunctuation category

            var category = curPos.GetUnicodeCategory();

            return category == UnicodeCategory.UppercaseLetter ||
                category == UnicodeCategory.LowercaseLetter ||
                category == UnicodeCategory.TitlecaseLetter ||
                category == UnicodeCategory.ModifierLetter ||
                category == UnicodeCategory.OtherLetter ||
                category == UnicodeCategory.NonSpacingMark ||
                category == UnicodeCategory.LetterNumber ||
                category == UnicodeCategory.DecimalDigitNumber;
        }

        public async ValueTask<LexResult> LexStringModeAsync(LexerContext context)
        {   
            var textResult = await LexStringModeTextAsync(context);
            if (textResult.HasValue)
                return textResult;

            if (context.Pos.Equals('"'))
                return new LexResult(
                    DoubleQuoteToken.Instance,
                    context.UpdatePos(await context.Pos.NextAsync()));

            if (context.Pos.Equals('$'))
            {
                var nextPos = await context.Pos.NextAsync();                

                if (nextPos.Equals('{'))
                    return new LexResult(
                        DollarLBraceToken.Instance,
                        context.UpdatePos(await nextPos.NextAsync()));

                var idResult = await LexIdentifierAsync(context.UpdatePos(nextPos), false);
                if (idResult.HasValue)
                    return idResult;
            }            

            return LexResult.Invalid;
        }

        // 키워드 처리
        private static Dictionary<string, Token> keywordInfos = new Dictionary<string, Token>()
        {
            { "foreach", ForeachToken.Instance },
            { "if", IfToken.Instance },
            { "else", ElseToken.Instance },
            { "for", ForToken.Instance },
            { "continue", ContinueToken.Instance },
            { "break", BreakToken.Instance },
            { "exec", ExecToken.Instance },
            { "task", TaskToken.Instance },
            { "params", ParamsToken.Instance },
            { "return", ReturnToken.Instance },
            { "async", AsyncToken.Instance },
            { "await", AwaitToken.Instance },
            { "in", InToken.Instance },
            { "yield", YieldToken.Instance },
            { "seq", SeqToken.Instance },
            { "enum", EnumToken.Instance },
            { "struct", StructToken.Instance },
            { "class", ClassToken.Instance },
            { "is", IsToken.Instance },
            { "ref", RefToken.Instance },
            { "box", BoxToken.Instance },
            { "null", NullToken.Instance },
            { "public", PublicToken.Instance },
            { "protected", ProtectedToken.Instance },
            { "private", PrivateToken.Instance },
            { "static", StaticToken.Instance },
            { "new", NewToken.Instance },
            { "namespace", NamespaceToken.Instance },
        };

        private static (string Text, Func<Token> Constructor)[] infos = new (string Text, Func<Token> Constructor)[]
        {   
            ("++", () => PlusPlusToken.Instance),
            ("--", () => MinusMinusToken.Instance),
            ("<=", () => LessThanEqualToken.Instance),
            (">=", () => GreaterThanEqualToken.Instance),
            ("=>", () => EqualGreaterThanToken.Instance),
            ("==", () => EqualEqualToken.Instance),
            ("!=", () => ExclEqualToken.Instance),


            ("@", () => ExecToken.Instance),
            ("<", () => LessThanToken.Instance),
            (">", () => GreaterThanToken.Instance),
            (";", () => SemiColonToken.Instance),
            (",", () => CommaToken.Instance),
            ("=", () => EqualToken.Instance),
            ("{", () => LBraceToken.Instance),
            ("}", () => RBraceToken.Instance),
            ("(", () => LParenToken.Instance),
            (")", () => RParenToken.Instance),
            ("[", () => LBracketToken.Instance),
            ("]", () => RBracketToken.Instance),


            ("+", () => PlusToken.Instance),
            ("-", () => MinusToken.Instance),
            ("*", () => StarToken.Instance),
            ("/", () => SlashToken.Instance),
            ("%", () => PercentToken.Instance),
            ("!", () => ExclToken.Instance),
            (".", () => DotToken.Instance),
            ("?", () => QuestionToken.Instance),
            ("&", () => AmpersandToken.Instance),

            (":", () => ColonToken.Instance),
            ("`", () => BacktickToken.Instance)
        };

        public async ValueTask<LexResult> LexNormalModeAsync(LexerContext context, bool bSkipNewLine)
        {
            // 스킵처리
            var wsResult = await LexWhitespaceAsync(context, bSkipNewLine);
            if (wsResult.HasValue)
                context = wsResult.Context;

            // 끝 처리
            if (context.Pos.IsReachEnd())
                return new LexResult(EndOfFileToken.Instance, context);

            // 줄바꿈 문자
            var newLineResult = await LexNewLineAsync(context);
            if(newLineResult.HasValue)
                return new LexResult(newLineResult.Token, newLineResult.Context);

            // 여러개 먼저
            var intResult = await LexIntAsync(context);
            if (intResult.HasValue)
                return new LexResult(intResult.Token, intResult.Context);

            var boolResult = await LexBoolAsync(context);
            if (boolResult.HasValue)
                return new LexResult(boolResult.Token, boolResult.Context);

            foreach (var info in infos)
            {
                var consumeResult = await ConsumeAsync(info.Text, context.Pos);
                if (consumeResult.HasValue)
                    return new LexResult(info.Constructor(), context.UpdatePos(consumeResult.Value));
            }

            if (context.Pos.Equals('"'))
                return new LexResult(
                    DoubleQuoteToken.Instance, 
                    context.UpdatePos(await context.Pos.NextAsync()));

            var keywordResult = await LexKeywordAsync(context);
            if (keywordResult.HasValue)
                return new LexResult(keywordResult.Token, keywordResult.Context);


            // Identifier 시도
            var idResult = await LexIdentifierAsync(context, true);
            if (idResult.HasValue)
                return new LexResult(idResult.Token, idResult.Context);

            return LexResult.Invalid;
        }

        public async ValueTask<LexResult> LexCommandModeAsync(LexerContext context)
        {   
            var newLineResult = await LexNewLineAsync(context);
            if (newLineResult.HasValue)
                return new LexResult(NewLineToken.Instance, newLineResult.Context);

            // TODO: \} 처리
            if (context.Pos.Equals('}'))
                return new LexResult(RBraceToken.Instance, context.UpdatePos(await context.Pos.NextAsync()));
            
            if (context.Pos.Equals('$'))
            {                
                var nextDollarPos = await context.Pos.NextAsync();

                if (nextDollarPos.Equals('{'))
                {
                    return new LexResult(
                        DollarLBraceToken.Instance,
                        context.UpdatePos(await nextDollarPos.NextAsync()));
                }

                if (!nextDollarPos.Equals('$'))
                {
                    var idResult = await LexIdentifierAsync(context.UpdatePos(nextDollarPos), false);
                    if (idResult.HasValue)
                        return idResult;
                }
            }

            var sb = new StringBuilder();

            // 나머지는 text모드
            while(true)
            {
                // 끝 도달
                if (context.Pos.IsReachEnd()) break;
                
                // NewLine문자
                if (context.Pos.Equals('\r') || context.Pos.Equals('\n')) break;

                // TODO: \} 처리
                if (context.Pos.Equals('}'))
                    break;
                
                if (context.Pos.Equals('$'))
                {
                    var nextDollarPos = await context.Pos.NextAsync();
                    if (nextDollarPos.Equals('$'))
                    {
                        sb.Append('$');
                        context = context.UpdatePos(await nextDollarPos.NextAsync());
                        continue;
                    }

                    break;
                }

                context.Pos.AppendTo(sb);
                context = context.UpdatePos(await context.Pos.NextAsync());
            }

            if (0 < sb.Length)
                return new LexResult(new TextToken(sb.ToString()), context);

            return LexResult.Invalid;
        }

        async ValueTask<LexResult> LexKeywordAsync(LexerContext context)
        {
            var sb = new StringBuilder();
            BufferPosition curPos = context.Pos;

            while (IsIdentifierLetter(curPos))
            {
                curPos.AppendTo(sb);
                curPos = await curPos.NextAsync();
            }

            if (sb.Length == 0)
                return LexResult.Invalid;

            if (keywordInfos.TryGetValue(sb.ToString(), out var token))
                return new LexResult(token, context.UpdatePos(curPos));            

            return LexResult.Invalid;
        }

        async ValueTask<LexResult> LexIdentifierAsync(LexerContext context, bool bAllowRawMark)
        {
            var sb = new StringBuilder();
            BufferPosition curPos = context.Pos;

            if (bAllowRawMark && curPos.Equals('@'))
            {
                curPos = await curPos.NextAsync();
            }
            else if (IsIdentifierStartLetter(curPos))
            {
                curPos.AppendTo(sb);
                curPos = await curPos.NextAsync();
            }
            else
            {
                return LexResult.Invalid;
            }

            while (IsIdentifierLetter(curPos))
            {   
                curPos.AppendTo(sb);
                curPos = await curPos.NextAsync();
            }

            if (sb.Length == 0)
                return LexResult.Invalid;

            return new LexResult(new IdentifierToken(sb.ToString()), context.UpdatePos(curPos));
        }

        async ValueTask<BufferPosition?> ConsumeAsync(string text, BufferPosition pos)
        {
            foreach (var c in text)
            {
                if (!pos.Equals(c))
                    return null;

                pos = await pos.NextAsync();
            }

            return pos;
        }

        async ValueTask<LexResult> LexBoolAsync(LexerContext context)
        {
            var trueResult = await ConsumeAsync("true", context.Pos);
            if (trueResult.HasValue)
                return new LexResult(new BoolToken(true), context.UpdatePos(trueResult.Value));

            var falseResult = await ConsumeAsync("false", context.Pos);
            if (falseResult.HasValue)
                return new LexResult(new BoolToken(false), context.UpdatePos(falseResult.Value));

            return LexResult.Invalid;
        }

        internal async ValueTask<LexResult> LexIntAsync(LexerContext context)
        {
            var sb = new StringBuilder();
            BufferPosition curPos = context.Pos;

            while (curPos.GetUnicodeCategory() == UnicodeCategory.DecimalDigitNumber)
            {   
                curPos.AppendTo(sb);
                curPos = await curPos.NextAsync();
            }

            if (sb.Length == 0)
                return LexResult.Invalid;

            return new LexResult(new IntToken(int.Parse(sb.ToString())), context.UpdatePos(curPos));
        }

        async ValueTask<LexResult> LexStringModeTextAsync(LexerContext context)
        {
            var sb = new StringBuilder();
            var curPos = context.Pos;
            while (true) // 조심
            {
                if (curPos.IsReachEnd())
                    break;

                if (curPos.Equals('"')) // "두개 처리
                {
                    var secondPos = await curPos.NextAsync();
                    if (!secondPos.Equals('"')) break;

                    sb.Append('"');
                    curPos = await secondPos.NextAsync();
                }
                else if (curPos.Equals('$')) // $ 처리
                {
                    var secondPos = await curPos.NextAsync();
                    if (!secondPos.Equals('$')) break;
                    
                    sb.Append('$');
                    curPos = await secondPos.NextAsync();
                }
                else
                {
                    curPos.AppendTo(sb);
                    curPos = await curPos.NextAsync();
                }
            }

            if (sb.Length == 0)
                return LexResult.Invalid;

            return new LexResult(new TextToken(sb.ToString()), context.UpdatePos(curPos));
        }
        
        internal async ValueTask<LexResult> LexWhitespaceAsync(LexerContext context, bool bIncludeNewLine)
        {
            LexResult? nextLineModeFailedResult = null;

            bool bUpdated = false;
            while(true)
            {
                if (context.Pos.Equals('\\'))
                {
                    nextLineModeFailedResult = bUpdated ? new LexResult(WhitespaceToken.Instance, context) : LexResult.Invalid;
                    context = context.UpdatePos(await context.Pos.NextAsync());
                    continue;
                }

                // 코멘트 처리
                var commentBeginPos = await ConsumeAsync("//", context.Pos);

                if (!commentBeginPos.HasValue)
                    commentBeginPos = await ConsumeAsync("#", context.Pos);

                if (commentBeginPos.HasValue)
                {
                    context = context.UpdatePos(commentBeginPos.Value);

                    while (!context.Pos.IsReachEnd() && !context.Pos.Equals('\r') && !context.Pos.Equals('\n'))
                    {
                        context = context.UpdatePos(await context.Pos.NextAsync());
                        bUpdated = true;
                    }
                    continue;
                }

                if (context.Pos.IsWhiteSpace())
                {
                    context = context.UpdatePos(await context.Pos.NextAsync());
                    bUpdated = true;
                    continue;
                }

                if (bIncludeNewLine && (context.Pos.Equals('\r') || context.Pos.Equals('\n')))
                {
                    context = context.UpdatePos(await context.Pos.NextAsync());
                    bUpdated = true;
                    continue;
                }

                if (nextLineModeFailedResult.HasValue)
                {
                    var rnPos = await ConsumeAsync("\r\n", context.Pos);
                    if (rnPos.HasValue)
                    {
                        nextLineModeFailedResult = null;
                        context = context.UpdatePos(rnPos.Value);
                        bUpdated = true;
                        continue;
                    }
                    else if (context.Pos.Equals('\r') || context.Pos.Equals('\n'))
                    {
                        nextLineModeFailedResult = null;
                        context = context.UpdatePos(await context.Pos.NextAsync());
                        bUpdated = true;
                        continue;
                    }
                    else
                    {
                        // \ 이전을 리턴한다
                        return nextLineModeFailedResult.Value;
                    }
                }

                break;
            }

            return bUpdated ? new LexResult(WhitespaceToken.Instance, context) : LexResult.Invalid;
        }

        internal async ValueTask<LexResult> LexNewLineAsync(LexerContext context)
        {
            bool bUpdated = false;
            while (context.Pos.Equals('\r') || context.Pos.Equals('\n'))
            {
                context = context.UpdatePos(await context.Pos.NextAsync());
                bUpdated = true;
            }

            return bUpdated ? new LexResult(NewLineToken.Instance, context) : LexResult.Invalid;
        }
    }
}