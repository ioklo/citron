using Gum.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Gum
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

            var expected = new CommandStmt(
                new StringExp(
                    new TextStringExpElement("echo "),
                    new ExpStringExpElement(new IdentifierExp("a")),
                    new TextStringExpElement("bbb  ")));

            Assert.Equal(expected, cmdStmt.Elem, SyntaxEqualityComparer.Instance);
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

            var expected = new CommandStmt(
                new StringExp(
                    new TextStringExpElement("    echo "),
                    new ExpStringExpElement(new IdentifierExp("a")),
                    new TextStringExpElement(" bbb   ")),
                new StringExp(new TextStringExpElement("xxx")));

            Assert.Equal(expected, cmdStmt.Elem, SyntaxEqualityComparer.Instance);
        }

        [Fact]
        async Task TestParseVarDeclStmtAsync()
        {
            (var parser, var context) = await PrepareAsync("string a = \"hello\";");
            
            var varDeclStmt = await parser.ParseVarDeclStmtAsync(context);

            var expected = new VarDeclStmt(new VarDecl(new IdTypeExp("string"),
                new VarDeclElement("a", new StringExp(new TextStringExpElement("hello")))));

            Assert.Equal(expected, varDeclStmt.Elem, SyntaxEqualityComparer.Instance);
        }
        
        [Fact]
        async Task TestParseIfStmtAsync()
        {
            (var parser, var context) = await PrepareAsync("if (b) {} else if (c) {} else {}");
            
            var ifStmt = await parser.ParseIfStmtAsync(context);

            var expected = new IfStmt(new IdentifierExp("b"),
                null,
                new BlockStmt(),
                new IfStmt(
                    new IdentifierExp("c"),
                    null,
                    new BlockStmt(),
                    new BlockStmt()));

            Assert.Equal(expected, ifStmt.Elem, SyntaxEqualityComparer.Instance);
        }

        [Fact]
        async Task TestParseIfStmtWithTestTypeAsync()
        {
            (var parser, var context) = await PrepareAsync("if (b is T) {} else if (c) {} else {}");

            var ifStmt = await parser.ParseIfStmtAsync(context);

            var expected = new IfStmt(new IdentifierExp("b"),
                new IdTypeExp("T"),
                new BlockStmt(),
                new IfStmt(
                    new IdentifierExp("c"),
                    null,
                    new BlockStmt(),
                    new BlockStmt()));

            Assert.Equal(expected, ifStmt.Elem, SyntaxEqualityComparer.Instance);
        }

        [Fact]
        async Task TestParseForStmtAsync()
        {
            (var parser, var context) = await PrepareAsync(@"
for (f(); g; h + g) ;
");

            var result = await parser.ParseForStmtAsync(context);

            var expected = new ForStmt(
                new ExpForStmtInitializer(new CallExp(new IdentifierExp("f"))),
                new IdentifierExp("g"),
                new BinaryOpExp(BinaryOpKind.Add, new IdentifierExp("h"), new IdentifierExp("g")),
                BlankStmt.Instance);

            Assert.Equal(expected, result.Elem, SyntaxEqualityComparer.Instance);
        }

        [Fact]
        async Task TestParseContinueStmtAsync()
        {
            (var parser, var context) = await PrepareAsync(@"continue;");
            var continueResult = await parser.ParseContinueStmtAsync(context);

            Assert.Equal(ContinueStmt.Instance, continueResult.Elem, SyntaxEqualityComparer.Instance);
        }

        [Fact]
        async Task TestParseBreakStmtAsync()
        {
            (var parser, var context) = await PrepareAsync(@"break;");
            var breakResult = await parser.ParseBreakStmtAsync(context);

            Assert.Equal(BreakStmt.Instance, breakResult.Elem, SyntaxEqualityComparer.Instance);
        }

        [Fact]
        async Task TestParseBlockStmtAsync()
        {
            (var parser, var context) = await PrepareAsync(@"{ { } { ; } ; }");
            var blockResult = await parser.ParseBlockStmtAsync(context);

            var expected = new BlockStmt(
                new BlockStmt(),
                new BlockStmt(BlankStmt.Instance),
                BlankStmt.Instance);

            Assert.Equal(expected, blockResult.Elem, SyntaxEqualityComparer.Instance);
        }

        [Fact]
        async Task TestParseBlankStmtAsync()
        {
            (var parser, var context) = await PrepareAsync("  ;  ");
            var blankResult = await parser.ParseBlankStmtAsync(context);

            Assert.Equal(BlankStmt.Instance, blankResult.Elem, SyntaxEqualityComparer.Instance);
        }

        [Fact]
        async Task TestParseExpStmtAsync()
        {
            (var parser, var context) = await PrepareAsync("a = b * c(1);");
            var expResult = await parser.ParseExpStmtAsync(context);

            var expected = new ExpStmt(new BinaryOpExp(BinaryOpKind.Assign,
                new IdentifierExp("a"),
                new BinaryOpExp(BinaryOpKind.Multiply,
                    new IdentifierExp("b"),
                    new CallExp(new IdentifierExp("c"), new IntLiteralExp(1)))));
                

            Assert.Equal(expected, expResult.Elem, SyntaxEqualityComparer.Instance);
        }

        [Fact]
        async Task TestParseForeachStmtAsync()
        {
            var (parser, context) = await PrepareAsync("foreach( var x in l ) { } ");
            var stmtResult = await parser.ParseForeachStmtAsync(context);

            var expected = new ForeachStmt(new IdTypeExp("var"), "x", new IdentifierExp("l"), new BlockStmt());

            Assert.Equal(expected, stmtResult.Elem, SyntaxEqualityComparer.Instance);
        }
    }
}
