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
        public async Task TestParseInlineCommandStmtAsync()
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
        public async Task TestParseBlockCommandStmtAsync()
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
        public async Task TestParseVarDeclStmtAsync()
        {
            (var parser, var context) = await PrepareAsync("string a = \"hello\";");
            
            var varDeclStmt = await parser.ParseVarDeclStmtAsync(context);

            var expected = SimpleSVarDeclStmt(
                SimpleSIdTypeExp("string"),
                new VarDeclElement("a", new VarDeclElemInitializer(false, SimpleSStringExp("hello")))
            );

            Assert.Equal<Stmt>(expected, varDeclStmt.Elem);
        }

        [Fact]
        public async Task TestParseRefVarDeclStmtAsync()
        {
            // ref int p;
            (var parser, var context) = await PrepareAsync("ref int p;");

            var varDeclStmt = await parser.ParseVarDeclStmtAsync(context);

            var expected = new VarDeclStmt(new VarDecl(true, SimpleSIdTypeExp("int"), Arr(new VarDeclElement("p", null))));

            Assert.Equal(expected, varDeclStmt.Elem);
        }

        [Fact]
        public async Task TestParseIfStmtAsync()
        {
            (var parser, var context) = await PrepareAsync("if (b) {} else if (c) {} else {}");
            
            var ifStmt = await parser.ParseIfStmtAsync(context);

            var expected = new IfStmt(SimpleSId("b"),
                SimpleSBlockStmt(),
                new IfStmt(
                    SimpleSId("c"),
                    SimpleSBlockStmt(),
                    SimpleSBlockStmt()));

            Assert.Equal(expected, ifStmt.Elem);
        }

        [Fact]
        public async Task TestParseIfTestStmtWithoutVarNameAsync()
        {
            (var parser, var context) = await PrepareAsync("if (b is T) {} else if (c) {} else {}");

            var ifStmt = await parser.ParseIfStmtAsync(context);

            var expected = new IfTestStmt(SimpleSId("b"),
                SimpleSIdTypeExp("T"), 
                null,
                SimpleSBlockStmt(),
                new IfStmt(
                    SimpleSId("c"),
                    SimpleSBlockStmt(),
                    SimpleSBlockStmt()));

            Assert.Equal(expected, ifStmt.Elem);
        }

        [Fact]
        public async Task TestParseIfTestStmtWithVarNameAsync()
        {
            (var parser, var context) = await PrepareAsync("if (b is T t) {} else if (c) {} else {}");

            var ifStmt = await parser.ParseIfStmtAsync(context);

            var expected = new IfTestStmt(SimpleSId("b"),
                SimpleSIdTypeExp("T"),
                "t",
                SimpleSBlockStmt(),
                new IfStmt(
                    SimpleSId("c"),
                    SimpleSBlockStmt(),
                    SimpleSBlockStmt()));

            Assert.Equal(expected, ifStmt.Elem);
        }

        [Fact]
        public async Task TestParseForStmtAsync()
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
        public async Task TestParseContinueStmtAsync()
        {
            (var parser, var context) = await PrepareAsync(@"continue;");
            var continueResult = await parser.ParseContinueStmtAsync(context);

            Assert.Equal(ContinueStmt.Instance, continueResult.Elem);
        }

        [Fact]
        public async Task TestParseBreakStmtAsync()
        {
            (var parser, var context) = await PrepareAsync(@"break;");
            var breakResult = await parser.ParseBreakStmtAsync(context);

            Assert.Equal(BreakStmt.Instance, breakResult.Elem);
        }

        [Fact]
        public async Task TestParseBlockStmtAsync()
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
        public async Task TestParseBlankStmtAsync()
        {
            (var parser, var context) = await PrepareAsync("  ;  ");
            var blankResult = await parser.ParseBlankStmtAsync(context);

            Assert.Equal(BlankStmt.Instance, blankResult.Elem);
        }

        [Fact]
        public async Task TestParseExpStmtAsync()
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
        public async Task TestParseForeachStmtAsync()
        {
            var (parser, context) = await PrepareAsync("foreach( var x in l ) { } ");
            var stmtResult = await parser.ParseForeachStmtAsync(context);

            var expected = new ForeachStmt(false, SimpleSIdTypeExp("var"), "x", SimpleSId("l"), SimpleSBlockStmt());

            Assert.Equal(expected, stmtResult.Elem);
        }

        [Fact]
        public async Task TestParseDirectiveStmtAsync()
        {
            var (parser, context) = await PrepareAsync("`notnull(a);");
            var stmtResult = await parser.ParseStmtAsync(context);

            var expected = new DirectiveStmt("notnull", Arr<Exp>(SimpleSId("a")));

            Assert.Equal(expected, stmtResult.Elem);
        }
    }
}
