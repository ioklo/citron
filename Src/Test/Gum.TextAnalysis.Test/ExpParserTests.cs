using Gum.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

using static Gum.Infra.Misc;
using static Gum.Syntax.SyntaxFactory;

namespace Gum.TextAnalysis.Test
{
    public class ExpParserTests
    {
        async ValueTask<(ExpParser, ParserContext)> PrepareAsync(string input)
        {
            var lexer = new Lexer();
            var parser = new Parser(lexer);
            var buffer = new Buffer(new StringReader(input));
            var bufferPos = await buffer.MakePosition().NextAsync();
            var lexerContext = LexerContext.Make(bufferPos);
            var parserContext = ParserContext.Make(lexerContext);

            return (parser.expParser, parserContext);
        }

        [Fact]
        public async Task TestParseIdentifierExpAsync()
        {
            (var expParser, var context) = await PrepareAsync("x");

            var expResult = await expParser.ParseExpAsync(context);

            Assert.Equal(SId("x"), expResult.Elem);
        }

        [Fact]
        public async Task TestParseIdentifierExpWithTypeArgsAsync()
        {
            // x<T> vs (x < T) > 
            (var expParser, var context) = await PrepareAsync("x<T>");

            var expResult = await expParser.ParseExpAsync(context);

            Assert.Equal(new IdentifierExp("x", Arr<TypeExp>(new IdTypeExp("T", default))), expResult.Elem);
        }

        [Fact]
        public async Task TestParseStringExpAsync()
        {
            var input = "\"aaa bbb ${\"xxx ${ddd}\"} ddd\"";
            (var expParser, var context) = await PrepareAsync(input);

            var expResult = await expParser.ParseExpAsync(context);

            var expected = new StringExp(Arr<StringExpElement>(
                new TextStringExpElement("aaa bbb "),
                new ExpStringExpElement(new StringExp(Arr<StringExpElement>(
                    new TextStringExpElement("xxx "),
                    new ExpStringExpElement(SId("ddd"))
                ))),
                new TextStringExpElement(" ddd")
            ));

            Assert.Equal(expected, expResult.Elem);
        }

        [Theory]
        [InlineData("true", true)]
        [InlineData("false", false)]
        public async Task TestParseBoolAsync(string input, bool bExpectedResult)
        {   
            (var expParser, var context) = await PrepareAsync(input); 
            
            var expResult = await expParser.ParseExpAsync(context);

            var expected = new BoolLiteralExp(bExpectedResult);

            Assert.Equal(expected, expResult.Elem);
        }

        [Fact]
        public async Task TestParseIntAsync()
        {
            var input = "1234";

            (var expParser, var context) = await PrepareAsync(input);

            var expResult = await expParser.ParseExpAsync(context);

            var expected = new IntLiteralExp(1234);

            Assert.Equal(expected, expResult.Elem);
        }

        [Fact]
        public async Task TestParsePrimaryExpAsync()
        {
            var input = "(c++(e, f) % d)++";
            (var expParser, var context) = await PrepareAsync(input);

            var expResult = await expParser.ParsePrimaryExpAsync(context);

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

            Assert.Equal(expected, expResult.Elem);
        }        

        [Fact]
        public async Task TestParseLambdaExpAsync()
        {
            var input = "a = b => (c, int d) => e";
            (var expParser, var context) = await PrepareAsync(input);

            var expResult = await expParser.ParseExpAsync(context);

            var expected = new BinaryOpExp(BinaryOpKind.Assign,
                SId("a"),
                new LambdaExp(
                    Arr(new LambdaExpParam(FuncParamKind.Normal, null, "b")),
                    new ReturnStmt(
                        new ReturnValueInfo(
                            false,
                            new LambdaExp(
                                Arr(
                                    new LambdaExpParam(FuncParamKind.Normal, null, "c"),
                                    new LambdaExpParam(FuncParamKind.Normal, new IdTypeExp("int", default), "d")
                                ),
                                new ReturnStmt(new ReturnValueInfo(false, SId("e")))
                            )
                        )
                    )
                )
            );

            Assert.Equal(expected, expResult.Elem);
        }

        [Fact]
        public async Task TestParseComplexMemberExpAsync()
        {
            var input = "a.b.c<int, list<int>>(1, \"str\").d";
            (var expParser, var context) = await PrepareAsync(input);

            var expResult = await expParser.ParseExpAsync(context);

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

            Assert.Equal(expected, expResult.Elem);
        }

        [Fact]
        public async Task TestParseListExpAsync()
        {
            var input = "[ 1, 2, 3 ]";
            (var expParser, var context) = await PrepareAsync(input);

            var expResult = await expParser.ParseExpAsync(context);

            var expected = new ListExp(null, Arr<Exp>(                
                new IntLiteralExp(1),
                new IntLiteralExp(2),
                new IntLiteralExp(3)
            ));
                
            Assert.Equal(expected, expResult.Elem);
        }

        [Fact]
        public async Task TestParseNewExpAsync()
        {
            var input = "new MyType<X>(2, false, \"string\")";
            (var expParser, var context) = await PrepareAsync(input);

            var expResult = await expParser.ParseExpAsync(context);

            var expected = new NewExp(
                new IdTypeExp("MyType", Arr<TypeExp>(new IdTypeExp("X", default))),
                Arr<Argument>(
                    new Argument.Normal(new IntLiteralExp(2)),
                    new Argument.Normal(new BoolLiteralExp(false)),
                    new Argument.Normal(SString("string"))
                ));

            Assert.Equal(expected, expResult.Elem);
        }
        
        [Fact]
        public async Task TestParseComplexExpAsync()
        {
            var input = "a = b = !!(c % d)++ * e + f - g / h % i == 3 != false";
            (var expParser, var context) = await PrepareAsync(input);
            
            var expResult = await expParser.ParseExpAsync(context);

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

            Assert.Equal(expected, expResult.Elem);
        }
    }
}
