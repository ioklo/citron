using Citron.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

using static Citron.Infra.Misc;
using static Citron.Syntax.SyntaxFactory;

namespace Citron.TextAnalysis.Test
{
    public class StmtParserTests
    {
        (Lexer, ParserContext) Prepare(string input)
        {
            var lexer = new Lexer();
            var buffer = new Buffer(new StringReader(input));
            var bufferPos = buffer.MakePosition().Next();
            var lexerContext = LexerContext.Make(bufferPos);
            var context = ParserContext.Make(lexerContext);

            return (lexer, context);
        }

        [Fact]
        public void TestParseInlineCommandStmtAsync()
        {
            var (lexer, context) = Prepare("@echo ${a}bbb  ");
            
            StmtParser.Parse(lexer, ref context, out var cmdStmt);

            var expected = new CommandStmt(Arr(
                new StringExp(Arr<StringExpElement>(
                    new TextStringExpElement("echo "),
                    new ExpStringExpElement(SId("a")),
                    new TextStringExpElement("bbb  ")
                ))
            ));

            Assert.Equal(expected, cmdStmt);
        }

        [Fact]
        public void TestParseBlockCommandStmtAsync()
        {
            var input = @"
@{ 
    echo ${ a } bbb   
xxx
}
";          
            var (lexer, context) = Prepare(input);

            StmtParser.Parse(lexer, ref context, out var stmt);

            var expected = new CommandStmt(Arr(
                new StringExp(Arr<StringExpElement>(
                    new TextStringExpElement("    echo "),
                    new ExpStringExpElement(SId("a")),
                    new TextStringExpElement(" bbb   ")
                )),
                SString("xxx")
            ));

            Assert.Equal(expected, stmt);
        }

        [Fact]
        public void TestParseVarDeclStmtAsync()
        {
            var (lexer, context) = Prepare("string a = \"hello\";");
            
            StmtParser.Parse(lexer, ref context, out var stmt);

            var expected = SVarDeclStmt(SIdTypeExp("string"), "a", SString("hello"));

            Assert.Equal(expected, stmt);
        }

        [Fact]
        public void TestParseLocalPtrVarDeclStmtAsync()
        {
            // int* p;
            var (lexer, context) = Prepare("int* p;");

            StmtParser.Parse(lexer, ref context, out var stmt);

            var expected = new VarDeclStmt(new VarDecl(SLocalPtrTypeExp(SIdTypeExp("int")), Arr(new VarDeclElement("p", null))));

            Assert.Equal(expected, stmt);
        }

        [Fact]
        public void TestParseBoxPtrVarDeclStmtAsync()
        {
            // box int* p;
            var (lexer, context) = Prepare("box int* p;");

            StmtParser.Parse(lexer, ref context, out var stmt);

            var expected = new VarDeclStmt(new VarDecl(SBoxPtrTypeExp(SIdTypeExp("int")), Arr(new VarDeclElement("p", null))));

            Assert.Equal(expected, stmt);
        }

        [Fact]
        public void TestParseNullableVarDeclStmtAsync()
        {
            // int? p;
            var (lexer, context) = Prepare("int? p;");

            StmtParser.Parse(lexer, ref context, out var stmt);

            var expected = new VarDeclStmt(new VarDecl(SNullableTypeExp(SIdTypeExp("int")), Arr(new VarDeclElement("p", null))));

            Assert.Equal(expected, stmt);
        }

        [Fact]
        public void TestParseIfStmtAsync()
        {
            var (lexer, context) = Prepare("if (b) {} else if (c) {} else {}");
            
            StmtParser.Parse(lexer, ref context, out var stmt);

            var expected = new IfStmt(SId("b"),
                new EmbeddableStmt.Multiple(default),
                new EmbeddableStmt.Single(new IfStmt(
                    SId("c"),
                    new EmbeddableStmt.Multiple(default),
                    new EmbeddableStmt.Multiple(default))));

            Assert.Equal(expected, stmt);
        }

        [Fact]
        public void TestParseIfTestStmtWithoutVarNameAsync()
        {
            var (lexer, context) = Prepare("if (b is T) {} else if (c) {} else {}");

            StmtParser.Parse(lexer, ref context, out var stmt);

            var expected = new IfTestStmt(SId("b"),
                SIdTypeExp("T"), 
                null,
                new EmbeddableStmt.Multiple(default),
                new EmbeddableStmt.Single(new IfStmt(
                    SId("c"),
                    new EmbeddableStmt.Multiple(default),
                    new EmbeddableStmt.Multiple(default)
                ))
            );

            Assert.Equal(expected, stmt);
        }

        [Fact]
        public void TestParseIfTestStmtWithVarNameAsync()
        {
            var (lexer, context) = Prepare("if (b is T t) {} else if (c) {} else {}");

            StmtParser.Parse(lexer, ref context, out var stmt);

            var expected = new IfTestStmt(SId("b"),
                SIdTypeExp("T"),
                "t",
                new EmbeddableStmt.Multiple(default),
                new EmbeddableStmt.Single(new IfStmt(
                    SId("c"),
                    new EmbeddableStmt.Multiple(default),
                    new EmbeddableStmt.Multiple(default))));

            Assert.Equal(expected, stmt);
        }

        [Fact]
        public void TestParseForStmtAsync()
        {
            var (lexer, context) = Prepare(@"
for (f(); g; h + g) ;
");

            StmtParser.Parse(lexer, ref context, out var stmt);

            var expected = new ForStmt(
                new ExpForStmtInitializer(new CallExp(SId("f"), default)),
                SId("g"),
                new BinaryOpExp(BinaryOpKind.Add, SId("h"), SId("g")),
                SEmbeddableBlankStmt());

            Assert.Equal(expected, stmt);
        }

        [Fact]
        public void TestParseContinueStmtAsync()
        {
            var (lexer, context) = Prepare(@"continue;");
            StmtParser.Parse(lexer, ref context, out var stmt);

            Assert.Equal(new ContinueStmt(), stmt);
        }

        [Fact]
        public void TestParseBreakStmtAsync()
        {
            var (lexer, context) = Prepare(@"break;");
            StmtParser.Parse(lexer, ref context, out var stmt);

            Assert.Equal(new BreakStmt(), stmt);
        }

        [Fact]
        public void TestParseBlockStmtAsync()
        {
            var (lexer, context) = Prepare(@"{ { } { ; } ; }");
            StmtParser.Parse(lexer, ref context, out var stmt);

            var expected = SBlock(
                SBlock(),
                SBlock(new BlankStmt()),
                new BlankStmt());

            Assert.Equal(expected, stmt);
        }

        [Fact]
        public void TestParseBlankStmtAsync()
        {
            var (lexer, context) = Prepare("  ;  ");
            StmtParser.Parse(lexer, ref context, out var stmt);

            Assert.Equal(new BlankStmt(), stmt);
        }

        [Fact]
        public void TestParseExpStmtAsync()
        {
            var (lexer, context) = Prepare("a = b * c(1);");
            StmtParser.Parse(lexer, ref context, out var stmt);

            var expected = new ExpStmt(new BinaryOpExp(BinaryOpKind.Assign,
                SId("a"),
                new BinaryOpExp(BinaryOpKind.Multiply,
                    SId("b"),
                    new CallExp(SId("c"), Arr<Argument>(new Argument.Normal(new IntLiteralExp(1)))))));
                

            Assert.Equal(expected, stmt);
        }

        [Fact]
        public void TestParseForeachStmtAsync()
        {
            var (lexer, context) = Prepare("foreach( var x in l ) { } ");
            StmtParser.Parse(lexer, ref context, out var stmt);

            var expected = new ForeachStmt(SIdTypeExp("var"), "x", SId("l"), new EmbeddableStmt.Multiple(default));

            Assert.Equal(expected, stmt);
        }

        [Fact]
        public void TestParseDirectiveStmtAsync()
        {
            var (lexer, context) = Prepare("`notnull(a);");
            StmtParser.Parse(lexer, ref context, out var stmt);

            var expected = new DirectiveStmt("notnull", Arr<Exp>(SId("a")));

            Assert.Equal(expected, stmt);
        }
    }
}
