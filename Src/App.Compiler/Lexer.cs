using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.App.Compiler
{
    // 렉서
    public class Lexer
    {
        public enum TokenKind
        {
            Invalid,

            LBrace, RBrace, // { }
            LBracket, RBracket, // []
            LParen, RParen,     // ( )
            Comma,              // ,
            SemiColon,          // ;
            Dot,                // .
            Colon,              // :

            PlusEqual, MinusEqual, StarEqual, SlashEqual, // +=, -=, *=, /=
            PlusPlus, MinusMinus, // ++, --
            
            Plus, Minus, Star, Slash,   // + - * /
            EqualEqual, NotEqual, Less, LessEqual, Greater, GreaterEqual, // == != < <= > >=
            AmperAmper, BarBar, Not, // && || !
            Equal,  // =

            // 한 종류가 여러가지 값을 가질 수 있는 것들
            Identifier,                                       // [a-zA-Z_][a-zA-Z0-9_]*
            TrueValue, FalseValue,                            // true, false
            IntValue, StringValue,                            // [0-9], "st\\ri\"ng" @"a""b\" <- 우선순위 밀림..

            Return, 

            // 기타 keywords.. 
            If, Else, For, While, Do,
            Break, Continue, 

            Class, Public, Private, Protected, Virtual, Override,          // class 용

            Out, Params,

            New, 
        }

        public class StringTokenKindInfo
        {
            public string Str { get; private set; }
            public TokenKind Kind { get; private set; }

            public StringTokenKindInfo(string str, TokenKind kind)
            {
                Str = str;
                Kind = kind;
            }
        }

        public struct State
        {
            public readonly int startIdx;
            public readonly int tokenLen;
            public readonly TokenKind kind;

            public State(int s, int t, TokenKind k)
            {
                startIdx = s;
                tokenLen = t;
                kind = k;
            }
        }

        string source;
        State state;
        StringTokenKindInfo[] twoLetterTokens;
        KeywordInfo[] keywordTokens;

        // properties
        public string Token { get { return source.Substring(state.startIdx, state.tokenLen); } }
        public TokenKind Kind { get { return state.kind; } }
        public bool End { get { return state.startIdx == source.Length; } }

        public Lexer(string s)
        {            
            source = s;
            state = new State(0, 0, TokenKind.Invalid);

            twoLetterTokens = new StringTokenKindInfo[] {
                new StringTokenKindInfo("==", TokenKind.EqualEqual),
                new StringTokenKindInfo("!=", TokenKind.NotEqual),
                new StringTokenKindInfo("<=", TokenKind.LessEqual),
                new StringTokenKindInfo(">=", TokenKind.GreaterEqual),
                new StringTokenKindInfo("&&", TokenKind.AmperAmper),
                new StringTokenKindInfo("||", TokenKind.BarBar),
                new StringTokenKindInfo("+=", TokenKind.PlusEqual),
                new StringTokenKindInfo("-=", TokenKind.MinusEqual),
                new StringTokenKindInfo("*=", TokenKind.StarEqual),
                new StringTokenKindInfo("/=", TokenKind.SlashEqual),
                new StringTokenKindInfo("++", TokenKind.PlusPlus),
                new StringTokenKindInfo("--", TokenKind.MinusMinus),
            };

            keywordTokens = new KeywordInfo[] {
                new KeywordInfo("true", TokenKind.TrueValue),                 
                new KeywordInfo("false", TokenKind.FalseValue),
                new KeywordInfo("return", TokenKind.Return),
                new KeywordInfo("if", TokenKind.If),
                new KeywordInfo("else", TokenKind.Else),
                new KeywordInfo("for", TokenKind.For),
                new KeywordInfo("while", TokenKind.While),
                new KeywordInfo("do", TokenKind.Do),
                new KeywordInfo("break", TokenKind.Break),
                new KeywordInfo("continue", TokenKind.Continue),
                new KeywordInfo("class", TokenKind.Class),
                new KeywordInfo("public", TokenKind.Public),
                new KeywordInfo("private", TokenKind.Private),
                new KeywordInfo("protected", TokenKind.Protected),
                new KeywordInfo("virtual", TokenKind.Virtual),
                new KeywordInfo("override", TokenKind.Override),
                new KeywordInfo("new", TokenKind.New),
            };
        }

        public LexerScope CreateScope()
        {
            return new LexerScope(this);
        }

        // 현재 위치에서 다음 토큰을 구한다.
        public TokenKind NextToken()
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
                state = new State(source.Length, 0, TokenKind.Invalid);
                return state.kind;
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
                        state = new State(source.Length, 0, TokenKind.Invalid);
                        return state.kind;
                    }

                    // 다음 줄까지 포인터를 이동하고 NextToken 수행
                    state = new State(idx + 1, state.tokenLen, state.kind);
                    return NextToken();
                }

                foreach(var info in twoLetterTokens)
                    if (info.Str == p)
                    {
                        state = new State(startIdx, 2, info.Kind);
                        return state.kind;
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
                case '{': state = new State(startIdx, 1, TokenKind.LBrace); return state.kind;
                case '}': state = new State(startIdx, 1, TokenKind.RBrace); return state.kind;

                case '[': state = new State(startIdx, 1, TokenKind.RBracket); return state.kind;
                case ']': state = new State(startIdx, 1, TokenKind.RBracket); return state.kind;
                
                case '(': state = new State(startIdx, 1, TokenKind.LParen); return state.kind;
                case ')': state = new State(startIdx, 1, TokenKind.RParen); return state.kind;
                case ',': state = new State(startIdx, 1, TokenKind.Comma); return state.kind;
                case ';': state = new State(startIdx, 1, TokenKind.SemiColon); return state.kind;
                case ':': state = new State(startIdx, 1, TokenKind.Colon); return state.kind;
                case '.': state = new State(startIdx, 1, TokenKind.Dot); return state.kind;
                case '!': state = new State(startIdx, 1, TokenKind.Not); return state.kind;

                case '+': state = new State(startIdx, 1, TokenKind.Plus); return state.kind;
                case '-': state = new State(startIdx, 1, TokenKind.Minus); return state.kind;
                case '*': state = new State(startIdx, 1, TokenKind.Star); return state.kind;
                case '/': state = new State(startIdx, 1, TokenKind.Slash); return state.kind;

                case '<': state = new State(startIdx, 1, TokenKind.Less); return state.kind;
                case '>': state = new State(startIdx, 1, TokenKind.Greater); return state.kind;
                case '=': state = new State(startIdx, 1, TokenKind.Equal); return state.kind;
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
                        state = new State(startIdx, endIdx - startIdx + 1, TokenKind.StringValue);
                        return state.kind;
                    }

                    endIdx++;
                }

                state = new State(source.Length, 0, TokenKind.Invalid);
                return state.kind;
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

                state = new State(startIdx, endIdx - startIdx, TokenKind.IntValue);
                return state.kind;
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

                foreach (var info in keywordTokens)
                    if (info.Token == sub)
                    {
                        state = new State(startIdx, endIdx - startIdx, info.Kind);
                        return state.kind;
                    }

                state = new State(startIdx, endIdx - startIdx, TokenKind.Identifier);
                return state.kind;
            }

            state = new State(source.Length, 0, TokenKind.Invalid);
            return state.kind;
        }

        public State GetState()
        {
            return state;
        }

        public void SetState(State pos)
        {
            this.state = pos;
        }

        public bool ConsumeAny(out TokenKind res, params TokenKind[] tokenKinds)
        {
            foreach (TokenKind tk in tokenKinds)
            {
                if (Kind == tk)
                {
                    NextToken();
                    res = tk;
                    return true;
                }
            }

            res = TokenKind.Invalid;
            return false;
        }

        public bool ConsumeSeq(params TokenKind[] tokenKinds)
        {
            using (LexerScope scope = CreateScope())
            {
                foreach (TokenKind tk in tokenKinds)
                {
                    if (Kind != tk) return false;
                    NextToken();
                }

                scope.Accept();
                return true;
            }
        }

        public bool Consume(TokenKind tk)
        {
            if (Kind != tk)
                return false;

            NextToken();
            return true;
        }

        public bool Consume(TokenKind tk, out string token)
        {
            if (Kind != tk) 
            {
                token = null;
                return false;
            }
            
            token = Token;
            NextToken();
            return true;
        }
    }

    public class LexerScope : IDisposable
    {
        Lexer lexer;
        Lexer.State state;
        bool bAccept = false;

        public LexerScope(Lexer lexer)
        {
            this.lexer = lexer;
            state = lexer.GetState();
        }

        public void Accept()
        {
            bAccept = true;
        }

        public void Dispose()
        {
            if (!bAccept)
                lexer.SetState(state);
        }
    }

}
