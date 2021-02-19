using Gum.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

using static Gum.TextAnalysis.Test.TestMisc;

namespace Gum.TextAnalysis.Test
{
    public class ParserTests
    {
        ImmutableArray<T> Arr<T>(params T[] elems) => ImmutableArray.Create(elems);

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

            var expected = SimpleSScript(
                new Script.StmtElement(new CommandStmt(Arr(
                    new StringExp(Arr<StringExpElement>(new TextStringExpElement("ls -al")))
                )))
            );

            Assert.Equal(expected, script.Elem, SyntaxEqualityComparer.Instance);
        }

        [Fact]
        public async Task TestParseFuncDeclAsync()
        {
            var lexer = new Lexer();
            var parser = new Parser(lexer);
            var context = await MakeContextAsync("void Func(int x, params string y, int z) { int a = 0; }");
            var funcDecl = await parser.ParseGlobalFuncDeclAsync(context);

            var expected = new GlobalFuncDecl(
                false,
                SimpleSIdTypeExp("void"),
                "Func", default,
                new FuncParamInfo(
                    Arr(
                        new TypeAndName(SimpleSIdTypeExp("int"), "x"),
                        new TypeAndName(SimpleSIdTypeExp("string"), "y"),
                        new TypeAndName(SimpleSIdTypeExp("int"), "z")
                    ),
                    1
                ),
                SimpleSBlockStmt(new VarDeclStmt(new VarDecl(SimpleSIdTypeExp("int"), Arr(new VarDeclElement("a", new IntLiteralExp(0)))))));

            Assert.Equal(expected, funcDecl.Elem, SyntaxEqualityComparer.Instance);
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
                default,
                Arr(
                    new EnumDeclElement("First", default),
                    new EnumDeclElement("Second", Arr(new TypeAndName(SimpleSIdTypeExp("int"), "i"))),
                    new EnumDeclElement("Third", default)
                )
            );

            Assert.Equal(expected, enumDecl.Elem, SyntaxEqualityComparer.Instance);
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

    public struct Nested<U> : B, I { int x; }

    static void Func<X>(string s) { }
    private seq int F2<T>() { yield 4; }
}
");
            var structDecl = await parser.ParseStructDeclAsync(context);

            var expected = new StructDecl(AccessModifier.Public, "S",
                Arr( "T" ),

                Arr<TypeExp>( SimpleSIdTypeExp("B"), SimpleSIdTypeExp("I") ),

                Arr<StructDecl.Element>(
                    new StructDecl.VarDeclElement(AccessModifier.Public, SimpleSIdTypeExp("int"), Arr("x1", "x2")),
                    new StructDecl.VarDeclElement(AccessModifier.Protected, SimpleSIdTypeExp("string"), Arr("y")),
                    new StructDecl.VarDeclElement(AccessModifier.Private, SimpleSIdTypeExp("int"), Arr("z")),

                    new StructDecl.TypeDeclElement(new StructDecl(
                        AccessModifier.Public, "Nested", Arr( "U" ), Arr<TypeExp>( SimpleSIdTypeExp("B"), SimpleSIdTypeExp("I")),
                        Arr<StructDecl.Element>(new StructDecl.VarDeclElement(AccessModifier.Public, SimpleSIdTypeExp("int"), Arr("x")))
                    )),

                    new StructDecl.FuncDeclElement(new StructFuncDecl(
                        AccessModifier.Public,
                        bStatic: true,
                        bSequence: false,
                        SimpleSIdTypeExp("void"),
                        "Func",
                        Arr("X"),
                        new FuncParamInfo(Arr(new TypeAndName(SimpleSIdTypeExp("string"), "s")), null),
                        SimpleSBlockStmt()
                    )),

                    new StructDecl.FuncDeclElement(new StructFuncDecl(
                        AccessModifier.Private,
                        bStatic: false,
                        bSequence: true,
                        SimpleSIdTypeExp("int"),
                        "F2",
                        Arr("T"),
                        new FuncParamInfo(default, null),
                        SimpleSBlockStmt(new YieldStmt(new IntLiteralExp(4)))
                    ))
                )
            );

            Assert.Equal(expected, structDecl.Elem, SyntaxEqualityComparer.Instance);

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

            var expected = SimpleSScript(
                new Script.StmtElement(SimpleSVarDeclStmt(SimpleSIdTypeExp("int"), new VarDeclElement("sum", new IntLiteralExp(0)))),
                new Script.StmtElement(new ForStmt(
                    new VarDeclForStmtInitializer(SimpleSVarDecl(SimpleSIdTypeExp("int"), new VarDeclElement("i", new IntLiteralExp(0)))),
                    new BinaryOpExp(BinaryOpKind.LessThan, SimpleSId("i"), new IntLiteralExp(5)),
                    new UnaryOpExp(UnaryOpKind.PostfixInc, SimpleSId("i")),
                    SimpleSBlockStmt(
                        new IfStmt(
                                new BinaryOpExp(BinaryOpKind.Equal,
                                    new BinaryOpExp(BinaryOpKind.Modulo, SimpleSId("i"), new IntLiteralExp(2)),
                                    new IntLiteralExp(0)),
                                null,
                                new ExpStmt(
                                    new BinaryOpExp(BinaryOpKind.Assign,
                                        SimpleSId("sum"),
                                        new BinaryOpExp(BinaryOpKind.Add, SimpleSId("sum"), SimpleSId("i")))),
                                new CommandStmt(Arr(SimpleSStringExp("        echo hi "))))))),
                new Script.StmtElement(new CommandStmt(Arr(new StringExp(Arr<StringExpElement>(
                    new TextStringExpElement("echo "),
                    new ExpStringExpElement(SimpleSId("sum")),
                    new TextStringExpElement(" Completed!")))))));
                    
            Assert.Equal(expected, script.Elem, SyntaxEqualityComparer.Instance);
        }
    }
}
