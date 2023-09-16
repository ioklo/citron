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
using static Citron.TextAnalysisTestMisc;

namespace Citron;

public class ParserTests
{   
    [Fact]
    public void TestParseSimpleScriptAsync()
    {
        var lexer = new Lexer();            
        var context = MakeParserContext(
@"void Main() 
{ 
    @ls -al
}");
        ScriptParser.Parse(lexer, ref context, out var script);

        var expected = SScript(
            new CommandStmt(Arr(
                new StringExp(Arr<StringExpElement>(new TextStringExpElement("ls -al")))
            ))
        );

        Assert.Equal(expected, script);
    }

    [Fact]
    public void TestParseFuncDeclAsync()
    {
        var lexer = new Lexer();
        var context = MakeParserContext("void Func(int x, string y, params int z) { int a = 0; }");
        ScriptParser.Parse(lexer, ref context, out var script);

        var expected = SScript(new GlobalFuncDeclScriptElement(new GlobalFuncDecl(
            null,
            isSequence: false,
            SIdTypeExp("void"),
            "Func", default,
            Arr(
                new FuncParam(HasParams: false, SIdTypeExp("int"), "x"),
                new FuncParam(HasParams: false, SIdTypeExp("string"), "y"),
                new FuncParam(HasParams: true, SIdTypeExp("int"), "z")
            ), 
            Arr<Stmt>(
                new VarDeclStmt(
                    new VarDecl(
                        SIdTypeExp("int"), 
                        Arr(
                            new VarDeclElement("a", new IntLiteralExp(0))
                        )
                    )
                )
            )
        )));

        Assert.Equal(expected, script);
    }

    [Fact]
    public void TestParseNamespaceDeclAsync()
    {
        var lexer = new Lexer();
        var context = MakeParserContext(@"
namespace NS1
{
    namespace NS2.NS3
    {
        void F()
        {

        }
    }
}");
        ScriptParser.Parse(lexer, ref context, out var script);

        var expected = new Script(Arr<ScriptElement>(new NamespaceDeclScriptElement(new NamespaceDecl(
            Arr("NS1"),
            Arr<NamespaceElement>(new NamespaceDeclNamespaceElement(new NamespaceDecl(
                Arr("NS2", "NS3"),
                Arr<NamespaceElement>(new GlobalFuncDeclNamespaceElement(new GlobalFuncDecl(
                        null,
                        isSequence: false, 
                        SVoidTypeExp(),
                        "F",
                        typeParams: default,
                        parameters: default,
                        body: default
                )))
            )))
        ))));

        Assert.Equal(expected, script);
    }

    [Fact]
    public void TestParseEnumDeclAsync()
    {
        var lexer = new Lexer();
        var context = MakeParserContext(@"
enum X
{
    First,
    Second (int i),
    Third
}");
        ScriptParser.Parse(lexer, ref context, out var script);

        var expected = SScript(new TypeDeclScriptElement(new EnumDecl(null, "X",
            default,
            Arr(
                new EnumElemDecl("First", default),
                new EnumElemDecl("Second", Arr(new EnumElemMemberVarDecl(SIdTypeExp("int"), "i"))),
                new EnumElemDecl("Third", default)
            )
        )));

        Assert.Equal(expected, script);
    }
    

    [Fact]
    public void TestParseStructDeclAsync()
    {
        var lexer = new Lexer();
        var context = MakeParserContext(@"
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
        ScriptParser.Parse(lexer, ref context, out var script);

        var expected = SScript(new TypeDeclScriptElement(new StructDecl(AccessModifier.Public, "S",
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
                    SIdTypeExp("void"),
                    "Func",
                    Arr(new TypeParam("X")),
                    Arr(new FuncParam(HasParams: false, SIdTypeExp("string"), "s")),
                    Arr<Stmt>()
                ),

                new StructMemberFuncDecl(
                    AccessModifier.Private,
                    IsStatic: false,
                    IsSequence: true,
                    SIdTypeExp("int"),
                    "F2",
                    Arr(new TypeParam("T")),
                    Parameters: default,
                    Arr<Stmt>(new YieldStmt(new IntLiteralExp(4)))
                )
            )
        )));
        
        Assert.Equal(expected, script);
    }

    
    [Fact]
    public void TestParseComplexScriptAsync()
    {
        var lexer = new Lexer();
        var context = MakeParserContext(@"
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
        ScriptParser.Parse(lexer, ref context, out var script);

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
                
        Assert.Equal(expected, script);
    }
}
