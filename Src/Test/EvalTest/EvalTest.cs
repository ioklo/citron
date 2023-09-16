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
using Citron.Analysis;
using Citron.Symbol;

using static Citron.Infra.Misc;

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

        static ModuleDeclSymbol MakeRuntimeModuleDS()
        {   
            var runtimeModuleDecl = new ModuleDeclSymbol(NormalName("System.Runtime"), bReference: true);
            var systemNSDecl = new NamespaceDeclSymbol(runtimeModuleDecl, NormalName("System"));
            runtimeModuleDecl.AddNamespace(systemNSDecl);

            var boolDecl = new StructDeclSymbol(systemNSDecl, Accessor.Public, NormalName("Bool"), typeParams: default);
            systemNSDecl.AddType(boolDecl);

            var intDecl = new StructDeclSymbol(systemNSDecl, Accessor.Public, NormalName("Int32"), typeParams: default);
            systemNSDecl.AddType(intDecl);

            var stringDecl = new ClassDeclSymbol(systemNSDecl, Accessor.Public, NormalName("String"), typeParams: default);
            systemNSDecl.AddType(stringDecl);

            var listDecl = new ClassDeclSymbol(systemNSDecl, Accessor.Public, NormalName("List"), Arr<Name>(NormalName("TItem")));
            systemNSDecl.AddType(listDecl);

            return runtimeModuleDecl;
        }

        // 실행이 잘 끝나는 테스트
        public static async ValueTask Test(string text, string expected)
        {
            var lexer = new Lexer();

            var buffer = new Citron.Buffer(new StringReader(text));
            var bufferPos = buffer.MakePosition().Next();
            var lexerContext = LexerContext.Make(bufferPos);
            var parserContext = ParserContext.Make(lexerContext);

            bool succeeded = ScriptParser.Parse(lexer, ref parserContext, out var script);
            Assert.True(succeeded);
            Debug.Assert(script != null);

            var logger = new TestLogger(false);
            var commandProvider = new TestCmdProvider();

            try
            {
                var runtimeModuleDS = MakeRuntimeModuleDS();
                var symbolFactory = new SymbolFactory();

                var rscript = SyntaxIR0Translator.Build(new Name.Normal("TestModule"), Arr(script), Arr(runtimeModuleDS), symbolFactory, logger);
                MyAssert.Assert(rscript != null);

                var retValue = await IR0Evaluator.EvalAsync(default, commandProvider, rscript); // retValue는 지금 쓰지 않는다
            }
            catch (Exception)
            {
                Assert.True(logger.HasError, "실행은 중간에 멈췄는데 에러로그가 남지 않았습니다");
            }

            Assert.False(logger.HasError, logger.GetMessages());
            Assert.Equal(expected, commandProvider.Output);
        }

    }
    
    public class EvalTest
    {
        [Theory]
        [ClassData(typeof(EvalTestDataFactory))]
        public async Task TestEvaluateScript(EvalTestData data)
        {
            string text;
            using (var reader = new StreamReader(data.Path))
                text = reader.ReadToEnd();

            string expected;
            if (data.OverriddenResult != null)
            {
                expected = data.OverriddenResult;
            }
            else
            {
                Assert.StartsWith("// ", text);

                int firstLineEnd = text.IndexOfAny(new char[] { '\r', '\n' });
                Assert.True(firstLineEnd != -1);

                expected = text.Substring(3, firstLineEnd - 3);
            }

            var lexer = new Lexer();

            var buffer = new Citron.Buffer(new StringReader(text));
            var bufferPos = buffer.MakePosition().Next();
            var lexerContext = LexerContext.Make(bufferPos);
            var parserContext = ParserContext.Make(lexerContext);

            bool succeeded = ScriptParser.Parse(lexer, ref parserContext, out var script);
            Assert.True(succeeded);

            var logger = new TestLogger(false);
            var commandProvider = new TestCmdProvider();

            try
            {
                var rscript = SyntaxIR0Translator.Build(new Name.Normal("TestModule"), Arr(script), refModuleDecls: default, scriptResult.Elem, logger);
                MyAssert.Assert(rscript != null);

                var retValue = await Evaluator.EvalAsync(default, commandProvider, rscript); // retValue는 지금 쓰지 않는다
            }
            catch(Exception)
            {
                Assert.True(logger.HasError, "실행은 중간에 멈췄는데 에러로그가 남지 않았습니다");
            }

            Assert.False(logger.HasError, logger.GetMessages());
            Assert.Equal(expected, commandProvider.Output);
        }
    }
    
    public class EvalTestData : IXunitSerializable
    {
        public string Path { get; private set; }
        public string? OverriddenResult { get; private set; }

        public EvalTestData()
        {
            Path = string.Empty;
            OverriddenResult = null;
        }

        public EvalTestData(string path, string? overridenResult = null)
        {
            Path = path;
            OverriddenResult = overridenResult;
        }

        public override string ToString()
        {
            return Path;
        }

        public void Deserialize(IXunitSerializationInfo info)
        {
            Path = info.GetValue<string>("Path");
            OverriddenResult = info.GetValue<string?>("OverriddenResult");
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue("Path", Path);
            info.AddValue("OverriddenResult", OverriddenResult);
        }
    }

    class EvalTestDataFactory : IEnumerable<object[]>
    {
        Dictionary<string, string> overriddenResults = new Dictionary<string, string>
        {
            [Path.Combine("Input", "Env", "01_Env.qs")] = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        };

        public IEnumerator<object[]> GetEnumerator()
        {
            var curDir = Directory.GetCurrentDirectory();

            foreach (var path in Directory.EnumerateFiles(curDir, "*.qs", SearchOption.AllDirectories))
            {
                if (Path.GetFileNameWithoutExtension(path).Contains("_TODO_"))
                    continue;

                var relPath = Path.GetRelativePath(curDir, path);

                var result = overriddenResults.GetValueOrDefault(relPath);
                yield return new object[] { new EvalTestData(relPath, result) };                
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
