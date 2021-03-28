using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using System.IO;

using S = Gum.Syntax;
using System;

namespace Gum.IR0Translator.Test
{
    public class AnalyzerTests
    {   

        //[Fact]
        //void TestAnalyzeIntLiteralExp()
        //{
        //    var typeValueFactory = new QsTypeValueFactory();
        //    var intTypeValue = typeValueFactory.GetTypeValue("int");

        //    var expAnalyzer = new QsExpAnalyzer(typeValueFactory);
        //    var context = new Context();

        //    var result = expAnalyzer.AnalyzeIntLiteralExp(new S.QsIntLiteralExp(3), context);

        //    Assert.False(context.HasError());
        //    Assert.Equal(result, intTypeValue);
        //}

        [Theory]
        [MemberData(nameof(GetScriptData))]
        public async Task TestAnalyzeScript(string file)
        {
            ParseResult<S.Script> scriptResult;

            using (var streamReader = new StreamReader(file))
            {
                var buffer = new Gum.Buffer(streamReader);
                var pos = await buffer.MakePosition().NextAsync();

                var lexer = new Lexer();
                var parser = new Parser(lexer);
                var parserContext = ParserContext.Make(LexerContext.Make(pos));
                scriptResult = await parser.ParseScriptAsync(parserContext);
            }

            Assert.True(scriptResult.HasValue);

            throw new NotImplementedException();

            // var runtimeModule = new RuntimeModule("/", "/");
            // var errors = new List<(object obj, string Message)>();

            // var app = new QsDefaultApplication(new QsTestCmdProvider(), runtimeModule);            

            // var context = scriptResult.Elem, errors, ImmutableArray.Create<IQsMetadata>(runtimeModule));
            
            // 통과만 하는 시나리오
            // Assert.False(context == null || 0 < errors.Count);
        }

        public static IEnumerable<object[]> GetScriptData()
        {
            foreach (var file in Directory.EnumerateFiles("Input", "*.qs"))
            {
                yield return new object[] { file };
            }
        }
    }

    public class TestData
    {
        public string DisplayName { get; }
        public string input { get; }
        public TestData(string displayName, string text)
        {
            DisplayName = displayName;
            input = text;
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }
    
}
