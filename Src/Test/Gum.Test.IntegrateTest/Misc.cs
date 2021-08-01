using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Gum.Test.Misc;

using S = Gum.Syntax;
using R = Gum.IR0;
using Gum.IR0Translator;
using System.IO;
using Xunit;
using System.Text.Json;
using Gum.IR0Evaluator;
using Gum.Collections;
using System.Reflection;

namespace Gum.Test.IntegrateTest
{
    static class Misc
    {
        public static R.Path.Nested Child(this R.Path.Normal outer, R.Name name, R.ParamHash paramHash, ImmutableArray<R.Path> typeArgs)
            => new R.Path.Nested(outer, name, paramHash, typeArgs);

        public static R.Path.Nested Child(this R.Path.Normal outer, R.Name name)
            => new R.Path.Nested(outer, name, R.ParamHash.None, default);

        // no typeparams, all normal paramtypes
        public static R.Path.Nested Child(this R.Path.Normal outer, R.Name name, params R.Path[] types)
            => new R.Path.Nested(outer, name, new R.ParamHash(0, types.Select(type => new R.ParamHashEntry(R.ParamKind.Normal, type)).ToImmutableArray()), default);

        // no typeparams version
        public static R.Path.Nested Child(this R.Path.Normal outer, R.Name name, params R.ParamHashEntry[] entries)
            => new R.Path.Nested(outer, name, new R.ParamHash(0, entries.ToImmutableArray()), default);

        class TestCmdProvider : ICommandProvider
        {
            public bool Error = false;
            public string Output { get => sb.ToString(); }
            StringBuilder sb = new StringBuilder();

            public async Task ExecuteAsync(string cmdText)
            {
                if (cmdText == "yield")
                {
                    // TODO: 좋은 방법이 있으면 교체한다
                    await Task.Delay(500);
                    return;
                }

                sb.Append(cmdText);
            }
        }

        public static async Task TestParseTranslateAsync(string code, S.Script sscript, R.Script rscript)
        {
            var testErrorCollector = new TestErrorCollector(false);

            var lexer = new Lexer();
            var parser = new Parser(lexer);

            var buffer = new Gum.Buffer(new StringReader(code));
            var bufferPos = await buffer.MakePosition().NextAsync();
            var lexerContext = LexerContext.Make(bufferPos);
            var parserContext = ParserContext.Make(lexerContext);

            var sscriptResult = await parser.ParseScriptAsync(parserContext);
            var rscriptResult = Translator.Translate("TestModule", default, sscript, testErrorCollector);

            Assert.Equal(sscript, sscriptResult.Elem);
            Assert.Equal(rscript, rscriptResult!);

        }

        public static async Task TestParseTranslateWithErrorAsync(string code, S.Script sscript, AnalyzeErrorCode errorCode, S.ISyntaxNode node)
        {
            var testErrorCollector = new TestErrorCollector(false);

            var lexer = new Lexer();
            var parser = new Parser(lexer);

            var buffer = new Buffer(new StringReader(code));
            var bufferPos = await buffer.MakePosition().NextAsync();
            var lexerContext = LexerContext.Make(bufferPos);
            var parserContext = ParserContext.Make(lexerContext);

            var sscriptResult = await parser.ParseScriptAsync(parserContext);
            var rscriptResult = Translator.Translate("TestModule", default, sscript, testErrorCollector);

            var result = testErrorCollector.Errors.OfType<AnalyzeError>()
                .Any(error => error.Code == errorCode && error.Node == node);

            Assert.True(result, $"Errors doesn't contain (Code: {code}, Node: {node})");
        }

        public static async Task TestEvalAsync(string code, string result)
        {
            var testErrorCollector = new TestErrorCollector(false);

            var lexer = new Lexer();
            var parser = new Parser(lexer);

            var buffer = new Buffer(new StringReader(code));
            var bufferPos = await buffer.MakePosition().NextAsync();
            var lexerContext = LexerContext.Make(bufferPos);
            var parserContext = ParserContext.Make(lexerContext);

            var sscriptResult = await parser.ParseScriptAsync(parserContext);
            var rscriptResult = Translator.Translate("TestModule", default, sscriptResult.Elem, testErrorCollector);

            var commandProvider = new TestCmdProvider();

            var _ = await Evaluator.EvalAsync(default, commandProvider, rscriptResult!);
            Assert.Equal(result, commandProvider.Output);
        }

        public static async Task TestEvalWithErrorAsync(string code, AnalyzeErrorCode errorCode)
        {
            var testErrorCollector = new TestErrorCollector(false);

            var lexer = new Lexer();
            var parser = new Parser(lexer);

            var buffer = new Buffer(new StringReader(code));
            var bufferPos = await buffer.MakePosition().NextAsync();
            var lexerContext = LexerContext.Make(bufferPos);
            var parserContext = ParserContext.Make(lexerContext);

            var sscriptResult = await parser.ParseScriptAsync(parserContext);
            var rscriptResult = Translator.Translate("TestModule", default, sscriptResult.Elem, testErrorCollector);

            var commandProvider = new TestCmdProvider();

            var result = testErrorCollector.Errors.OfType<AnalyzeError>()
                .Any(error => error.Code == errorCode);

            Assert.True(result, $"Errors doesn't contain (Code: {code}");
        }

        public static async Task TestParseTranslateEvalAsync(string code, S.Script sscript, R.Script rscript, string result)
        {
            var testErrorCollector = new TestErrorCollector(false);

            var lexer = new Lexer();
            var parser = new Parser(lexer);

            var buffer = new Buffer(new StringReader(code));
            var bufferPos = await buffer.MakePosition().NextAsync();
            var lexerContext = LexerContext.Make(bufferPos);
            var parserContext = ParserContext.Make(lexerContext);

            var sscriptResult = await parser.ParseScriptAsync(parserContext);
            var rscriptResult = Translator.Translate("TestModule", default, sscript, testErrorCollector);

            var commandProvider = new TestCmdProvider();
            
            var _ = await Evaluator.EvalAsync(default, commandProvider, rscript); // retValue는 지금 쓰지 않는다

            StringWriter writer0 = new StringWriter();
            Dumper.Dump(writer0, sscript);
            var text0 = writer0.ToString();

            StringWriter writer1 = new StringWriter();
            Dumper.Dump(writer1, sscriptResult.Elem!);
            var text1 = writer1.ToString();

            Assert.Equal(sscript, sscriptResult.Elem);
            Assert.Equal(rscript, rscriptResult!);
            Assert.Equal(result, commandProvider.Output);
        }
    }
}
