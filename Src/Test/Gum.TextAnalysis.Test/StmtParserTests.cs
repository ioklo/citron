using Gum.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

using static Gum.Infra.Misc;
using static Gum.TextAnalysis.Test.TestMisc;

namespace Gum.TextAnalysis.Test
{
    public class StmtParserTests
    {
        async ValueTask<(StmtParser, ParserContext)> PrepareAsync(string input)
        {
            var lexer = new Lexer();
            var parser = new Parser(lexer);

            var buffer = new Buffer(new StringReader(input));
            var bufferPos = await buffer.MakePosition().NextAsync();
            var lexerContext = LexerContext.Make(bufferPos);
            var context = ParserContext.Make(lexerContext);

            return (parser.stmtParser, context);
        }

        [Fact]
        async Task TestParseInlineCommandStmtAsync()
        {
            (var parser, var context) = await PrepareAsync("@echo ${a}bbb  ");
            
            var cmdStmt = await parser.ParseCommandStmtAsync(context);

            var expected = new CommandStmt(Arr(
                new StringExp(Arr<StringExpElement>(
                    new TextStringExpElement("echo "),
                    new ExpStringExpElement(SimpleSId("a")),
                    new TextStringExpElement("bbb  ")
                ))
            ));

            Assert.Equal(expected, cmdStmt.Elem);
        }

        [Fact]
        async Task TestParseBlockCommandStmtAsync()
        {
            var input = @"
@{ 
    echo ${ a } bbb   
xxx
}
";          
            (var parser, var context) = await PrepareAsync(input);

            var cmdStmt = await parser.ParseCommandStmtAsync(context);

            var expected = new CommandStmt(Arr(
                new StringExp(Arr<StringExpElement>(
                    new TextStringExpElement("    echo "),
                    new ExpStringExpElement(SimpleSId("a")),
                    new TextStringExpElement(" bbb   ")
                )),
                SimpleSStringExp("xxx")
            ));

            Assert.Equal(expected, cmdStmt.Elem);
        }

        [Fact]
        async Task TestParseVarDeclStmtAsync()
        {
            (var parser, var context) = await PrepareAsync("string a = \"hello\";");
            
            var varDeclStmt = await parser.ParseVarDeclStmtAsync(context);

            var expected = SimpleSVarDeclStmt(
                SimpleSIdTypeExp("string"),
                new VarDeclElement("a", SimpleSStringExp("hello"))
            );

            Assert.Equal<Stmt>(expected, varDeclStmt.Elem);
        }
        
        [Fact]
        async Task TestParseIfStmtAsync()
        {
            (var parser, var context) = await PrepareAsync("if (b) {} else if (c) {} else {}");
            
            var ifStmt = await parser.ParseIfStmtAsync(context);

            var expected = new IfStmt(SimpleSId("b"),
                null,
                SimpleSBlockStmt(),
                new IfStmt(
                    SimpleSId("c"),
                    null,
                    SimpleSBlockStmt(),
                    SimpleSBlockStmt()));

            Assert.Equal(expected, ifStmt.Elem);
        }

        [Fact]
        async Task TestParseIfStmtWithTestTypeAsync()
        {
            (var parser, var context) = await PrepareAsync("if (b is T) {} else if (c) {} else {}");

            var ifStmt = await parser.ParseIfStmtAsync(context);

            var expected = new IfStmt(SimpleSId("b"),
                SimpleSIdTypeExp("T"),
                SimpleSBlockStmt(),
                new IfStmt(
                    SimpleSId("c"),
                    null,
                    SimpleSBlockStmt(),
                    SimpleSBlockStmt()));

            Assert.Equal(expected, ifStmt.Elem);
        }

        [Fact]
        async Task TestParseForStmtAsync()
        {
            (var parser, var context) = await PrepareAsync(@"
for (f(); g; h + g) ;
");

            var result = await parser.ParseForStmtAsync(context);

            var expected = new ForStmt(
                new ExpForStmtInitializer(new CallExp(SimpleSId("f"), default)),
                SimpleSId("g"),
                new BinaryOpExp(BinaryOpKind.Add, SimpleSId("h"), SimpleSId("g")),
                BlankStmt.Instance);

            Assert.Equal(expected, result.Elem);
        }

        [Fact]
        async Task TestParseContinueStmtAsync()
        {
            (var parser, var context) = await PrepareAsync(@"continue;");
            var continueResult = await parser.ParseContinueStmtAsync(context);

            Assert.Equal(ContinueStmt.Instance, continueResult.Elem);
        }

        [Fact]
        async Task TestParseBreakStmtAsync()
        {
            (var parser, var context) = await PrepareAsync(@"break;");
            var breakResult = await parser.ParseBreakStmtAsync(context);

            Assert.Equal(BreakStmt.Instance, breakResult.Elem);
        }

        [Fact]
        async Task TestParseBlockStmtAsync()
        {
            (var parser, var context) = await PrepareAsync(@"{ { } { ; } ; }");
            var blockResult = await parser.ParseBlockStmtAsync(context);

            var expected = SimpleSBlockStmt(
                SimpleSBlockStmt(),
                SimpleSBlockStmt(BlankStmt.Instance),
                BlankStmt.Instance);

            Assert.Equal(expected, blockResult.Elem);
        }

        [Fact]
        async Task TestParseBlankStmtAsync()
        {
            (var parser, var context) = await PrepareAsync("  ;  ");
            var blankResult = await parser.ParseBlankStmtAsync(context);

            Assert.Equal(BlankStmt.Instance, blankResult.Elem);
        }

        [Fact]
        async Task TestParseExpStmtAsync()
        {
            (var parser, var context) = await PrepareAsync("a = b * c(1);");
            var expResult = await parser.ParseExpStmtAsync(context);

            var expected = new ExpStmt(new BinaryOpExp(BinaryOpKind.Assign,
                SimpleSId("a"),
                new BinaryOpExp(BinaryOpKind.Multiply,
                    SimpleSId("b"),
                    new CallExp(SimpleSId("c"), Arr<Argument>(new Argument.Normal(new IntLiteralExp(1)))))));
                

            Assert.Equal(expected, expResult.Elem);
        }

        [Fact]
        async Task TestParseForeachStmtAsync()
        {
            var (parser, context) = await PrepareAsync("foreach( var x in l ) { } ");
            var stmtResult = await parser.ParseForeachStmtAsync(context);

            var expected = new ForeachStmt(SimpleSIdTypeExp("var"), "x", SimpleSId("l"), SimpleSBlockStmt());

            Assert.Equal(expected, stmtResult.Elem);
        }
    }
}
