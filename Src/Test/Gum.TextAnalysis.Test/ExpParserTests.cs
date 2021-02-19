using Gum.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

using static Gum.TextAnalysis.Test.TestMisc;

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

            Assert.Equal(SimpleSId("x"), expResult.Elem, SyntaxEqualityComparer.Instance);
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
                    new ExpStringExpElement(SimpleSId("ddd"))
                ))),
                new TextStringExpElement(" ddd")
            ));

            Assert.Equal(expected, expResult.Elem, SyntaxEqualityComparer.Instance);
        }

        [Theory]
        [InlineData("true", true)]
        [InlineData("false", false)]
        public async Task TestParseBoolAsync(string input, bool bExpectedResult)
        {   
            (var expParser, var context) = await PrepareAsync(input); 
            
            var expResult = await expParser.ParseExpAsync(context);

            var expected = new BoolLiteralExp(bExpectedResult);

            Assert.Equal(expected, expResult.Elem, SyntaxEqualityComparer.Instance);
        }

        [Fact]
        public async Task TestParseIntAsync()
        {
            var input = "1234";

            (var expParser, var context) = await PrepareAsync(input);

            var expResult = await expParser.ParseExpAsync(context);

            var expected = new IntLiteralExp(1234);

            Assert.Equal(expected, expResult.Elem, SyntaxEqualityComparer.Instance);
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
                        new UnaryOpExp(UnaryOpKind.PostfixInc, SimpleSId("c")), 
                        Arr<Exp>(SimpleSId("e"), SimpleSId("f"))
                    ),
                    SimpleSId("d")
                )
            );

            Assert.Equal(expected, expResult.Elem, SyntaxEqualityComparer.Instance);
        }        

        [Fact]
        public async Task TestParseLambdaExpAsync()
        {
            var input = "a = b => (c, int d) => e";
            (var expParser, var context) = await PrepareAsync(input);

            var expResult = await expParser.ParseExpAsync(context);

            var expected = new BinaryOpExp(BinaryOpKind.Assign,
                SimpleSId("a"),
                new LambdaExp(
                    Arr(new LambdaExpParam(null, "b")),
                    new ReturnStmt(
                        new LambdaExp(
                            Arr(
                                new LambdaExpParam(null, "c"),
                                new LambdaExpParam(new IdTypeExp("int", default), "d")
                            ),
                            new ReturnStmt(SimpleSId("e"))
                        )
                    )
                )
            );

            Assert.Equal(expected, expResult.Elem, SyntaxEqualityComparer.Instance);
        }

        [Fact]
        public async Task TestParseComplexMemberExpAsync()
        {
            var input = "a.b.c(1, \"str\").d";
            (var expParser, var context) = await PrepareAsync(input);

            var expResult = await expParser.ParseExpAsync(context);

            var expected =
                new MemberExp(
                    new MemberCallExp(
                        new MemberExp(SimpleSId("a"), "b", default),
                        "c",
                        default,
                        Arr<Exp>(new IntLiteralExp(1), SimpleSStringExp("str"))
                    ),                        
                    "d",
                    default
                );

            Assert.Equal(expected, expResult.Elem, SyntaxEqualityComparer.Instance);
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
                
            Assert.Equal(expected, expResult.Elem, SyntaxEqualityComparer.Instance);
        }

        [Fact]
        public async Task TestParseNewExpAsync()
        {
            var input = "new MyType<X>(2, false, \"string\")";
            (var expParser, var context) = await PrepareAsync(input);

            var expResult = await expParser.ParseExpAsync(context);

            var expected = new NewExp(
                new IdTypeExp("MyType", Arr<TypeExp>(new IdTypeExp("X", default))),
                Arr<Exp>(
                    new IntLiteralExp(2),
                    new BoolLiteralExp(false),
                    SimpleSStringExp("string")
                ));

            Assert.Equal(expected, expResult.Elem, SyntaxEqualityComparer.Instance);
        }

        [Fact]
        public async Task TestParseComplexExpAsync()
        {
            var input = "a = b = !!(c % d)++ * e + f - g / h % i == 3 != false";
            (var expParser, var context) = await PrepareAsync(input);
            
            var expResult = await expParser.ParseExpAsync(context);

            var expected = new BinaryOpExp(BinaryOpKind.Assign,
                SimpleSId("a"),
                new BinaryOpExp(BinaryOpKind.Assign,
                    SimpleSId("b"),
                    new BinaryOpExp(BinaryOpKind.NotEqual,
                        new BinaryOpExp(BinaryOpKind.Equal,
                            new BinaryOpExp(BinaryOpKind.Subtract,
                                new BinaryOpExp(BinaryOpKind.Add,
                                    new BinaryOpExp(BinaryOpKind.Multiply,
                                        new UnaryOpExp(UnaryOpKind.LogicalNot,
                                            new UnaryOpExp(UnaryOpKind.LogicalNot,
                                                new UnaryOpExp(UnaryOpKind.PostfixInc,
                                                    new BinaryOpExp(BinaryOpKind.Modulo,
                                                        SimpleSId("c"),
                                                        SimpleSId("d"))))),
                                        SimpleSId("e")),
                                    SimpleSId("f")),
                                new BinaryOpExp(BinaryOpKind.Modulo,
                                    new BinaryOpExp(BinaryOpKind.Divide,
                                        SimpleSId("g"),
                                        SimpleSId("h")),
                                    SimpleSId("i"))),
                            new IntLiteralExp(3)),
                        new BoolLiteralExp(false))));

            Assert.Equal(expected, expResult.Elem, SyntaxEqualityComparer.Instance);
        }
    }
}
