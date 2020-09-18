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
    public class ParserTests
    {
        async ValueTask<ParserContext> MakeContextAsync(string input)
        {
            var buffer = new Buffer(new StringReader(input));
            var bufferPos = await buffer.MakePosition().NextAsync();
            var lexerContext = LexerContext.Make(bufferPos);
            return ParserContext.Make(lexerContext);
        }

        [Fact]
        public async Task TestParseSimpleScriptAsync()
        {
            var lexer = new Lexer();
            var parser = new Parser(lexer);
            var context = await MakeContextAsync("@ls -al");
            var script = await parser.ParseScriptAsync(context);

            var expected = new Script(new Script.StmtElement(new CommandStmt(new StringExp(new TextStringExpElement("ls -al")))));

            Assert.Equal(expected, script.Elem);
        }

        [Fact]
        public async Task TestParseFuncDeclAsync()
        {
            var lexer = new Lexer();
            var parser = new Parser(lexer);
            var context = await MakeContextAsync("void Func(int x, params string y, int z) { int a = 0; }");
            var funcDecl = await parser.ParseFuncDeclAsync(context);

            var expected = new FuncDecl(
                false,
                new IdTypeExp("void"),
                "Func", Enumerable.Empty<string>(),
                new FuncParamInfo(
                    new TypeAndName[] {
                        new TypeAndName(new IdTypeExp("int"), "x"),
                        new TypeAndName(new IdTypeExp("string"), "y"),
                        new TypeAndName(new IdTypeExp("int"), "z") },
                    1),
                new BlockStmt(new VarDeclStmt(new VarDecl(new IdTypeExp("int"), new VarDeclElement("a", new IntLiteralExp(0))))));

            Assert.Equal(expected, funcDecl.Elem);
        }

        [Fact]
        public async Task TestParseEnumDeclAsync()
        {
            var lexer = new Lexer();
            var parser = new Parser(lexer);
            var context = await MakeContextAsync(@"
enum X
{
    First,
    Second (int i),
    Third
}");
            var enumDecl = await parser.ParseEnumDeclAsync(context);

            var expected = new EnumDecl("X",
                Enumerable.Empty<string>(),
                new EnumDeclElement("First"),
                new EnumDeclElement("Second", new TypeAndName(new IdTypeExp("int"), "i")),
                new EnumDeclElement("Third"));

            Assert.Equal(expected, enumDecl.Elem);
        }

        [Fact]
        public async Task TestParseStructDeclAsync()
        {
            var lexer = new Lexer();
            var parser = new Parser(lexer);
            var context = await MakeContextAsync(@"
public struct S<T> : B, I
{
    int x1, x2;
    protected string y;
    private int z;

    static void Func<X>(string s) { }
    private seq int F2<T>() { yield 4; }
}
");
            var structDecl = await parser.ParseStructDeclAsync(context);

            var expected = new StructDecl(AccessModifier.Public, "S",
                new string[] { "T" },

                new TypeExp[] { new IdTypeExp("B"), new IdTypeExp("I") },

                new StructDecl.Element[]
                {
                    StructDecl.MakeVarDeclElement(AccessModifier.Public, new IdTypeExp("int"), new[] { "x1", "x2" }),
                    StructDecl.MakeVarDeclElement(AccessModifier.Protected, new IdTypeExp("string"), new[] { "y" }),
                    StructDecl.MakeVarDeclElement(AccessModifier.Private, new IdTypeExp("int"), new[] { "z" }),

                    StructDecl.MakeFuncDeclElement(
                        AccessModifier.Public,
                        bStatic: true,
                        bSequence: false,
                        new IdTypeExp("void"),
                        "Func",
                        new string[] { "X" },
                        new FuncParamInfo(new TypeAndName[] { new TypeAndName(new IdTypeExp("string"), "s") }, null),
                        new BlockStmt()),

                    StructDecl.MakeFuncDeclElement(
                        AccessModifier.Private,
                        bStatic: false,
                        bSequence: true,
                        new IdTypeExp("int"),
                        "F2",
                        new string[] { "T" },
                        new FuncParamInfo(Enumerable.Empty<TypeAndName>(), null),
                        new BlockStmt(new YieldStmt(new IntLiteralExp(4))))
                });

            Assert.Equal(expected, structDecl.Elem);

        }

        [Fact]
        public async Task TestParseComplexScriptAsync()
        {
            var lexer = new Lexer();
            var parser = new Parser(lexer);
            var context = await MakeContextAsync(@"
int sum = 0;

for (int i = 0; i < 5; i++)
{
    if (i % 2 == 0)
        sum = sum + i;
    else @{ 
        echo hi 
    }
}

@echo $sum Completed!

");
            var script = await parser.ParseScriptAsync(context);

            var expected = new Script(
                new Script.StmtElement(new VarDeclStmt(new VarDecl(new IdTypeExp("int"), new VarDeclElement("sum", new IntLiteralExp(0))))),
                new Script.StmtElement(new ForStmt(
                    new VarDeclForStmtInitializer(new VarDecl(new IdTypeExp("int"), new VarDeclElement("i", new IntLiteralExp(0)))),
                    new BinaryOpExp(BinaryOpKind.LessThan, new IdentifierExp("i"), new IntLiteralExp(5)),
                    new UnaryOpExp(UnaryOpKind.PostfixInc, new IdentifierExp("i")),
                    new BlockStmt(
                        new IfStmt(
                                new BinaryOpExp(BinaryOpKind.Equal,
                                    new BinaryOpExp(BinaryOpKind.Modulo, new IdentifierExp("i"), new IntLiteralExp(2)),
                                    new IntLiteralExp(0)),
                                null,
                                new ExpStmt(
                                    new BinaryOpExp(BinaryOpKind.Assign,
                                        new IdentifierExp("sum"),
                                        new BinaryOpExp(BinaryOpKind.Add, new IdentifierExp("sum"), new IdentifierExp("i")))),
                                new CommandStmt(new StringExp(new TextStringExpElement("        echo hi "))))))),
                new Script.StmtElement(new CommandStmt(new StringExp(
                    new TextStringExpElement("echo "),
                    new ExpStringExpElement(new IdentifierExp("sum")),
                    new TextStringExpElement(" Completed!")))));
                    
            Assert.Equal(expected, script.Elem);
        }
    }
}
