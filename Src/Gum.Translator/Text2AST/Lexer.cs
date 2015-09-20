using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Translator.Text2AST
{
    // 렉서
    public class Lexer
    {
        string source;
        LexerState state;        

        // properties
        public string TokenValue { get { return source.Substring(state.startIdx, state.tokenLen); } }
        public TokenType TokenType { get { return state.kind; } }
        public bool End { get { return state.startIdx == source.Length; } }

        public static Token[] keywords = new Token[] {
            new Token(TokenType.TrueValue, "true"),
            new Token(TokenType.FalseValue, "false"),
            new Token(TokenType.Return, "return"),
            new Token(TokenType.If, "if"),
            new Token(TokenType.Else, "else"),
            new Token(TokenType.For, "for"),
            new Token(TokenType.While, "while"),
            new Token(TokenType.Do, "do"),
            new Token(TokenType.Break, "break"),
            new Token(TokenType.Continue, "continue"),
            new Token(TokenType.Class, "class"),
            new Token(TokenType.Public, "public"),
            new Token(TokenType.Private, "private"),
            new Token(TokenType.Protected, "protected"),
            new Token(TokenType.Virtual, "virtual"),
            new Token(TokenType.Override, "override"),
            new Token(TokenType.New, "new"),
        };

        public static Token[] twoLetterTokens = new Token[] {
            new Token(TokenType.EqualEqual, "=="),
            new Token(TokenType.NotEqual, "!="),
            new Token(TokenType.LessEqual, "<="),
            new Token(TokenType.GreaterEqual, ">="),
            new Token(TokenType.AmperAmper, "&&"),
            new Token(TokenType.BarBar, "||"),
            new Token(TokenType.PlusEqual, "+="),
            new Token(TokenType.MinusEqual, "-="),
            new Token(TokenType.StarEqual, "*="),
            new Token(TokenType.SlashEqual, "/="),
            new Token(TokenType.PlusPlus, "++"),
            new Token(TokenType.MinusMinus, "--"),
        };

        // 인터페이스가?
        public Lexer(string s)
        {            
            source = s;
            state = new LexerState(0, 0, TokenType.Invalid);
        }

        public LexerScope CreateScope()
        {
            return new LexerScope(this);
        }

        // 현재 위치에서 다음 토큰을 구한다.
        public bool NextToken()
        {
            // 토큰 바로 다음 부터 시작한다, 
            int startIdx = state.startIdx + state.tokenLen;

            // whitespace 건너뛰기
            while(startIdx < source.Length)
            {
                if (!char.IsWhiteSpace(source[startIdx]))
                    break;

                startIdx++;
            }

            // 끝에 도달했다면            
            if (source.Length <= startIdx)
            {
                state = new LexerState(source.Length, 0, TokenType.Invalid);
                return false;
            }
            
            //IntValue, StringValue,                            // [0-9], "st\\ri\"ng" @"a""b\" <- 우선순위 밀림..

            // 두개짜리 먼저 == != <= >= && ||
            if (startIdx + 1 < source.Length)
            {
                string p = source.Substring(startIdx, 2);

                // 주석 처리, 
                if (p == "//")
                {
                    var idx = source.IndexOf('\n', startIdx + 2);
                    if (idx == -1)
                    {
                        state = new LexerState(source.Length, 0, TokenType.Invalid);
                        return false;
                    }

                    // 다음 줄까지 포인터를 이동하고 NextToken 수행
                    state = new LexerState(idx + 1, state.tokenLen, state.kind);
                    return false;
                }

                foreach(var info in twoLetterTokens)
                    if (info.Value == p)
                    {
                        state = new LexerState(startIdx, 2, info.Type);
                        return true;
                    }

                // TODO: 0x
                // if (p == "0x")
                //{
                    // 나머지는 숫자
                //}
            }

            char ch = source[startIdx];

            // 한개짜리.. 
            switch (ch)
            {
                case '{': state = new LexerState(startIdx, 1, TokenType.LBrace); return true;
                case '}': state = new LexerState(startIdx, 1, TokenType.RBrace); return true;

                case '[': state = new LexerState(startIdx, 1, TokenType.RBracket); return true;
                case ']': state = new LexerState(startIdx, 1, TokenType.RBracket); return true;
                
                case '(': state = new LexerState(startIdx, 1, TokenType.LParen); return true;
                case ')': state = new LexerState(startIdx, 1, TokenType.RParen); return true;
                case ',': state = new LexerState(startIdx, 1, TokenType.Comma); return true;
                case ';': state = new LexerState(startIdx, 1, TokenType.SemiColon); return true;
                case ':': state = new LexerState(startIdx, 1, TokenType.Colon); return true;
                case '.': state = new LexerState(startIdx, 1, TokenType.Dot); return true;
                case '!': state = new LexerState(startIdx, 1, TokenType.Not); return true;

                case '+': state = new LexerState(startIdx, 1, TokenType.Plus); return true;
                case '-': state = new LexerState(startIdx, 1, TokenType.Minus); return true;
                case '*': state = new LexerState(startIdx, 1, TokenType.Star); return true;
                case '/': state = new LexerState(startIdx, 1, TokenType.Slash); return true;

                case '<': state = new LexerState(startIdx, 1, TokenType.Less); return true;
                case '>': state = new LexerState(startIdx, 1, TokenType.Greater); return true;
                case '=': state = new LexerState(startIdx, 1, TokenType.Equal); return true;
            }

            // string 처리
            if (ch == '"')
            {
                // TODO: support escape sequence
                // TODO: support long string
                // 지금은 "가 나올때까지 계속 읽는다..  단 \"가 나왔으면 넘어가기..
                int endIdx = startIdx + 1;

                while (endIdx < source.Length)
                {   
                    if (source[endIdx] == '"' &&
                        (endIdx - 1 <= startIdx || source[endIdx - 1] != '\\'))
                    {
                        state = new LexerState(startIdx, endIdx - startIdx + 1, TokenType.StringValue);
                        return true;
                    }

                    endIdx++;
                }

                state = new LexerState(source.Length, 0, TokenType.Invalid);
                return false;
            }
            
            // 숫자부터 처리
            if ('0' <= ch && ch <= '9')
            {
                // 숫자가 계속
                int endIdx = startIdx + 1;

                while (endIdx < source.Length)
                {
                    char d = source[endIdx];
                    if (!('0' <= d && d <= '9')) break;

                    endIdx++;
                }

                state = new LexerState(startIdx, endIdx - startIdx, TokenType.IntValue);
                return true;
            }

            // identifier
            if (('a' <= ch && ch <= 'z') ||
                ('A' <= ch && ch <= 'Z') ||
                ('_' == ch))
            {
                int endIdx = startIdx + 1;

                while (endIdx < source.Length)
                {
                    char d = source[endIdx];
                    if (!(('a' <= d && d <= 'z') ||
                        ('A' <= d && d <= 'Z') ||
                        ('0' <= d && d <= '9') ||
                        (d == '_')))
                        break;

                    endIdx++;
                }

                // we've get identifier..
                string sub = source.Substring(startIdx, endIdx - startIdx);

                foreach (var info in keywords)
                    if (info.Value == sub)
                    {
                        state = new LexerState(startIdx, endIdx - startIdx, info.Type);
                        return true;
                    }

                state = new LexerState(startIdx, endIdx - startIdx, TokenType.Identifier);
                return true;
            }

            state = new LexerState(source.Length, 0, TokenType.Invalid);
            return true;
        }

        public LexerState GetState()
        {
            return state;
        }

        public void SetState(LexerState pos)
        {
            this.state = pos;
        }

        public bool ConsumeAny(out TokenType res, params TokenType[] tokenKinds)
        {
            foreach (TokenType tk in tokenKinds)
            {
                if (TokenType == tk)
                {
                    NextToken();
                    res = tk;
                    return true;
                }
            }

            res = TokenType.Invalid;
            return false;
        }

        public bool ConsumeSeq(params TokenType[] tokenKinds)
        {
            using (LexerScope scope = CreateScope())
            {
                foreach (TokenType tk in tokenKinds)
                {
                    if (TokenType != tk) return false;
                    NextToken();
                }

                scope.Accept();
                return true;
            }
        }

        public bool Consume(TokenType tk)
        {
            if (TokenType != tk)
                return false;

            NextToken();
            return true;
        }

        public bool Consume(TokenType tk, out string token)
        {
            if (TokenType != tk) 
            {
                token = null;
                return false;
            }
            
            token = TokenValue;
            NextToken();
            return true;
        }
    }
}
