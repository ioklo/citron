#include "pch.h"

#include <Syntax/Syntax.h>
#include <TextAnalysis/ExpParser.h>

#include "TestMisc.h"

using namespace std;
using namespace Citron;

TEST(ExpParser, ParseBoolFalse)
{
    auto [buffer, lexer] = Prepare(UR"---(false)---");

    auto oExp = ParseExp(&lexer);

    auto expected = R"---({
    "$type": "BoolLiteralExpSyntax",
    "value": false
})---";

    EXPECT_SYNTAX_EQ(oExp, expected);
}

TEST(ExpParser, ParseBoolTrue)
{
    auto [buffer, lexer] = Prepare(UR"---(true)---");

    auto oExp = ParseExp(&lexer);

    auto expected = R"---({
    "$type": "BoolLiteralExpSyntax",
    "value": true
})---";

    EXPECT_SYNTAX_EQ(oExp, expected);
}

TEST(ExpParser, ParseComplexExp)
{
    auto [buffer, lexer] = Prepare(UR"---(a = b = !!(c % d)++ * e + f - g / h % i == 3 != false)---");

    auto oExp = ParseExp(&lexer);

    auto expected = R"---({
    "$type": "BinaryOpExpSyntax",
    "kind": "Assign",
    "operand0": {
        "$type": "IdentifierExpSyntax",
        "value": "a",
        "typeArgs": []
    },
    "operand1": {
        "$type": "BinaryOpExpSyntax",
        "kind": "Assign",
        "operand0": {
            "$type": "IdentifierExpSyntax",
            "value": "b",
            "typeArgs": []
        },
        "operand1": {
            "$type": "BinaryOpExpSyntax",
            "kind": "NotEqual",
            "operand0": {
                "$type": "BinaryOpExpSyntax",
                "kind": "Equal",
                "operand0": {
                    "$type": "BinaryOpExpSyntax",
                    "kind": "Subtract",
                    "operand0": {
                        "$type": "BinaryOpExpSyntax",
                        "kind": "Add",
                        "operand0": {
                            "$type": "BinaryOpExpSyntax",
                            "kind": "Multiply",
                            "operand0": {
                                "$type": "UnaryOpExpSyntax",
                                "kind": "LogicalNot",
                                "operand": {
                                    "$type": "UnaryOpExpSyntax",
                                    "kind": "LogicalNot",
                                    "operand": {
                                        "$type": "UnaryOpExpSyntax",
                                        "kind": "PostfixInc",
                                        "operand": {
                                            "$type": "BinaryOpExpSyntax",
                                            "kind": "Modulo",
                                            "operand0": {
                                                "$type": "IdentifierExpSyntax",
                                                "value": "c",
                                                "typeArgs": []
                                            },
                                            "operand1": {
                                                "$type": "IdentifierExpSyntax",
                                                "value": "d",
                                                "typeArgs": []
                                            }
                                        }
                                    }
                                }
                            },
                            "operand1": {
                                "$type": "IdentifierExpSyntax",
                                "value": "e",
                                "typeArgs": []
                            }
                        },
                        "operand1": {
                            "$type": "IdentifierExpSyntax",
                            "value": "f",
                            "typeArgs": []
                        }
                    },
                    "operand1": {
                        "$type": "BinaryOpExpSyntax",
                        "kind": "Modulo",
                        "operand0": {
                            "$type": "BinaryOpExpSyntax",
                            "kind": "Divide",
                            "operand0": {
                                "$type": "IdentifierExpSyntax",
                                "value": "g",
                                "typeArgs": []
                            },
                            "operand1": {
                                "$type": "IdentifierExpSyntax",
                                "value": "h",
                                "typeArgs": []
                            }
                        },
                        "operand1": {
                            "$type": "IdentifierExpSyntax",
                            "value": "i",
                            "typeArgs": []
                        }
                    }
                },
                "operand1": {
                    "$type": "IntLiteralExpSyntax",
                    "value": 3
                }
            },
            "operand1": {
                "$type": "BoolLiteralExpSyntax",
                "value": false
            }
        }
    }
})---";

    EXPECT_SYNTAX_EQ(oExp, expected);
}

TEST(ExpParser, ParseComplexMemberExpSyntax)
{
    auto [buffer, lexer] = Prepare(UR"---(a.b.c<int, list<int>>(1, "str").d)---");

    auto oExp = ParseExp(&lexer);

    auto expected = R"---({
    "$type": "MemberExpSyntax",
    "parent": {
        "$type": "CallExpSyntax",
        "callable": {
            "$type": "MemberExpSyntax",
            "parent": {
                "$type": "MemberExpSyntax",
                "parent": {
                    "$type": "IdentifierExpSyntax",
                    "value": "a",
                    "typeArgs": []
                },
                "memberName": "b",
                "memberTypeArgs": []
            },
            "memberName": "c",
            "memberTypeArgs": [
                {
                    "$type": "IdTypeExpSyntax",
                    "name": "int",
                    "typeArgs": []
                },
                {
                    "$type": "IdTypeExpSyntax",
                    "name": "list",
                    "typeArgs": [
                        {
                            "$type": "IdTypeExpSyntax",
                            "name": "int",
                            "typeArgs": []
                        }
                    ]
                }
            ]
        },
        "args": [
            {
                "$type": "ArgumentSyntax",
                "bOut": false,
                "bParams": false,
                "exp": {
                    "$type": "IntLiteralExpSyntax",
                    "value": 1
                }
            },
            {
                "$type": "ArgumentSyntax",
                "bOut": false,
                "bParams": false,
                "exp": {
                    "$type": "StringExpSyntax",
                    "elements": [
                        {
                            "$type": "TextStringExpSyntaxElement",
                            "text": "str"
                        }
                    ]
                }
            }
        ]
    },
    "memberName": "d",
    "memberTypeArgs": []
})---";

    EXPECT_SYNTAX_EQ(oExp, expected);
}

TEST(ExpParser, ParseIdentifier)
{
    auto [buffer, lexer] = Prepare(UR"---(s)---");

    auto oExp = ParseExp(&lexer);

    auto expected = R"---({
    "$type": "IdentifierExpSyntax",
    "value": "s",
    "typeArgs": []
})---";

    EXPECT_SYNTAX_EQ(oExp, expected);
}

TEST(ExpParser, ParseIdentifierExpWithTypeArgs)
{
    auto [buffer, lexer] = Prepare(UR"---(x<T>)---");

    auto oExp = ParseExp(&lexer);

    auto expected = R"---({
    "$type": "IdentifierExpSyntax",
    "value": "x",
    "typeArgs": [
        {
            "$type": "IdTypeExpSyntax",
            "name": "T",
            "typeArgs": []
        }
    ]
})---";

    EXPECT_SYNTAX_EQ(oExp, expected);
}

TEST(ExpParser, ParseIndirectMemberExp)
{
    auto [buffer, lexer] = Prepare(UR"---(a->b<int>)---");

    auto oExp = ParseExp(&lexer);

    auto expected = R"---({
    "$type": "IndirectMemberExpSyntax",
    "parent": {
        "$type": "IdentifierExpSyntax",
        "value": "a",
        "typeArgs": []
    },
    "memberName": "b",
    "memberTypeArgs": [
        {
            "$type": "IdTypeExpSyntax",
            "name": "int",
            "typeArgs": []
        }
    ]
})---";

    EXPECT_SYNTAX_EQ(oExp, expected);
}

TEST(ExpParser, ParseInt)
{
    auto [buffer, lexer] = Prepare(UR"---(1234)---");

    auto oExp = ParseExp(&lexer);

    auto expected = R"---({
    "$type": "IntLiteralExpSyntax",
    "value": 1234
})---";

    EXPECT_SYNTAX_EQ(oExp, expected);
}

TEST(ExpParser, ParseLambdaExp)
{
    auto [buffer, lexer] = Prepare(UR"---(a = b => (c, int d) => e)---");

    auto oExp = ParseExp(&lexer);

    auto expected = R"---({
    "$type": "BinaryOpExpSyntax",
    "kind": "Assign",
    "operand0": {
        "$type": "IdentifierExpSyntax",
        "value": "a",
        "typeArgs": []
    },
    "operand1": {
        "$type": "LambdaExpSyntax",
        "params": [
            {
                "$type": "LambdaExpParamSyntax",
                "type": null,
                "name": "b",
                "hasOut": false,
                "hasParams": false
            }
        ],
        "body": {
            "$type": "ExpLambdaExpBodySyntax",
            "exp": {
                "$type": "LambdaExpSyntax",
                "params": [
                    {
                        "$type": "LambdaExpParamSyntax",
                        "type": null,
                        "name": "c",
                        "hasOut": false,
                        "hasParams": false
                    },
                    {
                        "$type": "LambdaExpParamSyntax",
                        "type": {
                            "$type": "IdTypeExpSyntax",
                            "name": "int",
                            "typeArgs": []
                        },
                        "name": "d",
                        "hasOut": false,
                        "hasParams": false
                    }
                ],
                "body": {
                    "$type": "ExpLambdaExpBodySyntax",
                    "exp": {
                        "$type": "IdentifierExpSyntax",
                        "value": "e",
                        "typeArgs": []
                    }
                }
            }
        }
    }
})---";

    EXPECT_SYNTAX_EQ(oExp, expected);
}

TEST(ExpParser, ParseListExp)
{
    auto [buffer, lexer] = Prepare(UR"---([ 1, 2, 3 ])---");

    auto oExp = ParseExp(&lexer);

    auto expected = R"---({
    "$type": "ListExpSyntax",
    "elements": [
        {
            "$type": "IntLiteralExpSyntax",
            "value": 1
        },
        {
            "$type": "IntLiteralExpSyntax",
            "value": 2
        },
        {
            "$type": "IntLiteralExpSyntax",
            "value": 3
        }
    ]
})---";

    EXPECT_SYNTAX_EQ(oExp, expected);
}

TEST(ExpParser, ParseNewExp)
{
    auto [buffer, lexer] = Prepare(UR"---(new MyType<X>(2, false, "string"))---");

    auto oExp = ParseExp(&lexer);

    auto expected = R"---({
    "$type": "NewExpSyntax",
    "type": {
        "$type": "IdTypeExpSyntax",
        "name": "MyType",
        "typeArgs": [
            {
                "$type": "IdTypeExpSyntax",
                "name": "X",
                "typeArgs": []
            }
        ]
    },
    "args": [
        {
            "$type": "ArgumentSyntax",
            "bOut": false,
            "bParams": false,
            "exp": {
                "$type": "IntLiteralExpSyntax",
                "value": 2
            }
        },
        {
            "$type": "ArgumentSyntax",
            "bOut": false,
            "bParams": false,
            "exp": {
                "$type": "BoolLiteralExpSyntax",
                "value": false
            }
        },
        {
            "$type": "ArgumentSyntax",
            "bOut": false,
            "bParams": false,
            "exp": {
                "$type": "StringExpSyntax",
                "elements": [
                    {
                        "$type": "TextStringExpSyntaxElement",
                        "text": "string"
                    }
                ]
            }
        }
    ]
})---";

    EXPECT_SYNTAX_EQ(oExp, expected);
}

TEST(ExpParser, ParsePrimaryExp)
{
    auto [buffer, lexer] = Prepare(UR"---((c++(e, f) % d)++)---");

    auto oExp = ParseExp(&lexer);

    auto expected = R"---({
    "$type": "UnaryOpExpSyntax",
    "kind": "PostfixInc",
    "operand": {
        "$type": "BinaryOpExpSyntax",
        "kind": "Modulo",
        "operand0": {
            "$type": "CallExpSyntax",
            "callable": {
                "$type": "UnaryOpExpSyntax",
                "kind": "PostfixInc",
                "operand": {
                    "$type": "IdentifierExpSyntax",
                    "value": "c",
                    "typeArgs": []
                }
            },
            "args": [
                {
                    "$type": "ArgumentSyntax",
                    "bOut": false,
                    "bParams": false,
                    "exp": {
                        "$type": "IdentifierExpSyntax",
                        "value": "e",
                        "typeArgs": []
                    }
                },
                {
                    "$type": "ArgumentSyntax",
                    "bOut": false,
                    "bParams": false,
                    "exp": {
                        "$type": "IdentifierExpSyntax",
                        "value": "f",
                        "typeArgs": []
                    }
                }
            ]
        },
        "operand1": {
            "$type": "IdentifierExpSyntax",
            "value": "d",
            "typeArgs": []
        }
    }
})---";

    EXPECT_SYNTAX_EQ(oExp, expected);
}

TEST(ExpParser, ParseStringExp)
{
    auto [buffer, lexer] = Prepare(UR"---("aaa bbb ${"xxx ${ddd}"} ddd")---");

    auto oExp = ParseExp(&lexer);

    auto expected = R"---({
    "$type": "StringExpSyntax",
    "elements": [
        {
            "$type": "TextStringExpSyntaxElement",
            "text": "aaa bbb "
        },
        {
            "$type": "ExpStringExpSyntaxElement",
            "exp": {
                "$type": "StringExpSyntax",
                "elements": [
                    {
                        "$type": "TextStringExpSyntaxElement",
                        "text": "xxx "
                    },
                    {
                        "$type": "ExpStringExpSyntaxElement",
                        "exp": {
                            "$type": "IdentifierExpSyntax",
                            "value": "ddd",
                            "typeArgs": []
                        }
                    }
                ]
            }
        },
        {
            "$type": "TextStringExpSyntaxElement",
            "text": " ddd"
        }
    ]
})---";

    EXPECT_SYNTAX_EQ(oExp, expected);
}

TEST(ExpParser, ParseTestAndTypeTestExp)
{
    auto [buffer, lexer] = Prepare(UR"---(e + 1 is X<int> < d + 1 is T)---");

    auto oExp = ParseExp(&lexer);

    auto expected = R"---({
    "$type": "IsExpSyntax",
    "exp": {
        "$type": "BinaryOpExpSyntax",
        "kind": "LessThan",
        "operand0": {
            "$type": "IsExpSyntax",
            "exp": {
                "$type": "BinaryOpExpSyntax",
                "kind": "Add",
                "operand0": {
                    "$type": "IdentifierExpSyntax",
                    "value": "e",
                    "typeArgs": []
                },
                "operand1": {
                    "$type": "IntLiteralExpSyntax",
                    "value": 1
                }
            },
            "type": {
                "$type": "IdTypeExpSyntax",
                "name": "X",
                "typeArgs": [
                    {
                        "$type": "IdTypeExpSyntax",
                        "name": "int",
                        "typeArgs": []
                    }
                ]
            }
        },
        "operand1": {
            "$type": "BinaryOpExpSyntax",
            "kind": "Add",
            "operand0": {
                "$type": "IdentifierExpSyntax",
                "value": "d",
                "typeArgs": []
            },
            "operand1": {
                "$type": "IntLiteralExpSyntax",
                "value": 1
            }
        }
    },
    "type": {
        "$type": "IdTypeExpSyntax",
        "name": "T",
        "typeArgs": []
    }
})---";

    EXPECT_SYNTAX_EQ(oExp, expected);
}

