using Gum.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
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

            var expected = new CommandStmt(
                new StringExp(
                    new TextStringExpElement("    echo "),
                    new ExpStringExpElement(new IdentifierExp("a")),
                    new TextStringExpElement(" bbb   ")),
                new StringExp(new TextStringExpElement("xxx")));

            Assert.Equal(expected, cmdStmt.Elem);
        }

        [Fact]
        async Task TestParseVarDeclStmtAsync()
        {
            (var parser, var context) = await PrepareAsync("string a = \"hello\";");
            
            var varDeclStmt = await parser.ParseVarDeclStmtAsync(context);

            var expected = new VarDeclStmt(new VarDecl(new IdTypeExp("string"),
                new VarDeclElement("a", new StringExp(new TextStringExpElement("hello")))));

            Assert.Equal(expected, varDeclStmt.Elem);
        }
        
        [Fact]
        async Task TestParseIfStmtAsync()
        {
            (var parser, var context) = await PrepareAsync("if (b) {} else if (c) {} else {}");
            
            var ifStmt = await parser.ParseIfStmtAsync(context);

            var expected = new IfStmt(new IdentifierExp("b"),
                null,
                new BlockStmt(ImmutableArray<Stmt>.Empty),                
                new IfStmt(
                    new IdentifierExp("c"),
                    null,
                    new BlockStmt(ImmutableArray<Stmt>.Empty),
                    new BlockStmt(ImmutableArray<Stmt>.Empty)));

            Assert.Equal(expected, ifStmt.Elem);
        }

        [Fact]
        async Task TestParseIfStmtWithTestTypeAsync()
        {
            (var parser, var context) = await PrepareAsync("if (b is T) {} else if (c) {} else {}");

            var ifStmt = await parser.ParseIfStmtAsync(context);

            var expected = new IfStmt(new IdentifierExp("b"),
                new IdTypeExp("T"),
                new BlockStmt(ImmutableArray<Stmt>.Empty),
                new IfStmt(
                    new IdentifierExp("c"),
                    null,
                    new BlockStmt(ImmutableArray<Stmt>.Empty),
                    new BlockStmt(ImmutableArray<Stmt>.Empty)));

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
                new ExpForStmtInitializer(new CallExp(new IdentifierExp("f"), ImmutableArray<TypeExp>.Empty)),
                new IdentifierExp("g"),
                new BinaryOpExp(BinaryOpKind.Add, new IdentifierExp("h"), new IdentifierExp("g")),
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

            var expected = new BlockStmt(
                new BlockStmt(),
                new BlockStmt(BlankStmt.Instance),
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
                new IdentifierExp("a"),
                new BinaryOpExp(BinaryOpKind.Multiply,
                    new IdentifierExp("b"),
                    new CallExp(new IdentifierExp("c"), ImmutableArray<TypeExp>.Empty, new IntLiteralExp(1)))));
                

            Assert.Equal(expected, expResult.Elem);
        }

        [Fact]
        async Task TestParseForeachStmtAsync()
        {
            var (parser, context) = await PrepareAsync("foreach( var x in l ) { } ");
            var stmtResult = await parser.ParseForeachStmtAsync(context);

            var expected = new ForeachStmt(new IdTypeExp("var"), "x", new IdentifierExp("l"), new BlockStmt());

            Assert.Equal(expected, stmtResult.Elem);
        }
    }
}
