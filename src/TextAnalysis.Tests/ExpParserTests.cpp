#include "pch.h"

#include <string>
#include <Infra/StringWriter.h>
#include <Infra/make_vector.h>
#include <TextAnalysis/ExpParser.h>
#include <Syntax/Syntax.h>

#include "TestMisc.h"

using namespace std;
using namespace tcb;
using namespace Citron;

optional<ExpSyntax> RunCode(u32string code)
{
    auto [buffer, lexer] = Prepare(code);
    return ParseExp(&lexer);
}

TEST(ExpParser, ParseIdentifier)
{
    auto oExp = RunCode(U"x");
    auto expect = IdentifierExpSyntax("x");

    EXPECT_EQ(ToJson(oExp), ToJson(expect));
}

TEST(ExpParser, ParseIdentifierExpWithTypeArgs)
{
    // x<T> vs (x < T) > 
    auto oExp = RunCode(U"x<T>");
    auto expect = IdentifierExpSyntax("x", make_vector<TypeExpSyntax>( IdTypeExpSyntax("T") ));
    EXPECT_EQ(ToJson(oExp), ToJson(expect));
}

TEST(ExpParser, ParseStringExp)
{
    auto oExp = RunCode(U"\"aaa bbb ${\"xxx ${ddd}\"} ddd\"");
    auto expect = StringExpSyntax(make_vector<StringExpSyntaxElement>(
        TextStringExpSyntaxElement{"aaa bbb "},
        ExpStringExpSyntaxElement(StringExpSyntax(make_vector<StringExpSyntaxElement>(
            TextStringExpSyntaxElement{"xxx "},
            ExpStringExpSyntaxElement{IdentifierExpSyntax("ddd")}
        ))),
        TextStringExpSyntaxElement{" ddd"}
    ));

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
                    IdentifierExpSyntax("e"),
                    IntLiteralExpSyntax(1)
                ),
                IdTypeExpSyntax("X", make_vector<TypeExpSyntax>(IdTypeExpSyntax("int")))
            ),
            BinaryOpExpSyntax(
                BinaryOpSyntaxKind::Add,
                IdentifierExpSyntax("d"),
                IntLiteralExpSyntax(1)
            )
        ),
        IdTypeExpSyntax("T")
    );

    // EXPECT_SYNTAX_EQ(oExp, expected);
}

TEST(ExpParser, ParsePrimaryExp)
{
    auto oExp = RunCode(U"(c++(e, f) % d)++");

    auto expected = UnaryOpExpSyntax(
        UnaryOpSyntaxKind::PostfixInc,
        BinaryOpExpSyntax(
            BinaryOpSyntaxKind::Modulo,
            CallExpSyntax(
                UnaryOpExpSyntax(UnaryOpSyntaxKind::PostfixInc, IdentifierExpSyntax("c")), 
                make_vector(
                    ArgumentSyntax(IdentifierExpSyntax("e")),
                    ArgumentSyntax(IdentifierExpSyntax("f"))
                )
            ),
            IdentifierExpSyntax("d")
        )
    );

    EXPECT_EQ(ToJson(oExp), ToJson(expected));
}

TEST(ExpParser, ParseLambdaExp)
{
    auto oExp = RunCode(U"a = b => (c, int d) => e");

    auto expected = BinaryOpExpSyntax(BinaryOpSyntaxKind::Assign,
        IdentifierExpSyntax("a"),
        LambdaExpSyntax(
            make_vector( LambdaExpParamSyntax{ nullopt, "b", false, false} ),
            make_vector<StmtSyntax>(
                ReturnStmtSyntax(
                    LambdaExpSyntax(
                        make_vector(
                            LambdaExpParamSyntax{ nullopt, "c", false, false },
                            LambdaExpParamSyntax{ IdTypeExpSyntax("int"), "d", false, false }
                        ),
                        make_vector<StmtSyntax>(ReturnStmtSyntax(IdentifierExpSyntax("e")))
                    )
                )
            )
        )
    );

    EXPECT_EQ(ToJson(oExp), ToJson(expected));
}


TEST(ExpParser, ParseIndirectMemberExp)
{
    auto oExp = RunCode(U"a->b<int>");
    
    // (*a).b<int>
    auto expected = MemberExpSyntax(
        UnaryOpExpSyntax(UnaryOpSyntaxKind::Deref, IdentifierExpSyntax("a")), 
        "b", 
        make_vector<TypeExpSyntax>( IdTypeExpSyntax("int") )
    );

    // EXPECT_SYNTAX_EQ(oExp, expected);
}

TEST(ExpParser, ParseComplexMemberExpSyntax)
{
    auto oExp = RunCode(U"a.b.c<int, list<int>>(1, \"str\").d");
    
    auto expected =
        MemberExpSyntax(
            CallExpSyntax(
                MemberExpSyntax(
                    MemberExpSyntax(
                        IdentifierExpSyntax("a"), 
                        "b"
                    ),
                    "c",
                    make_vector<TypeExpSyntax>(
                        IdTypeExpSyntax("int"),
                        IdTypeExpSyntax("list", make_vector<TypeExpSyntax>(IdTypeExpSyntax("int")))
                    )
                ),
                make_vector(
                    ArgumentSyntax(IntLiteralExpSyntax(1)),
                    ArgumentSyntax(StringExpSyntax("str"))
                )
            ),
            "d"
        );

    EXPECT_EQ(ToJson(oExp), ToJson(expected));
}

TEST(ExpParser, ParseListExp)
{
    auto oExp = RunCode(U"[ 1, 2, 3 ]");

    auto expected = ListExpSyntax(make_vector<ExpSyntax>(
        IntLiteralExpSyntax(1),
        IntLiteralExpSyntax(2),
        IntLiteralExpSyntax(3)
    ));

    EXPECT_EQ(ToJson(oExp), ToJson(expected));
}

TEST(ExpParser, ParseNewExp)
{
    auto oExp = RunCode(U"new MyType<X>(2, false, \"string\")");
    auto expected = NewExpSyntax(
        IdTypeExpSyntax("MyType", make_vector<TypeExpSyntax>( IdTypeExpSyntax("X") )),
        make_vector(
            ArgumentSyntax(IntLiteralExpSyntax(2)),
            ArgumentSyntax(BoolLiteralExpSyntax(false)),
            ArgumentSyntax(StringExpSyntax("string"))
        )
    );

    // EXPECT_SYNTAX_EQ(oExp, expected);
}

TEST(ExpParser, ParseComplexExp)
{
    auto oExp = RunCode(U"a = b = !!(c % d)++ * e + f - g / h % i == 3 != false");

    auto expected = BinaryOpExpSyntax(BinaryOpSyntaxKind::Assign,
        IdentifierExpSyntax("a"),
        BinaryOpExpSyntax(BinaryOpSyntaxKind::Assign,
            IdentifierExpSyntax("b"),
            BinaryOpExpSyntax(BinaryOpSyntaxKind::NotEqual,
                BinaryOpExpSyntax(BinaryOpSyntaxKind::Equal,
                    BinaryOpExpSyntax(BinaryOpSyntaxKind::Subtract,
                        BinaryOpExpSyntax(BinaryOpSyntaxKind::Add,
                            BinaryOpExpSyntax(BinaryOpSyntaxKind::Multiply,
                                UnaryOpExpSyntax(UnaryOpSyntaxKind::LogicalNot,
                                    UnaryOpExpSyntax(UnaryOpSyntaxKind::LogicalNot,
                                        UnaryOpExpSyntax(UnaryOpSyntaxKind::PostfixInc,
                                            BinaryOpExpSyntax(BinaryOpSyntaxKind::Modulo,
                                                IdentifierExpSyntax("c"),
                                                IdentifierExpSyntax("d"))))),
                                IdentifierExpSyntax("e")),
                            IdentifierExpSyntax("f")),
                        BinaryOpExpSyntax(BinaryOpSyntaxKind::Modulo,
                            BinaryOpExpSyntax(BinaryOpSyntaxKind::Divide,
                                IdentifierExpSyntax("g"),
                                IdentifierExpSyntax("h")),
                            IdentifierExpSyntax("i"))),
                    IntLiteralExpSyntax(3)),
                BoolLiteralExpSyntax(false))));

    EXPECT_EQ(ToJson(oExp), ToJson(expected));
}