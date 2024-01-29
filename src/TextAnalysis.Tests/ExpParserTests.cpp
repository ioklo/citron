#include "pch.h"

#include <string>
#include <Infra/StringWriter.h>
#include <TextAnalysis/ExpParser.h>
#include <Syntax/StringExpSyntaxElements.h>
#include <Syntax/StmtSyntaxes.h>
#include <Syntax/ExpSyntaxes.h>

#include "TestMisc.h"

using namespace std;
using namespace Citron;

optional<ExpSyntax> RunCode(u32string code)
{
    auto [buffer, lexer] = Prepare(code);
    return ParseExp(&lexer);
}

TEST(ExpParser, ParseIdentifier)
{
    auto oExp = RunCode(U"x");
    auto expect = IdentifierExpSyntax(U"x");

    EXPECT_EQ(ToJson(oExp), ToJson(expect));
}

TEST(ExpParser, ParseIdentifierExpWithTypeArgs)
{
    // x<T> vs (x < T) > 
    auto oExp = RunCode(U"x<T>");
    auto expect = IdentifierExpSyntax(U"x", { IdTypeExpSyntax(U"T") });
    EXPECT_EQ(ToJson(oExp), ToJson(expect));
}

TEST(ExpParser, ParseStringExp)
{
    auto oExp = RunCode(U"\"aaa bbb ${\"xxx ${ddd}\"} ddd\"");
    auto expect = StringExpSyntax({
        TextStringExpSyntaxElement{U"aaa bbb "},
        ExpStringExpSyntaxElement{StringExpSyntax({
            TextStringExpSyntaxElement{U"xxx "},
            ExpStringExpSyntaxElement{IdentifierExpSyntax(U"ddd")}
        })},
        TextStringExpSyntaxElement{U" ddd"}
    });

    EXPECT_EQ(ToJson(oExp), ToJson(expect));
}

TEST(ExpParser, ParseBoolTrue)
{
    auto oExp = RunCode(U"true");
    auto expected = BoolLiteralExpSyntax(true);

    EXPECT_EQ(ToJson(oExp), ToJson(expected));
}

TEST(ExpParser, ParseBoolFalse)
{
    auto oExp = RunCode(U"false");
    auto expected = BoolLiteralExpSyntax(false);

    EXPECT_EQ(ToJson(oExp), ToJson(expected));
}

TEST(ExpParser, ParseInt)
{
    auto oExp = RunCode(U"1234");
    auto expected = IntLiteralExpSyntax(1234);

    EXPECT_EQ(ToJson(oExp), ToJson(expected));
}

TEST(ExpParser, ParseTestAndTypeTestExp) // left associative
{
    auto oExp = RunCode(U"e + 1 is X<int> < d + 1 is T"); // (((e + 1) is X<int>) < (d + 1)) is T

    auto expected = IsExpSyntax(
        BinaryOpExpSyntax(
            BinaryOpSyntaxKind::LessThan,
            IsExpSyntax(
                BinaryOpExpSyntax(
                    BinaryOpSyntaxKind::Add,
                    IdentifierExpSyntax(U"e"),
                    IntLiteralExpSyntax(1)
                ),
                IdTypeExpSyntax(U"X", { IdTypeExpSyntax(U"int") })
            ),
            BinaryOpExpSyntax(
                BinaryOpSyntaxKind::Add,
                IdentifierExpSyntax(U"d"),
                IntLiteralExpSyntax(1)
            )
        ),
        IdTypeExpSyntax(U"T")
    );

    EXPECT_SYNTAX_EQ(oExp, expected);
}

TEST(ExpParser, ParsePrimaryExp)
{
    auto oExp = RunCode(U"(c++(e, f) % d)++");

    auto expected = UnaryOpExpSyntax(
        UnaryOpSyntaxKind::PostfixInc,
        BinaryOpExpSyntax(
            BinaryOpSyntaxKind::Modulo,
            CallExpSyntax(
                UnaryOpExpSyntax(UnaryOpSyntaxKind::PostfixInc, IdentifierExpSyntax(U"c")), {
                    ArgumentSyntax(IdentifierExpSyntax(U"e")),
                    ArgumentSyntax(IdentifierExpSyntax(U"f")),
                }
            ),
            IdentifierExpSyntax(U"d")
        )
    );

    EXPECT_EQ(ToJson(oExp), ToJson(expected));
}

TEST(ExpParser, ParseLambdaExp)
{
    auto oExp = RunCode(U"a = b => (c, int d) => e");

    auto expected = BinaryOpExpSyntax(BinaryOpSyntaxKind::Assign,
        IdentifierExpSyntax(U"a"),
        LambdaExpSyntax(
            { LambdaExpParamSyntax{ nullopt, U"b", false, false} },
            {
                ReturnStmtSyntax(
                    ReturnValueSyntaxInfo{
                        LambdaExpSyntax(
                            {
                                LambdaExpParamSyntax{ nullopt, U"c", false, false },
                                LambdaExpParamSyntax{ IdTypeExpSyntax(U"int"), U"d", false, false }
                            },
                            {ReturnStmtSyntax(ReturnValueSyntaxInfo{IdentifierExpSyntax(U"e")})}
                        )
                    }
                )
            }
        )
    );

    EXPECT_EQ(ToJson(oExp), ToJson(expected));
}


TEST(ExpParser, ParseIndirectMemberExp)
{
    auto oExp = RunCode(U"a->b<int>");
    
    // (*a).b<int>
    auto expected = MemberExpSyntax(
        UnaryOpExpSyntax(UnaryOpSyntaxKind::Deref, IdentifierExpSyntax(U"a")), 
        U"b", 
        { IdTypeExpSyntax(U"int") }
    );

    EXPECT_SYNTAX_EQ(oExp, expected);
}

TEST(ExpParser, ParseComplexMemberExpSyntax)
{
    auto oExp = RunCode(U"a.b.c<int, list<int>>(1, \"str\").d");
    
    auto expected =
        MemberExpSyntax(
            CallExpSyntax(
                MemberExpSyntax(
                    MemberExpSyntax(
                        IdentifierExpSyntax(U"a"), 
                        U"b"
                    ),
                    U"c",
                    {
                        IdTypeExpSyntax(U"int"),
                        IdTypeExpSyntax(U"list", {IdTypeExpSyntax(U"int")})
                    }
                ),
                {
                    ArgumentSyntax(IntLiteralExpSyntax(1)),
                    ArgumentSyntax(StringExpSyntax(U"str"))
                }
            ),
            U"d"
        );

    EXPECT_EQ(ToJson(oExp), ToJson(expected));
}

TEST(ExpParser, ParseListExp)
{
    auto oExp = RunCode(U"[ 1, 2, 3 ]");

    auto expected = ListExpSyntax({
        IntLiteralExpSyntax(1),
        IntLiteralExpSyntax(2),
        IntLiteralExpSyntax(3)
    });

    EXPECT_EQ(ToJson(oExp), ToJson(expected));
}

TEST(ExpParser, ParseNewExp)
{
    auto oExp = RunCode(U"new MyType<X>(2, false, \"string\")");
    auto expected = NewExpSyntax(
        IdTypeExpSyntax(U"MyType", { IdTypeExpSyntax(U"X") }),
        {
            ArgumentSyntax(IntLiteralExpSyntax(2)),
            ArgumentSyntax(BoolLiteralExpSyntax(false)),
            ArgumentSyntax(StringExpSyntax(U"string"))
        }
    );

    EXPECT_SYNTAX_EQ(oExp, expected);
}

TEST(ExpParser, ParseComplexExp)
{
    auto oExp = RunCode(U"a = b = !!(c % d)++ * e + f - g / h % i == 3 != false");

    auto expected = BinaryOpExpSyntax(BinaryOpSyntaxKind::Assign,
        IdentifierExpSyntax(U"a"),
        BinaryOpExpSyntax(BinaryOpSyntaxKind::Assign,
            IdentifierExpSyntax(U"b"),
            BinaryOpExpSyntax(BinaryOpSyntaxKind::NotEqual,
                BinaryOpExpSyntax(BinaryOpSyntaxKind::Equal,
                    BinaryOpExpSyntax(BinaryOpSyntaxKind::Subtract,
                        BinaryOpExpSyntax(BinaryOpSyntaxKind::Add,
                            BinaryOpExpSyntax(BinaryOpSyntaxKind::Multiply,
                                UnaryOpExpSyntax(UnaryOpSyntaxKind::LogicalNot,
                                    UnaryOpExpSyntax(UnaryOpSyntaxKind::LogicalNot,
                                        UnaryOpExpSyntax(UnaryOpSyntaxKind::PostfixInc,
                                            BinaryOpExpSyntax(BinaryOpSyntaxKind::Modulo,
                                                IdentifierExpSyntax(U"c"),
                                                IdentifierExpSyntax(U"d"))))),
                                IdentifierExpSyntax(U"e")),
                            IdentifierExpSyntax(U"f")),
                        BinaryOpExpSyntax(BinaryOpSyntaxKind::Modulo,
                            BinaryOpExpSyntax(BinaryOpSyntaxKind::Divide,
                                IdentifierExpSyntax(U"g"),
                                IdentifierExpSyntax(U"h")),
                            IdentifierExpSyntax(U"i"))),
                    IntLiteralExpSyntax(3)),
                BoolLiteralExpSyntax(false))));

    EXPECT_EQ(ToJson(oExp), ToJson(expected));
}