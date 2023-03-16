using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using S = Citron.Syntax;
using R = Citron.IR0;
using Citron.IR0Translator;
using System.IO;
using Xunit;
using System.Text.Json;
using Citron.IR0Evaluator;
using Citron.Collections;
using System.Reflection;
using System.Diagnostics;
using Citron.Log;

namespace Citron.Test.IntegrateTest
{
    static class Misc
    {   
        class TestCmdProvider : IIR0CommandProvider
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
            var testLogger = new TestLogger(false);

            var lexer = new Lexer();
            var parser = new Parser(lexer);

            var buffer = new Citron.Buffer(new StringReader(code));
            var bufferPos = await buffer.MakePosition().NextAsync();
            var lexerContext = LexerContext.Make(bufferPos);
            var parserContext = ParserContext.Make(lexerContext);

            var sscriptResult = await parser.ParseScriptAsync(parserContext);
            var rscriptResult = Translator.Translate("TestModule", default, sscript, testLogger);

            Assert.Equal(sscript, sscriptResult.Elem);

            var text0 = IR0Writer.ToString(rscript);
            var text1 = IR0Writer.ToString(rscriptResult!);


            Assert.Equal(rscript, rscriptResult!);

        }

        public static async Task TestParseTranslateWithErrorAsync(string code, S.Script sscript, ILog expectedLog)
        {
            var testLogger = new TestLogger(false);

            var lexer = new Lexer();
            var parser = new Parser(lexer);

            var buffer = new Buffer(new StringReader(code));
            var bufferPos = await buffer.MakePosition().NextAsync();
            var lexerContext = LexerContext.Make(bufferPos);
            var parserContext = ParserContext.Make(lexerContext);

            var sscriptResult = await parser.ParseScriptAsync(parserContext);
            var rscriptResult = Translator.Translate("TestModule", default, sscript, testLogger);

            var result = testLogger.Logs
                .Any(log => log.Equals(expectedLog));

            Assert.True(result, $"Logs doesn't contain {expectedLog}");
        }

        public static async Task TestEvalAsync(string code, string result)
        {
            var testLogger = new TestLogger(false);

            var lexer = new Lexer();
            var parser = new Parser(lexer);

            var buffer = new Buffer(new StringReader(code));
            var bufferPos = await buffer.MakePosition().NextAsync();
            var lexerContext = LexerContext.Make(bufferPos);
            var parserContext = ParserContext.Make(lexerContext);

            var sscriptResult = await parser.ParseScriptAsync(parserContext);
            var rscriptResult = Translator.Translate("TestModule", default, sscriptResult.Elem, testLogger);

            var commandProvider = new TestCmdProvider();
            Debug.Assert(rscriptResult != null);

            var _ = await Evaluator.EvalAsync(default, commandProvider, rscriptResult);
            Assert.Equal(result, commandProvider.Output);
        }

        public static async Task TestEvalWithErrorAsync(string code, SyntaxAnalysisErrorCode errorCode)
        {
            var testLogger = new TestLogger(false);

            var lexer = new Lexer();
            var parser = new Parser(lexer);

            var buffer = new Buffer(new StringReader(code));
            var bufferPos = await buffer.MakePosition().NextAsync();
            var lexerContext = LexerContext.Make(bufferPos);
            var parserContext = ParserContext.Make(lexerContext);

            var sscriptResult = await parser.ParseScriptAsync(parserContext);
            var rscriptResult = Translator.Translate("TestModule", default, sscriptResult.Elem, testLogger);

            var commandProvider = new TestCmdProvider();

            var result = testLogger.Logs.OfType<SyntaxAnalysisErrorLog>()
                .Any(error => error.Code == errorCode);

            Assert.True(result, $"Errors doesn't contain (Code: {code}");
        }

        public static async Task TestParseTranslateEvalAsync(string code, S.Script sscript, R.Script rscript, string result)
        {
            var testLogger = new TestLogger(false);

            var lexer = new Lexer();
            var parser = new Parser(lexer);

            var buffer = new Buffer(new StringReader(code));
            var bufferPos = await buffer.MakePosition().NextAsync();
            var lexerContext = LexerContext.Make(bufferPos);
            var parserContext = ParserContext.Make(lexerContext);

            var sscriptResult = await parser.ParseScriptAsync(parserContext);
            var rscriptResult = Translator.Translate("TestModule", default, sscript, testLogger);

            var commandProvider = new TestCmdProvider();
            
            var _ = await Evaluator.EvalAsync(default, commandProvider, rscript); // retValue는 지금 쓰지 않는다
            
            Assert.Equal(sscript, sscriptResult.Elem);
            Assert.Equal(rscript, rscriptResult!);
            Assert.Equal(result, commandProvider.Output);
        }
    }
}
