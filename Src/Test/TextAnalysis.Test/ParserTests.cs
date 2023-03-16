using Citron.Syntax;
using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

using static Citron.Infra.Misc;
using static Citron.Syntax.SyntaxFactory;

namespace Citron.TextAnalysis.Test
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
            var context = await MakeContextAsync(
@"void Main() 
{ 
    @ls -al
}");
            var script = await parser.ParseScriptAsync(context);

            var expected = SScript(
                new CommandStmt(Arr(
                    new StringExp(Arr<StringExpElement>(new TextStringExpElement("ls -al")))
                ))
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
                null,
                false,
                false,
                SIdTypeExp("void"),
                "Func", default,
                Arr(
                    new FuncParam(FuncParamKind.Normal, SIdTypeExp("int"), "x"),
                    new FuncParam(FuncParamKind.Params, SIdTypeExp("string"), "y"),
                    new FuncParam(FuncParamKind.Normal, SIdTypeExp("int"), "z")
                ),
                Arr<Stmt>(
                    new VarDeclStmt(
                        new VarDecl(false, 
                            SIdTypeExp("int"), 
                            Arr(
                                new VarDeclElement("a", new VarDeclElemInitializer(false, new IntLiteralExp(0)))
                            )
                        )
                    )
                )
            );

            Assert.Equal(expected, funcDecl.Elem);
        }

        [Fact]
        public async Task TestParseNamespaceDeclAsync()
        {
            var lexer = new Lexer();
            var parser = new Parser(lexer);
            var context = await MakeContextAsync(@"
namespace NS1
{
    namespace NS2.NS3
    {
        void F()
        {

        }
    }
}");
            var script = await parser.ParseScriptAsync(context);

            var expected = new Script(Arr<ScriptElement>(new NamespaceDeclScriptElement(new NamespaceDecl(
                Arr("NS1"),
                Arr<NamespaceElement>(new NamespaceDeclNamespaceElement(new NamespaceDecl(
                    Arr("NS2", "NS3"),
                    Arr<NamespaceElement>(new GlobalFuncDeclNamespaceElement(new GlobalFuncDecl(
                            null,
                            isSequence: false, 
                            isRefReturn: false,
                            SVoidTypeExp(),
                            "F",
                            typeParams: default,
                            parameters: default,
                            body: default
                    )))
                )))
            ))));

            Assert.Equal(expected, script.Elem);
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

            var expected = new EnumDecl(null, "X",
                default,
                Arr(
                    new EnumElemDecl("First", default),
                    new EnumElemDecl("Second", Arr(new EnumElemMemberVarDecl(SIdTypeExp("int"), "i"))),
                    new EnumElemDecl("Third", default)
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
    int x1;
    public int x2;
    protected string y;
    private int z;

    public struct Nested<U> : B, I { int x; }

    static void Func<X>(string s) { }
    private seq int F2<T>() { yield 4; }
}
");
            var structDecl = await parser.ParseStructDeclAsync(context);

            var expected = new StructDecl(AccessModifier.Public, "S",
                Arr(new TypeParam("T")),

                Arr<TypeExp>( SIdTypeExp("B"), SIdTypeExp("I") ),

                Arr<StructMemberDecl>(
                    new StructMemberVarDecl(null, SIdTypeExp("int"), Arr("x1")),
                    new StructMemberVarDecl(AccessModifier.Public, SIdTypeExp("int"), Arr("x2")),
                    new StructMemberVarDecl(AccessModifier.Protected, SIdTypeExp("string"), Arr("y")),
                    new StructMemberVarDecl(AccessModifier.Private, SIdTypeExp("int"), Arr("z")),

                    new StructMemberTypeDecl(new StructDecl(
                        AccessModifier.Public, "Nested", Arr(new TypeParam("U")), Arr<TypeExp>( SIdTypeExp("B"), SIdTypeExp("I")),
                        Arr<StructMemberDecl>(new StructMemberVarDecl(null, SIdTypeExp("int"), Arr("x")))
                    )),

                    new StructMemberFuncDecl(
                        null,
                        IsStatic: true,
                        IsSequence: false,
                        IsRefReturn: false,
                        SIdTypeExp("void"),
                        "Func",
                        Arr(new TypeParam("X")),
                        Arr(new FuncParam(FuncParamKind.Normal, SIdTypeExp("string"), "s")),
                        Arr<Stmt>()
                    ),

                    new StructMemberFuncDecl(
                        AccessModifier.Private,
                        IsStatic: false,
                        IsSequence: true,
                        IsRefReturn: false,
                        SIdTypeExp("int"),
                        "F2",
                        Arr(new TypeParam("T")),
                        default,
                        Arr<Stmt>(new YieldStmt(new IntLiteralExp(4)))
                    )
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
void Main()
{
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
}

");
            var script = await parser.ParseScriptAsync(context);

            var expected = SScript(
                SVarDeclStmt(SIdTypeExp("int"), "sum", new IntLiteralExp(0)),
                new ForStmt(
                    new VarDeclForStmtInitializer(SVarDecl(SIdTypeExp("int"), "i", new IntLiteralExp(0))),
                    new BinaryOpExp(BinaryOpKind.LessThan, SId("i"), new IntLiteralExp(5)),
                    new UnaryOpExp(UnaryOpKind.PostfixInc, SId("i")),
                    new EmbeddableStmt.Multiple(Arr<Stmt>(
                        new IfStmt(
                            new BinaryOpExp(BinaryOpKind.Equal,
                                new BinaryOpExp(BinaryOpKind.Modulo, SId("i"), new IntLiteralExp(2)),
                                new IntLiteralExp(0)),
                            new EmbeddableStmt.Single(new ExpStmt(
                                new BinaryOpExp(BinaryOpKind.Assign,
                                    SId("sum"),
                                    new BinaryOpExp(BinaryOpKind.Add, SId("sum"), SId("i"))))),
                            new EmbeddableStmt.Single(new CommandStmt(Arr(SString("            echo hi "))))
                        )
                    ))
                ),
                new CommandStmt(Arr(new StringExp(Arr<StringExpElement>(
                    new TextStringExpElement("echo "),
                    new ExpStringExpElement(SId("sum")),
                    new TextStringExpElement(" Completed!"))))));
                    
            Assert.Equal(expected, script.Elem);
        }
    }
}
