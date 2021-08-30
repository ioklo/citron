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
using System.Diagnostics;

namespace Gum.Test.IntegrateTest
{
    static class Misc
    {   
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

            var text0 = IR0Writer.ToString(rscript);
            var text1 = IR0Writer.ToString(rscriptResult!);


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
            Debug.Assert(rscriptResult != null);

            var _ = await Evaluator.EvalAsync(default, commandProvider, rscriptResult);
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
            
            Assert.Equal(sscript, sscriptResult.Elem);
            Assert.Equal(rscript, rscriptResult!);
            Assert.Equal(result, commandProvider.Output);
        }
    }
}
