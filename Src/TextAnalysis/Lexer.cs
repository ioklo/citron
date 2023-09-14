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

        public LexResult LexStringMode(LexerContext context)
        {   
            var textResult = LexStringModeText(context);
            if (textResult.HasValue)
                return textResult;

            if (context.Pos.Equals('"'))
                return new LexResult(
                    Tokens.DoubleQuote,
                    context.UpdatePos(context.Pos.Next()));

            if (context.Pos.Equals('$'))
            {
                var nextPos = context.Pos.Next();

                if (nextPos.Equals('{'))
                    return new LexResult(
                        Tokens.DollarLBrace,
                        context.UpdatePos(nextPos.Next()));

                var idResult = LexIdentifier(context.UpdatePos(nextPos), false);
                if (idResult.HasValue)
                    return idResult;
            }            

            return LexResult.Invalid;
        }

        // 키워드 처리
        private static Dictionary<string, Token> keywordInfos = new Dictionary<string, Token>()
        {
            { "foreach", Tokens.Foreach },
            { "if", Tokens.If },
            { "else", Tokens.Else },
            { "for", Tokens.For },
            { "continue", Tokens.Continue },
            { "break", Tokens.Break },
            { "task", Tokens.Task },
            { "params", Tokens.Params },
            { "return", Tokens.Return },
            { "async", Tokens.Async },
            { "await", Tokens.Await },
            { "in", Tokens.In },
            { "yield", Tokens.Yield },
            { "seq", Tokens.Seq },
            { "enum", Tokens.Enum },
            { "struct", Tokens.Struct },
            { "class", Tokens.Class },
            { "is", Tokens.Is },
            { "as", Tokens.As },
            { "ref", Tokens.Ref },
            { "box", Tokens.Box },
            { "null", Tokens.Null },
            { "public", Tokens.Public },
            { "protected", Tokens.Protected },
            { "private", Tokens.Private },
            { "static", Tokens.Static },
            { "new", Tokens.New },
            { "namespace", Tokens.Namespace },
        };

        private static (string Text, Func<Token> Constructor)[] infos = new (string Text, Func<Token> Constructor)[]
        {   
            ("++", () => Tokens.PlusPlus),
            ("--", () => Tokens.MinusMinus),
            ("<=", () => Tokens.LessThanEqual),
            (">=", () => Tokens.GreaterThanEqual),
            ("=>", () => Tokens.EqualGreaterThan),
            ("==", () => Tokens.EqualEqual),
            ("!=", () => Tokens.ExclEqual),
            ("->", () => Tokens.MinusGreaterThan),


            ("@", () => Tokens.At),
            ("<", () => Tokens.LessThan),
            (">", () => Tokens.GreaterThan),
            (";", () => Tokens.SemiColon),
            (",", () => Tokens.Comma),
            ("=", () => Tokens.Equal),
            ("{", () => Tokens.LBrace),
            ("}", () => Tokens.RBrace),
            ("(", () => Tokens.LParen),
            (")", () => Tokens.RParen),
            ("[", () => Tokens.LBracket),
            ("]", () => Tokens.RBracket),


            ("+", () => Tokens.Plus),
            ("-", () => Tokens.Minus),
            ("*", () => Tokens.Star),
            ("/", () => Tokens.Slash),
            ("%", () => Tokens.Percent),
            ("!", () => Tokens.Excl),
            (".", () => Tokens.Dot),
            ("?", () => Tokens.Question),
            ("&", () => Tokens.Ampersand),

            (":", () => Tokens.Colon),
            ("`", () => Tokens.Backtick)
        };

        public LexResult LexNormalMode(LexerContext context, bool bSkipNewLine)
        {
            // 스킵처리
            var wsResult = LexWhitespace(context, bSkipNewLine);
            if (wsResult.HasValue)
                context = wsResult.Context;

            // 끝 처리
            if (context.Pos.IsReachEnd())
                return new LexResult(Tokens.EndOfFile, context);

            // 줄바꿈 문자
            var newLineResult = LexNewLine(context);
            if(newLineResult.HasValue)
                return new LexResult(newLineResult.Token, newLineResult.Context);

            // 여러개 먼저
            var intResult = LexInt(context);
            if (intResult.HasValue)
                return new LexResult(intResult.Token, intResult.Context);

            var boolResult = LexBool(context);
            if (boolResult.HasValue)
                return new LexResult(boolResult.Token, boolResult.Context);

            foreach (var info in infos)
            {
                var consumeResult = Consume(info.Text, context.Pos);
                if (consumeResult.HasValue)
                    return new LexResult(info.Constructor(), context.UpdatePos(consumeResult.Value));
            }

            if (context.Pos.Equals('"'))
                return new LexResult(
                    Tokens.DoubleQuote, 
                    context.UpdatePos(context.Pos.Next()));

            var keywordResult = LexKeyword(context);
            if (keywordResult.HasValue)
                return new LexResult(keywordResult.Token, keywordResult.Context);


            // Identifier 시도
            var idResult = LexIdentifier(context, true);
            if (idResult.HasValue)
                return new LexResult(idResult.Token, idResult.Context);

            return LexResult.Invalid;
        }

        public LexResult LexCommandMode(LexerContext context)
        {   
            var newLineResult = LexNewLine(context);
            if (newLineResult.HasValue)
                return new LexResult(Tokens.NewLine, newLineResult.Context);

            // TODO: \} 처리
            if (context.Pos.Equals('}'))
                return new LexResult(Tokens.RBrace, context.UpdatePos(context.Pos.Next()));
            
            if (context.Pos.Equals('$'))
            {                
                var nextDollarPos = context.Pos.Next();

                if (nextDollarPos.Equals('{'))
                {
                    return new LexResult(
                        Tokens.DollarLBrace,
                        context.UpdatePos(nextDollarPos.Next()));
                }

                if (!nextDollarPos.Equals('$'))
                {
                    var idResult = LexIdentifier(context.UpdatePos(nextDollarPos), false);
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
                    var nextDollarPos = context.Pos.Next();
                    if (nextDollarPos.Equals('$'))
                    {
                        sb.Append('$');
                        context = context.UpdatePos(nextDollarPos.Next());
                        continue;
                    }

                    break;
                }

                context.Pos.AppendTo(sb);
                context = context.UpdatePos(context.Pos.Next());
            }

            if (0 < sb.Length)
                return new LexResult(new TextToken(sb.ToString()), context);

            return LexResult.Invalid;
        }

        LexResult LexKeyword(LexerContext context)
        {
            var sb = new StringBuilder();
            BufferPosition curPos = context.Pos;

            while (IsIdentifierLetter(curPos))
            {
                curPos.AppendTo(sb);
                curPos = curPos.Next();
            }

            if (sb.Length == 0)
                return LexResult.Invalid;

            if (keywordInfos.TryGetValue(sb.ToString(), out var token))
                return new LexResult(token, context.UpdatePos(curPos));            

            return LexResult.Invalid;
        }

        LexResult LexIdentifier(LexerContext context, bool bAllowRawMark)
        {
            var sb = new StringBuilder();
            BufferPosition curPos = context.Pos;

            if (bAllowRawMark && curPos.Equals('@'))
            {
                curPos = curPos.Next();
            }
            else if (IsIdentifierStartLetter(curPos))
            {
                curPos.AppendTo(sb);
                curPos = curPos.Next();
            }
            else
            {
                return LexResult.Invalid;
            }

            while (IsIdentifierLetter(curPos))
            {   
                curPos.AppendTo(sb);
                curPos = curPos.Next();
            }

            if (sb.Length == 0)
                return LexResult.Invalid;

            return new LexResult(new IdentifierToken(sb.ToString()), context.UpdatePos(curPos));
        }

        BufferPosition? Consume(string text, BufferPosition pos)
        {
            foreach (var c in text)
            {
                if (!pos.Equals(c))
                    return null;

                pos = pos.Next();
            }

            return pos;
        }

        LexResult LexBool(LexerContext context)
        {
            var trueResult = Consume("true", context.Pos);
            if (trueResult.HasValue)
                return new LexResult(new BoolToken(true), context.UpdatePos(trueResult.Value));

            var falseResult = Consume("false", context.Pos);
            if (falseResult.HasValue)
                return new LexResult(new BoolToken(false), context.UpdatePos(falseResult.Value));

            return LexResult.Invalid;
        }

        internal LexResult LexInt(LexerContext context)
        {
            var sb = new StringBuilder();
            BufferPosition curPos = context.Pos;

            while (curPos.GetUnicodeCategory() == UnicodeCategory.DecimalDigitNumber)
            {   
                curPos.AppendTo(sb);
                curPos = curPos.Next();
            }

            if (sb.Length == 0)
                return LexResult.Invalid;

            return new LexResult(new IntToken(int.Parse(sb.ToString())), context.UpdatePos(curPos));
        }

        LexResult LexStringModeText(LexerContext context)
        {
            var sb = new StringBuilder();
            var curPos = context.Pos;
            while (true) // 조심
            {
                if (curPos.IsReachEnd())
                    break;

                if (curPos.Equals('"')) // "두개 처리
                {
                    var secondPos = curPos.Next();
                    if (!secondPos.Equals('"')) break;

                    sb.Append('"');
                    curPos = secondPos.Next();
                }
                else if (curPos.Equals('$')) // $ 처리
                {
                    var secondPos = curPos.Next();
                    if (!secondPos.Equals('$')) break;
                    
                    sb.Append('$');
                    curPos = secondPos.Next();
                }
                else
                {
                    curPos.AppendTo(sb);
                    curPos = curPos.Next();
                }
            }

            if (sb.Length == 0)
                return LexResult.Invalid;

            return new LexResult(new TextToken(sb.ToString()), context.UpdatePos(curPos));
        }
        
        internal LexResult LexWhitespace(LexerContext context, bool bIncludeNewLine)
        {
            LexResult? nextLineModeFailedResult = null;

            bool bUpdated = false;
            while(true)
            {
                if (context.Pos.Equals('\\'))
                {
                    nextLineModeFailedResult = bUpdated ? new LexResult(Tokens.Whitespace, context) : LexResult.Invalid;
                    context = context.UpdatePos(context.Pos.Next());
                    continue;
                }

                // 코멘트 처리
                var commentBeginPos = Consume("//", context.Pos);

                if (!commentBeginPos.HasValue)
                    commentBeginPos = Consume("#", context.Pos);

                if (commentBeginPos.HasValue)
                {
                    context = context.UpdatePos(commentBeginPos.Value);

                    while (!context.Pos.IsReachEnd() && !context.Pos.Equals('\r') && !context.Pos.Equals('\n'))
                    {
                        context = context.UpdatePos(context.Pos.Next());
                        bUpdated = true;
                    }
                    continue;
                }

                if (context.Pos.IsWhiteSpace())
                {
                    context = context.UpdatePos(context.Pos.Next());
                    bUpdated = true;
                    continue;
                }

                if (bIncludeNewLine && (context.Pos.Equals('\r') || context.Pos.Equals('\n')))
                {
                    context = context.UpdatePos(context.Pos.Next());
                    bUpdated = true;
                    continue;
                }

                if (nextLineModeFailedResult.HasValue)
                {
                    var rnPos = Consume("\r\n", context.Pos);
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
                        context = context.UpdatePos(context.Pos.Next());
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

            return bUpdated ? new LexResult(Tokens.Whitespace, context) : LexResult.Invalid;
        }

        internal LexResult LexNewLine(LexerContext context)
        {
            bool bUpdated = false;
            while (context.Pos.Equals('\r') || context.Pos.Equals('\n'))
            {
                context = context.UpdatePos(context.Pos.Next());
                bUpdated = true;
            }

            return bUpdated ? new LexResult(Tokens.NewLine, context) : LexResult.Invalid;
        }
    }
}