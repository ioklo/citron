using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Compiler
{
    // 텍스트를 토큰단위로 끊어줍니다
    public partial class Lexer
    {
        string source;
        State state;        

        // properties
        public string TokenValue { get { return source.Substring(state.StartIndex, state.TokenLength); } }
        public TokenType TokenType { get { return state.TokenType; } }
        public bool End { get { return state.StartIndex == source.Length; } }

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
            new Token(TokenType.Using, "using"),
            new Token(TokenType.Namespace, "namespace"),
            new Token(TokenType.Static, "static"),
        };

        public static Token[] threeLetterTokens = new Token[]
        {
            new Token(TokenType.LessLessEqual, "<<="), // 3
            new Token(TokenType.GreaterGreaterEqual, ">>="), // 3
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
            new Token(TokenType.PercentEqual, "%="),
            
            new Token(TokenType.AmperEqual, "&="),
            new Token(TokenType.CaretEqual, "^="),
            new Token(TokenType.BarEqual, "|="),
            new Token(TokenType.PlusPlus, "++"),
            new Token(TokenType.MinusMinus, "--"),

            new Token(TokenType.LessLess, "<<"),
            new Token(TokenType.GreaterGreater, ">>"),
        };

        // 인터페이스가?
        public Lexer(string s)
        {            
            source = s;
            NextToken();
        }

        public Scope CreateScope()
        {
            return new Scope(this);
        }

        // 현재 위치에서 다음 토큰을 구한다.
        public bool NextToken()
        {
            // 토큰 바로 다음 부터 시작한다, 
            int startIdx = state.StartIndex + state.TokenLength;

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
                state = new State(source.Length, 0, TokenType.Invalid);
                return false;
            }

            //IntValue, StringValue,                            // [0-9], "st\\ri\"ng" @"a""b\" <- 우선순위 밀림..

            // 세개짜리 먼저 <<= >>=
            if( startIdx + 2 < source.Length)
            {
                string p = source.Substring(startIdx, 3);

                foreach (var info in threeLetterTokens)
                    if (info.Value == p)
                    {
                        state = new State(startIdx, 2, info.Type);
                        return true;
                    }
            }

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
                        state = new State(source.Length, 0, TokenType.Invalid);
                        return false;
                    }

                    // 다음 줄까지 포인터를 이동하고 NextToken 수행
                    state = new State(idx + 1, state.TokenLength, state.TokenType);
                    return false;
                }

                foreach(var info in twoLetterTokens)
                    if (info.Value == p)
                    {
                        state = new State(startIdx, 2, info.Type);
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
                case '{': state = new State(startIdx, 1, TokenType.LBrace); return true;
                case '}': state = new State(startIdx, 1, TokenType.RBrace); return true;

                case '[': state = new State(startIdx, 1, TokenType.RBracket); return true;
                case ']': state = new State(startIdx, 1, TokenType.RBracket); return true;
                
                case '(': state = new State(startIdx, 1, TokenType.LParen); return true;
                case ')': state = new State(startIdx, 1, TokenType.RParen); return true;
                case ',': state = new State(startIdx, 1, TokenType.Comma); return true;
                case ';': state = new State(startIdx, 1, TokenType.SemiColon); return true;
                case ':': state = new State(startIdx, 1, TokenType.Colon); return true;
                case '.': state = new State(startIdx, 1, TokenType.Dot); return true;

                case '+': state = new State(startIdx, 1, TokenType.Plus); return true;
                case '-': state = new State(startIdx, 1, TokenType.Minus); return true;
                case '*': state = new State(startIdx, 1, TokenType.Star); return true;
                case '/': state = new State(startIdx, 1, TokenType.Slash); return true;
                case '%': state = new State(startIdx, 1, TokenType.Percent); return true;
                case '^': state = new State(startIdx, 1, TokenType.Caret); return true;

                case '<': state = new State(startIdx, 1, TokenType.Less); return true;
                case '>': state = new State(startIdx, 1, TokenType.Greater); return true;
                case '=': state = new State(startIdx, 1, TokenType.Equal); return true;

                case '&': state = new State(startIdx, 1, TokenType.Amper); return true;
                case '!': state = new State(startIdx, 1, TokenType.Exclamation); return true;
                case '~': state = new State(startIdx, 1, TokenType.Tilde); return true;
                case '|': state = new State(startIdx, 1, TokenType.Bar); return true;
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
                        state = new State(startIdx, endIdx - startIdx + 1, TokenType.StringValue);
                        return true;
                    }

                    endIdx++;
                }

                state = new State(source.Length, 0, TokenType.Invalid);
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

                state = new State(startIdx, endIdx - startIdx, TokenType.IntValue);
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
                        state = new State(startIdx, endIdx - startIdx, info.Type);
                        return true;
                    }

                state = new State(startIdx, endIdx - startIdx, TokenType.Identifier);
                return true;
            }

            state = new State(source.Length, 0, TokenType.Invalid);
            return true;
        }

        private State GetState()
        {
            return state;
        }

        private void SetState(State pos)
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
            using (Scope scope = CreateScope())
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
