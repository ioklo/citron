using Citron;
using Citron.Infra;
using Citron.Test;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Citron.Symbol;

using static Citron.Infra.Misc;
using Citron.Collections;
using Citron.Analysis;

namespace EvalTest
{
    static class MyAssert
    {
        public static void Assert([DoesNotReturnIf(false)] bool cond)
        {
            Xunit.Assert.True(cond);
        }
    }

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

    // code -> IR0 -> 
    public struct IR0EvaluationTester
    {
        static Name NormalName(string text) => new Name.Normal(text);

        // 실행이 잘 끝나는 테스트
        public static async ValueTask TestAsync(string text, string expected)
        {
            var lexer = new Lexer();

            var buffer = new Citron.Buffer(new StringReader(text));
            var bufferPos = buffer.MakePosition().Next();
            var lexerContext = LexerContext.Make(bufferPos);
            var parserContext = ParserContext.Make(lexerContext);

            bool succeeded = ScriptParser.Parse(lexer, ref parserContext, out var script);
            Assert.True(succeeded);
            Debug.Assert(script != null);

            var logger = new TestLogger(raiseAssertionFail: true);
            var commandProvider = new TestCmdProvider();
            var (moduleName, symbolFactory, r, runtimeModuleDS) = TestPreparations.Prepare();
            
            try
            {
                var rscript = SyntaxIR0Translator.Translate(NormalName("TestModule"), Arr(script), Arr(runtimeModuleDS), symbolFactory, logger);
                MyAssert.Assert(rscript != null);

                // moduleDrivers에 추가
                var moduleDriverInitializers = ImmutableArray<Action<ModuleDriverContext>>.Empty.Add(driverContext =>
                {
                    var evaluator = driverContext.GetEvaluator();

                    var symbolFactory = new SymbolFactory();
                    var symbolLoader = IR0Loader.Make(symbolFactory, rscript);
                    var globalContext = new IR0GlobalContext(evaluator, symbolLoader, moduleName, commandProvider);

                    IR0ModuleDriver.Init(driverContext, evaluator, globalContext, rscript.ModuleDeclSymbol.GetName());
                });

                var entry = new SymbolId(moduleName, new SymbolPath(null, NormalName("Main")));
                var retValue = await Evaluator.EvalAsync(moduleDriverInitializers, entry); // retValue는 지금 쓰지 않는다
            }
            catch (Exception)
            {
                Assert.True(logger.HasError, "실행은 중간에 멈췄는데 에러로그가 남지 않았습니다");
            }

            Assert.False(logger.HasError, logger.GetMessages());
            Assert.Equal(expected, commandProvider.Output);
        }
    }

    // 임시 주석 처리    
    //public class EvalTest
    //{
    //    [Theory]
    //    [ClassData(typeof(EvalTestDataFactory))]
    //    public async Task TestEvaluateScript(EvalTestData data)
    //    {
    //        string text;
    //        using (var reader = new StreamReader(data.Path))
    //            text = reader.ReadToEnd();

    //        string expected;
    //        if (data.OverriddenResult != null)
    //        {
    //            expected = data.OverriddenResult;
    //        }
    //        else
    //        {
    //            Assert.StartsWith("// ", text);

    //            int firstLineEnd = text.IndexOfAny(new char[] { '\r', '\n' });
    //            Assert.True(firstLineEnd != -1);

    //            expected = text.Substring(3, firstLineEnd - 3);
    //        }

    //        await IR0EvaluationTester.TestAsync(text, expected);
    //    }
    //}
    
    //public class EvalTestData : IXunitSerializable
    //{
    //    public string Path { get; private set; }
    //    public string? OverriddenResult { get; private set; }

    //    public EvalTestData()
    //    {
    //        Path = string.Empty;
    //        OverriddenResult = null;
    //    }

    //    public EvalTestData(string path, string? overridenResult = null)
    //    {
    //        Path = path;
    //        OverriddenResult = overridenResult;
    //    }

    //    public override string ToString()
    //    {
    //        return Path;
    //    }

    //    public void Deserialize(IXunitSerializationInfo info)
    //    {
    //        Path = info.GetValue<string>("Path");
    //        OverriddenResult = info.GetValue<string?>("OverriddenResult");
    //    }

    //    public void Serialize(IXunitSerializationInfo info)
    //    {
    //        info.AddValue("Path", Path);
    //        info.AddValue("OverriddenResult", OverriddenResult);
    //    }
    //}

    //class EvalTestDataFactory : IEnumerable<object[]>
    //{
    //    Dictionary<string, string> overriddenResults = new Dictionary<string, string>
    //    {
    //        [Path.Combine("Input", "Env", "01_Env.qs")] = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
    //    };

    //    public IEnumerator<object[]> GetEnumerator()
    //    {
    //        var curDir = Directory.GetCurrentDirectory();

    //        foreach (var path in Directory.EnumerateFiles(curDir, "*.qs", SearchOption.AllDirectories))
    //        {
    //            if (Path.GetFileNameWithoutExtension(path).Contains("_TODO_"))
    //                continue;

    //            var relPath = Path.GetRelativePath(curDir, path);

    //            var result = overriddenResults.GetValueOrDefault(relPath);
    //            yield return new object[] { new EvalTestData(relPath, result) };                
    //        }
    //    }

    //    IEnumerator IEnumerable.GetEnumerator()
    //    {
    //        return GetEnumerator();
    //    }
    //}
}
