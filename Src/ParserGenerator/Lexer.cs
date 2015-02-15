using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ParserGenerator
{
    class Lexer
    {
        string text;

        // 현재 위치와 길이
        int pos, len;
        Token kind;

        List<Tuple<string, Token>> strTokens = new List<Tuple<string, Token>>()
        {
            Tuple.Create("=", Token.Equal),
            Tuple.Create("*", Token.Star),
            Tuple.Create("|", Token.Bar),
            Tuple.Create("+", Token.Plus),
            Tuple.Create("?", Token.Question),
            Tuple.Create("(", Token.LParen),
            Tuple.Create(")", Token.RParen),
            Tuple.Create("{", Token.LBrace),
            Tuple.Create("}", Token.RBrace),
            Tuple.Create("[", Token.LBracket),
            Tuple.Create("]", Token.RBracket),
            Tuple.Create(",", Token.Comma),
        };

        // longest match를 어떻게 찾나
        List<Tuple<Regex, Token>> regexTokens = new List<Tuple<Regex, Token>>()
        {
            Tuple.Create(new Regex(@"\G""([^""\\]+|\\.)*"""), Token.String),
            Tuple.Create(new Regex(@"\Gr""([^""\\]+|\\.)*"""), Token.Regex),
            Tuple.Create(new Regex(@"\G[_a-zA-Z][0-9a-zA-Z]*"), Token.ID),
        };

        public string TokenString { get { return text.Substring(pos, len); } }

        public Lexer(string text)
        {
            this.text = text;
            pos = 0;
            len = 0;
            kind = Token.Invalid;
            NextToken();
        }

        public bool NextToken()
        {
            // whitespace와 주석 처리
            
            // pos + len 부터 시작해서 

            int start = pos + len;            

            // start 위치 잡기
            while (start < text.Length)
            {
                if (start + 1 < text.Length && text[start] == '/' && text[start + 1] == '/')
                {
                    while (start < text.Length)
                    {
                        if (text[start] == '\n')
                        {
                            start++;
                            break;
                        }

                        start++;
                    }
                    
                    continue;
                }

                if (!char.IsWhiteSpace(text[start]))
                {                    
                    break;
                }

                start++;
            }

            if (start == text.Length)
            {
                pos = start;
                len = 0;
                kind = Token.Invalid;
                return false;
            }

            foreach (var token in strTokens)
            {
                if (string.Compare(text, start, token.Item1, 0, token.Item1.Length) == 0)
                {
                    pos = start;
                    len = token.Item1.Length;
                    kind = token.Item2;
                    Console.WriteLine("Token: {0}", TokenString);
                    return true;
                }
            }

            foreach (var token in regexTokens)
            {
                var match = token.Item1.Match(text, start);
                if (match.Success)
                {
                    pos = start;
                    len = match.Length;
                    kind = token.Item2;
                    Console.WriteLine("Token: {0}", TokenString);
                    return true;
                }
            }

            //// end 위치 잡기
            //int end = start + 1;
            //while (end < text.Length)
            //{
            //    if (end + 1 < text.Length && text[end] == '/' && text[end + 1] == '/')
            //        break;

            //    string subString = text.Substring(start, end - start);


                

            //    if (char.IsWhiteSpace(text, end)) break;
            //    end++;
            //}

            //pos = start;
            //len = end - start;
            pos = text.Length;
            len = 0;
            kind = Token.Invalid;
            return false;
        }

        // 위치와 tag
        HashSet<Tuple<int, string>> tags = new HashSet<Tuple<int, string>>();
        internal LexerScope CreateScope(string tag)
        {
            var key = Tuple.Create(pos, tag);

            if (tags.Contains(key)) return null;

            Console.WriteLine(tag);
            var lexerScope = new LexerScope(this, pos, len, kind, tag);
            tags.Add(key);
            return lexerScope;
        }

        internal void CancelScope(LexerScope scope)
        {
            // 복구
            pos = scope.Pos;
            len = scope.Len;
            kind = scope.Kind;

            tags.Remove(Tuple.Create(scope.Pos, scope.Tag));
        }

        internal void AcceptScope(LexerScope scope)
        {
            tags.Remove(Tuple.Create(scope.Pos, scope.Tag));
        }

        internal bool Consume(Token kind, out string token)
        {
            if (this.kind == kind)
            {
                token = text.Substring(pos, len);
                NextToken();
                return true;
            }

            token = null;
            return false;
        }

        internal bool Consume(Token kind)
        {
            if (this.kind == kind)
            {
                NextToken();
                return true;
            }

            return false;
        }

        internal bool Peek(Token token)
        {
            return this.kind == token;
        }

        public bool EOF
        {
            get
            {
                return pos == text.Length;
            }
        }
    }
}
