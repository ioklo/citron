using Gum.Syntax;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

using static Gum.Infra.Misc;
using static Gum.TextAnalysis.Test.TestMisc;

namespace Gum.TextAnalysis.Test
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

            var expected = SimpleSScript(
                new StmtScriptElement(new CommandStmt(Arr(
                    new StringExp(Arr<StringExpElement>(new TextStringExpElement("ls -al")))
                )))
            );

            Assert.Equal(expected, script.Elem);
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
                false,
                SimpleSIdTypeExp("void"),
                "Func", default,
                Arr(
                    new FuncParam(FuncParamKind.Normal, SimpleSIdTypeExp("int"), "x"),
                    new FuncParam(FuncParamKind.Params, SimpleSIdTypeExp("string"), "y"),
                    new FuncParam(FuncParamKind.Normal, SimpleSIdTypeExp("int"), "z")
                ),
                SimpleSBlockStmt(new VarDeclStmt(new VarDecl(false, SimpleSIdTypeExp("int"), Arr(new VarDeclElement("a", new IntLiteralExp(0)))))));

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
                default,
                Arr(
                    new EnumDeclElement("First", default),
                    new EnumDeclElement("Second", Arr(new EnumElementField(SimpleSIdTypeExp("int"), "i"))),
                    new EnumDeclElement("Third", default)
                )
            );

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

    public struct Nested<U> : B, I { int x; }

    static void Func<X>(string s) { }
    private seq int F2<T>() { yield 4; }
}
");
            var structDecl = await parser.ParseStructDeclAsync(context);

            var expected = new StructDecl(AccessModifier.Public, "S",
                Arr( "T" ),

                Arr<TypeExp>( SimpleSIdTypeExp("B"), SimpleSIdTypeExp("I") ),

                Arr<StructDeclElement>(
                    new VarStructDeclElement(AccessModifier.Public, SimpleSIdTypeExp("int"), Arr("x1", "x2")),
                    new VarStructDeclElement(AccessModifier.Protected, SimpleSIdTypeExp("string"), Arr("y")),
                    new VarStructDeclElement(AccessModifier.Private, SimpleSIdTypeExp("int"), Arr("z")),

                    new TypeStructDeclElement(new StructDecl(
                        AccessModifier.Public, "Nested", Arr( "U" ), Arr<TypeExp>( SimpleSIdTypeExp("B"), SimpleSIdTypeExp("I")),
                        Arr<StructDeclElement>(new VarStructDeclElement(AccessModifier.Public, SimpleSIdTypeExp("int"), Arr("x")))
                    )),

                    new FuncStructDeclElement(new StructFuncDecl(
                        AccessModifier.Public,
                        isStatic: true,
                        isSequence: false,
                        isRefReturn: false,
                        SimpleSIdTypeExp("void"),
                        "Func",
                        Arr("X"),
                        Arr(new FuncParam(FuncParamKind.Normal, SimpleSIdTypeExp("string"), "s")),
                        SimpleSBlockStmt()
                    )),

                    new FuncStructDeclElement(new StructFuncDecl(
                        AccessModifier.Private,
                        isStatic: false,
                        isSequence: true,
                        isRefReturn: false,
                        SimpleSIdTypeExp("int"),
                        "F2",
                        Arr("T"),
                        default,
                        SimpleSBlockStmt(new YieldStmt(new IntLiteralExp(4)))
                    ))
                )
            );

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

            var expected = SimpleSScript(
                new StmtScriptElement(SimpleSVarDeclStmt(SimpleSIdTypeExp("int"), new VarDeclElement("sum", new IntLiteralExp(0)))),
                new StmtScriptElement(new ForStmt(
                    new VarDeclForStmtInitializer(SimpleSVarDecl(SimpleSIdTypeExp("int"), new VarDeclElement("i", new IntLiteralExp(0)))),
                    new BinaryOpExp(BinaryOpKind.LessThan, SimpleSId("i"), new IntLiteralExp(5)),
                    new UnaryOpExp(UnaryOpKind.PostfixInc, SimpleSId("i")),
                    SimpleSBlockStmt(
                        new IfStmt(
                                new BinaryOpExp(BinaryOpKind.Equal,
                                    new BinaryOpExp(BinaryOpKind.Modulo, SimpleSId("i"), new IntLiteralExp(2)),
                                    new IntLiteralExp(0)),
                                new ExpStmt(
                                    new BinaryOpExp(BinaryOpKind.Assign,
                                        SimpleSId("sum"),
                                        new BinaryOpExp(BinaryOpKind.Add, SimpleSId("sum"), SimpleSId("i")))),
                                new CommandStmt(Arr(SimpleSStringExp("        echo hi "))))))),
                new StmtScriptElement(new CommandStmt(Arr(new StringExp(Arr<StringExpElement>(
                    new TextStringExpElement("echo "),
                    new ExpStringExpElement(SimpleSId("sum")),
                    new TextStringExpElement(" Completed!")))))));
                    
            Assert.Equal(expected, script.Elem);
        }
    }
}
