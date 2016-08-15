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

        public static Dictionary<char, TokenType> singleLetterTokens = new Dictionary<char, TokenType>
        {
            { '{', TokenType.LBrace },
            { '}', TokenType.RBrace },

            { '[', TokenType.LBracket },
            { ']', TokenType.RBracket },
                
            { '(', TokenType.LParen },
            { ')', TokenType.RParen },
            { ',', TokenType.Comma },
            { ';', TokenType.SemiColon },
            { ':', TokenType.Colon },
            { '.', TokenType.Dot },

            { '+', TokenType.Plus },
            { '-', TokenType.Minus },
            { '*', TokenType.Star },
            { '/', TokenType.Slash },
            { '%', TokenType.Percent },
            { '^', TokenType.Caret },

            { '<', TokenType.Less },
            { '>', TokenType.Greater },
            { '=', TokenType.Equal },

            { '&', TokenType.Amper },
            { '!', TokenType.Exclamation },
            { '~', TokenType.Tilde },
            { '|', TokenType.Bar },
        };

        public static Dictionary<string, TokenType> keywords = new Dictionary<string, TokenType> 
        {
            { "true", TokenType.TrueValue },
            { "false", TokenType.FalseValue },
            { "return", TokenType.Return },
            { "if", TokenType.If },
            { "else", TokenType.Else },
            { "for", TokenType.For },
            { "while", TokenType.While },
            { "do", TokenType.Do },
            { "break", TokenType.Break },
            { "continue", TokenType.Continue },
            { "class", TokenType.Class },
            { "public", TokenType.Public },
            { "private", TokenType.Private },
            { "protected", TokenType.Protected },
            { "virtual", TokenType.Virtual },
            { "override", TokenType.Override },
            { "new", TokenType.New },
            { "using", TokenType.Using },
            { "namespace", TokenType.Namespace },
            { "static", TokenType.Static },
        };

        public static Dictionary<string, TokenType> twoLetterTokens = new Dictionary<string, TokenType>
        {
            { "==", TokenType.EqualEqual },
            { "!=", TokenType.NotEqual },
            { "<=", TokenType.LessEqual },
            { ">=", TokenType.GreaterEqual },
            { "&&", TokenType.AmperAmper },
            { "||", TokenType.BarBar },
            { "+=", TokenType.PlusEqual },
            { "-=", TokenType.MinusEqual },
            { "*=", TokenType.StarEqual },
            { "/=", TokenType.SlashEqual },
            { "%=", TokenType.PercentEqual },

            { "&=", TokenType.AmperEqual },
            { "^=", TokenType.CaretEqual },
            { "|=", TokenType.BarEqual },
            { "++", TokenType.PlusPlus },
            { "--", TokenType.MinusMinus },

            { "<<", TokenType.LessLess },
            { ">>", TokenType.GreaterGreater },
        };

        public static Dictionary<string, TokenType> threeLetterTokens = new Dictionary<string, TokenType>
        {
            { "<<=", TokenType.LessLessEqual }, 
            { ">>=", TokenType.GreaterGreaterEqual },
        };

        
        // lexer는 두가지 상태를 가지고 있습니다
        // 1. 뭔가를 받아들인 상태
        // 2. Invalid 상태 (더 이상 읽을 수 없는 상태일 떄)
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
                string token = source.Substring(startIdx, 3);

                TokenType threeLetterTokenType;
                if( threeLetterTokens.TryGetValue(token, out threeLetterTokenType) )
                {
                    state = new State(startIdx, 3, threeLetterTokenType);
                    return true;
                }
            }

            // 그 다음에 두개짜리 검색 == != <= >= && ||
            if (startIdx + 1 < source.Length)
            {
                string token = source.Substring(startIdx, 2);

                TokenType twoLetterTokenType;
                if( twoLetterTokens.TryGetValue(token, out twoLetterTokenType))
                {
                    state = new State(startIdx, 2, twoLetterTokenType);
                    return true;
                }

                // TODO: 0x
                // if (p == "0x")
                //{
                    // 나머지는 숫자
                //}
            }

            char ch = source[startIdx];

            // 한개짜리
            TokenType singleLetterTokenType;
            if( singleLetterTokens.TryGetValue(ch, out singleLetterTokenType))
            {
                state = new State(startIdx, 1, singleLetterTokenType);
                return true;
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
                string token = source.Substring(startIdx, endIdx - startIdx);

                TokenType keywordTokenType;
                if(keywords.TryGetValue(token, out keywordTokenType) )
                {
                    state = new State(startIdx, endIdx - startIdx, keywordTokenType);
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
