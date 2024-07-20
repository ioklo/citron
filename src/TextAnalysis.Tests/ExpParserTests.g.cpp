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
    "$type": "SBoolLiteralExp",
    "value": false
})---";

    EXPECT_SYNTAX_EQ(oExp, expected);
}

TEST(ExpParser, ParseBoolTrue)
{
    auto [buffer, lexer] = Prepare(UR"---(true)---");

    auto oExp = ParseExp(&lexer);

    auto expected = R"---({
    "$type": "SBoolLiteralExp",
    "value": true
})---";

    EXPECT_SYNTAX_EQ(oExp, expected);
}

TEST(ExpParser, ParseComplexExp)
{
    auto [buffer, lexer] = Prepare(UR"---(a = b = !!(c % d)++ * e + f - g / h % i == 3 != false)---");

    auto oExp = ParseExp(&lexer);

    auto expected = R"---({
    "$type": "SBinaryOpExp",
    "kind": "Assign",
    "operand0": {
        "$type": "SIdentifierExp",
        "value": "a",
        "typeArgs": []
    },
    "operand1": {
        "$type": "SBinaryOpExp",
        "kind": "Assign",
        "operand0": {
            "$type": "SIdentifierExp",
            "value": "b",
            "typeArgs": []
        },
        "operand1": {
            "$type": "SBinaryOpExp",
            "kind": "NotEqual",
            "operand0": {
                "$type": "SBinaryOpExp",
                "kind": "Equal",
                "operand0": {
                    "$type": "SBinaryOpExp",
                    "kind": "Subtract",
                    "operand0": {
                        "$type": "SBinaryOpExp",
                        "kind": "Add",
                        "operand0": {
                            "$type": "SBinaryOpExp",
                            "kind": "Multiply",
                            "operand0": {
                                "$type": "SUnaryOpExp",
                                "kind": "LogicalNot",
                                "operand": {
                                    "$type": "SUnaryOpExp",
                                    "kind": "LogicalNot",
                                    "operand": {
                                        "$type": "SUnaryOpExp",
                                        "kind": "PostfixInc",
                                        "operand": {
                                            "$type": "SBinaryOpExp",
                                            "kind": "Modulo",
                                            "operand0": {
                                                "$type": "SIdentifierExp",
                                                "value": "c",
                                                "typeArgs": []
                                            },
                                            "operand1": {
                                                "$type": "SIdentifierExp",
                                                "value": "d",
                                                "typeArgs": []
                                            }
                                        }
                                    }
                                }
                            },
                            "operand1": {
                                "$type": "SIdentifierExp",
                                "value": "e",
                                "typeArgs": []
                            }
                        },
                        "operand1": {
                            "$type": "SIdentifierExp",
                            "value": "f",
                            "typeArgs": []
                        }
                    },
                    "operand1": {
                        "$type": "SBinaryOpExp",
                        "kind": "Modulo",
                        "operand0": {
                            "$type": "SBinaryOpExp",
                            "kind": "Divide",
                            "operand0": {
                                "$type": "SIdentifierExp",
                                "value": "g",
                                "typeArgs": []
                            },
                            "operand1": {
                                "$type": "SIdentifierExp",
                                "value": "h",
                                "typeArgs": []
                            }
                        },
                        "operand1": {
                            "$type": "SIdentifierExp",
                            "value": "i",
                            "typeArgs": []
                        }
                    }
                },
                "operand1": {
                    "$type": "SIntLiteralExp",
                    "value": 3
                }
            },
            "operand1": {
                "$type": "SBoolLiteralExp",
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
    "$type": "SMemberExp",
    "parent": {
        "$type": "SCallExp",
        "callable": {
            "$type": "SMemberExp",
            "parent": {
                "$type": "SMemberExp",
                "parent": {
                    "$type": "SIdentifierExp",
                    "value": "a",
                    "typeArgs": []
                },
                "memberName": "b",
                "memberTypeArgs": []
            },
            "memberName": "c",
            "memberTypeArgs": [
                {
                    "$type": "SIdTypeExp",
                    "name": "int",
                    "typeArgs": []
                },
                {
                    "$type": "SIdTypeExp",
                    "name": "list",
                    "typeArgs": [
                        {
                            "$type": "SIdTypeExp",
                            "name": "int",
                            "typeArgs": []
                        }
                    ]
                }
            ]
        },
        "args": [
            {
                "$type": "SArgument",
                "bOut": false,
                "bParams": false,
                "exp": {
                    "$type": "SIntLiteralExp",
                    "value": 1
                }
            },
            {
                "$type": "SArgument",
                "bOut": false,
                "bParams": false,
                "exp": {
                    "$type": "SStringExp",
                    "elements": [
                        {
                            "$type": "STextStringExpElement",
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
    "$type": "SIdentifierExp",
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
    "$type": "SIdentifierExp",
    "value": "x",
    "typeArgs": [
        {
            "$type": "SIdTypeExp",
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
    "$type": "SIndirectMemberExp",
    "parent": {
        "$type": "SIdentifierExp",
        "value": "a",
        "typeArgs": []
    },
    "memberName": "b",
    "memberTypeArgs": [
        {
            "$type": "SIdTypeExp",
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
    "$type": "SIntLiteralExp",
    "value": 1234
})---";

    EXPECT_SYNTAX_EQ(oExp, expected);
}

TEST(ExpParser, ParseLambdaExp)
{
    auto [buffer, lexer] = Prepare(UR"---(a = b => (c, int d) => e)---");

    auto oExp = ParseExp(&lexer);

    auto expected = R"---({
    "$type": "SBinaryOpExp",
    "kind": "Assign",
    "operand0": {
        "$type": "SIdentifierExp",
        "value": "a",
        "typeArgs": []
    },
    "operand1": {
        "$type": "SLambdaExp",
        "params": [
            {
                "$type": "SLambdaExpParam",
                "type": null,
                "name": "b",
                "hasOut": false,
                "hasParams": false
            }
        ],
        "body": {
            "$type": "SExpLambdaExpBody",
            "exp": {
                "$type": "SLambdaExp",
                "params": [
                    {
                        "$type": "SLambdaExpParam",
                        "type": null,
                        "name": "c",
                        "hasOut": false,
                        "hasParams": false
                    },
                    {
                        "$type": "SLambdaExpParam",
                        "type": {
                            "$type": "SIdTypeExp",
                            "name": "int",
                            "typeArgs": []
                        },
                        "name": "d",
                        "hasOut": false,
                        "hasParams": false
                    }
                ],
                "body": {
                    "$type": "SExpLambdaExpBody",
                    "exp": {
                        "$type": "SIdentifierExp",
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
    "$type": "SListExp",
    "elements": [
        {
            "$type": "SIntLiteralExp",
            "value": 1
        },
        {
            "$type": "SIntLiteralExp",
            "value": 2
        },
        {
            "$type": "SIntLiteralExp",
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
    "$type": "SNewExp",
    "type": {
        "$type": "SIdTypeExp",
        "name": "MyType",
        "typeArgs": [
            {
                "$type": "SIdTypeExp",
                "name": "X",
                "typeArgs": []
            }
        ]
    },
    "args": [
        {
            "$type": "SArgument",
            "bOut": false,
            "bParams": false,
            "exp": {
                "$type": "SIntLiteralExp",
                "value": 2
            }
        },
        {
            "$type": "SArgument",
            "bOut": false,
            "bParams": false,
            "exp": {
                "$type": "SBoolLiteralExp",
                "value": false
            }
        },
        {
            "$type": "SArgument",
            "bOut": false,
            "bParams": false,
            "exp": {
                "$type": "SStringExp",
                "elements": [
                    {
                        "$type": "STextStringExpElement",
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
    "$type": "SUnaryOpExp",
    "kind": "PostfixInc",
    "operand": {
        "$type": "SBinaryOpExp",
        "kind": "Modulo",
        "operand0": {
            "$type": "SCallExp",
            "callable": {
                "$type": "SUnaryOpExp",
                "kind": "PostfixInc",
                "operand": {
                    "$type": "SIdentifierExp",
                    "value": "c",
                    "typeArgs": []
                }
            },
            "args": [
                {
                    "$type": "SArgument",
                    "bOut": false,
                    "bParams": false,
                    "exp": {
                        "$type": "SIdentifierExp",
                        "value": "e",
                        "typeArgs": []
                    }
                },
                {
                    "$type": "SArgument",
                    "bOut": false,
                    "bParams": false,
                    "exp": {
                        "$type": "SIdentifierExp",
                        "value": "f",
                        "typeArgs": []
                    }
                }
            ]
        },
        "operand1": {
            "$type": "SIdentifierExp",
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
    "$type": "SStringExp",
    "elements": [
        {
            "$type": "STextStringExpElement",
            "text": "aaa bbb "
        },
        {
            "$type": "SExpStringExpElement",
            "exp": {
                "$type": "SStringExp",
                "elements": [
                    {
                        "$type": "STextStringExpElement",
                        "text": "xxx "
                    },
                    {
                        "$type": "SExpStringExpElement",
                        "exp": {
                            "$type": "SIdentifierExp",
                            "value": "ddd",
                            "typeArgs": []
                        }
                    }
                ]
            }
        },
        {
            "$type": "STextStringExpElement",
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
    "$type": "SIsExp",
    "exp": {
        "$type": "SBinaryOpExp",
        "kind": "LessThan",
        "operand0": {
            "$type": "SIsExp",
            "exp": {
                "$type": "SBinaryOpExp",
                "kind": "Add",
                "operand0": {
                    "$type": "SIdentifierExp",
                    "value": "e",
                    "typeArgs": []
                },
                "operand1": {
                    "$type": "SIntLiteralExp",
                    "value": 1
                }
            },
            "type": {
                "$type": "SIdTypeExp",
                "name": "X",
                "typeArgs": [
                    {
                        "$type": "SIdTypeExp",
                        "name": "int",
                        "typeArgs": []
                    }
                ]
            }
        },
        "operand1": {
            "$type": "SBinaryOpExp",
            "kind": "Add",
            "operand0": {
                "$type": "SIdentifierExp",
                "value": "d",
                "typeArgs": []
            },
            "operand1": {
                "$type": "SIntLiteralExp",
                "value": 1
            }
        }
    },
    "type": {
        "$type": "SIdTypeExp",
        "name": "T",
        "typeArgs": []
    }
})---";

    EXPECT_SYNTAX_EQ(oExp, expected);
}

