using System.Collections.Generic;

namespace Gum.Compiler
{
    public static class Parser<T, ParserT>
        where ParserT : Parser<T>, new()
    {
        private static ParserT parser = new ParserT();
        public static T Parse(Lexer lexer)
        {
            return parser.Parse(lexer);
        }
    }

    public interface IParser<out T>
    {
        T Parse(Lexer lexer);
    }

    public abstract class Parser<T> : IParser<T>
    {
        public static U Parse<U, ParserU>(Lexer lexer)
            where ParserU : Parser<U>, new()
        {
            return Parser<U, ParserU>.Parse(lexer);
        }

        public static List<C> ParseList<ParserH, C, ParserC, ParserS, ParserF>(Lexer lexer, ParserH headerParser, ParserC contentParser, ParserS separatorParser, ParserF footerParser)
            where ParserH : IParser<object>
            where ParserC : Parser<C>, new()
            where ParserS : IParser<object>
            where ParserF : IParser<object>
        {
            var contents = new List<C>();
            headerParser.Parse(lexer);

            C content = contentParser.Parse(lexer);
            contents.Add(content);

            while (separatorParser.Parse(lexer) != null)
            {
                content = contentParser.Parse(lexer);
                contents.Add(content);
            }

            footerParser.Parse(lexer);

            return contents;
        }


        public T Parse(Lexer lexer)
        {
            using (var lexerScope = lexer.CreateScope())
            {
                T t = ParseInner(lexer);
                if (t != null)
                {
                    lexerScope.Accept();
                    return t;
                }

                return default(T);
            }
        }

        protected abstract T ParseInner(Lexer lexer);
    }
}