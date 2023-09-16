namespace Citron;

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

public class ExpParserTests
{
    (Lexer, ParserContext) Prepare(string input)
    {
        var lexer = new Lexer();
        var buffer = new Buffer(new StringReader(input));
        var bufferPos = buffer.MakePosition().Next();
        var lexerContext = LexerContext.Make(bufferPos);
        var parserContext = ParserContext.Make(lexerContext);

        return (lexer, parserContext);
    }

    [Fact]
    public void TestParseIdentifierExp()
    {
        var (lexer, context) = Prepare("x");

        ExpParser.Parse(lexer, ref context, out var exp);
        Assert.Equal(SId("x"), exp);
    }

    [Fact]
    public void TestParseIdentifierExpWithTypeArgs()
    {
        // x<T> vs (x < T) > 
        var (lexer, context) = Prepare("x<T>");
        ExpParser.Parse(lexer, ref context, out var exp);

        Assert.Equal(new IdentifierExp("x", Arr<TypeExp>(new IdTypeExp("T", default))), exp);
    }

    [Fact]
    public void TestParseStringExp()
    {
        var input = "\"aaa bbb ${\"xxx ${ddd}\"} ddd\"";
        var (lexer, context) = Prepare(input);

        ExpParser.Parse(lexer, ref context, out var exp);

        var expected = new StringExp(Arr<StringExpElement>(
            new TextStringExpElement("aaa bbb "),
            new ExpStringExpElement(new StringExp(Arr<StringExpElement>(
                new TextStringExpElement("xxx "),
                new ExpStringExpElement(SId("ddd"))
            ))),
            new TextStringExpElement(" ddd")
        ));

        Assert.Equal(expected, exp);
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    public void TestParseBool(string input, bool bExpectedResult)
    {   
        var (lexer, context) = Prepare(input); 
            
        ExpParser.Parse(lexer, ref context, out var exp);

        var expected = new BoolLiteralExp(bExpectedResult);

        Assert.Equal(expected, exp);
    }

    [Fact]
    public void TestParseInt()
    {
        var input = "1234";

        var (lexer, context) = Prepare(input);

        ExpParser.Parse(lexer, ref context, out var exp);

        var expected = new IntLiteralExp(1234);

        Assert.Equal(expected, exp);
    }

    [Fact]
    public void TestParseTestAndTypeTestExp() // left associative
    {
        var input = "e + 1 is X<int> < d + 1 is T"; // (((e + 1) is X<int>) < (d + 1)) is T
        var (lexer, context) = Prepare(input);

        ExpParser.Parse(lexer, ref context, out var exp);

        var expected = new IsExp(
            new BinaryOpExp(
                BinaryOpKind.LessThan,
                new IsExp(
                    new BinaryOpExp(
                        BinaryOpKind.Add,
                        new IdentifierExp("e", default),
                        new IntLiteralExp(1)
                    ),
                    SIdTypeExp("X", SIdTypeExp("int"))
                ),
                new BinaryOpExp(
                    BinaryOpKind.Add,
                    new IdentifierExp("d", default),
                    new IntLiteralExp(1)
                )
            ),
            SIdTypeExp("T")
        );

        Assert.Equal(expected, exp);
    }

    [Fact]
    public void TestParsePrimaryExp()
    {
        var input = "(c++(e, f) % d)++";
        var (lexer, context) = Prepare(input);

        ExpParser.Parse(lexer, ref context, out var exp);

        var expected = new UnaryOpExp(
            UnaryOpKind.PostfixInc,
            new BinaryOpExp(
                BinaryOpKind.Modulo,
                new CallExp(
                    new UnaryOpExp(UnaryOpKind.PostfixInc, SId("c")), 
                    Arr<Argument>(new Argument.Normal(SId("e")), new Argument.Normal(SId("f")))
                ),
                SId("d")
            )
        );

        Assert.Equal(expected, exp);
    }        

    [Fact]
    public void TestParseLambdaExp()
    {
        var input = "a = b => (c, int d) => e";
        var (lexer, context) = Prepare(input);

        ExpParser.Parse(lexer, ref context, out var exp);

        var expected = new BinaryOpExp(BinaryOpKind.Assign,
            SId("a"),
            new LambdaExp(
                Arr(new LambdaExpParam(null, "b")),
                Arr<Stmt>(new ReturnStmt(
                    new ReturnValueInfo(
                        new LambdaExp(
                            Arr(
                                new LambdaExpParam(null, "c"),
                                new LambdaExpParam(new IdTypeExp("int", default), "d")
                            ),
                            Arr<Stmt>(new ReturnStmt(new ReturnValueInfo(SId("e"))))
                        )
                    )
                ))
            )
        );

        Assert.Equal(expected, exp);
    }

    [Fact]
    public void TestParseComplexMemberExp()
    {
        var input = "a.b.c<int, list<int>>(1, \"str\").d";
        var (lexer, context) = Prepare(input);

        ExpParser.Parse(lexer, ref context, out var exp);

        var expected =
            new MemberExp(
                new CallExp(
                    new MemberExp(
                        new MemberExp(SId("a"), "b", default),
                        "c",
                        Arr<TypeExp>(
                            new IdTypeExp("int", default), 
                            new IdTypeExp("list", Arr<TypeExp>(new IdTypeExp("int", default)))
                        )
                    ),
                    Arr<Argument>(new Argument.Normal(new IntLiteralExp(1)), new Argument.Normal(SString("str")))
                ),
                "d",
                default
            );

        Assert.Equal(expected, exp);
    }

    [Fact]
    public void TestParseListExp()
    {
        var input = "[ 1, 2, 3 ]";
        var (lexer, context) = Prepare(input);

        ExpParser.Parse(lexer, ref context, out var exp);

        var expected = new ListExp(Arr<Exp>(                
            new IntLiteralExp(1),
            new IntLiteralExp(2),
            new IntLiteralExp(3)
        ));
                
        Assert.Equal(expected, exp);
    }

    [Fact]
    public void TestParseNewExp()
    {
        var input = "new MyType<X>(2, false, \"string\")";
        var (lexer, context) = Prepare(input);

        ExpParser.Parse(lexer, ref context, out var exp);

        var expected = new NewExp(
            new IdTypeExp("MyType", Arr<TypeExp>(new IdTypeExp("X", default))),
            Arr<Argument>(
                new Argument.Normal(new IntLiteralExp(2)),
                new Argument.Normal(new BoolLiteralExp(false)),
                new Argument.Normal(SString("string"))
            ));

        Assert.Equal(expected, exp);
    }
        
    [Fact]
    public void TestParseComplexExp()
    {
        var input = "a = b = !!(c % d)++ * e + f - g / h % i == 3 != false";
        var (lexer, context) = Prepare(input);
            
        ExpParser.Parse(lexer, ref context, out var exp);

        var expected = new BinaryOpExp(BinaryOpKind.Assign,
            SId("a"),
            new BinaryOpExp(BinaryOpKind.Assign,
                SId("b"),
                new BinaryOpExp(BinaryOpKind.NotEqual,
                    new BinaryOpExp(BinaryOpKind.Equal,
                        new BinaryOpExp(BinaryOpKind.Subtract,
                            new BinaryOpExp(BinaryOpKind.Add,
                                new BinaryOpExp(BinaryOpKind.Multiply,
                                    new UnaryOpExp(UnaryOpKind.LogicalNot,
                                        new UnaryOpExp(UnaryOpKind.LogicalNot,
                                            new UnaryOpExp(UnaryOpKind.PostfixInc,
                                                new BinaryOpExp(BinaryOpKind.Modulo,
                                                    SId("c"),
                                                    SId("d"))))),
                                    SId("e")),
                                SId("f")),
                            new BinaryOpExp(BinaryOpKind.Modulo,
                                new BinaryOpExp(BinaryOpKind.Divide,
                                    SId("g"),
                                    SId("h")),
                                SId("i"))),
                        new IntLiteralExp(3)),
                    new BoolLiteralExp(false))));

        Assert.Equal(expected, exp);
    }
}
