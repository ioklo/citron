namespace Citron;

using System.IO;

static class TextAnalysisTestMisc
{
    public static LexerContext MakeLexerContext(string text)
    {
        var buffer = new Buffer(new StringReader(text));
        return LexerContext.Make(buffer.MakePosition().Next());
    }

    public static ParserContext MakeParserContext(string input)
    {
        var buffer = new Buffer(new StringReader(input));
        var bufferPos = buffer.MakePosition().Next();
        var lexerContext = LexerContext.Make(bufferPos);
        return ParserContext.Make(lexerContext);
    }
}
