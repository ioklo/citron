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
    "$type": "SExp_BoolLiteral",
    "value": false
})---";

    EXPECT_SYNTAX_EQ(oExp, expected);
}

TEST(ExpParser, ParseBoolTrue)
{
    auto [buffer, lexer] = Prepare(UR"---(true)---");

    auto oExp = ParseExp(&lexer);

    auto expected = R"---({
    "$type": "SExp_BoolLiteral",
    "value": true
})---";

    EXPECT_SYNTAX_EQ(oExp, expected);
}

TEST(ExpParser, ParseComplexExp)
{
    auto [buffer, lexer] = Prepare(UR"---(a = b = !!(c % d)++ * e + f - g / h % i == 3 != false)---");

    auto oExp = ParseExp(&lexer);

    auto expected = R"---({
    "$type": "SExp_BinaryOp",
    "kind": "Assign",
    "operand0": {
        "$type": "SExp_Identifier",
        "value": "a",
        "typeArgs": []
    },
    "operand1": {
        "$type": "SExp_BinaryOp",
        "kind": "Assign",
        "operand0": {
            "$type": "SExp_Identifier",
            "value": "b",
            "typeArgs": []
        },
        "operand1": {
            "$type": "SExp_BinaryOp",
            "kind": "NotEqual",
            "operand0": {
                "$type": "SExp_BinaryOp",
                "kind": "Equal",
                "operand0": {
                    "$type": "SExp_BinaryOp",
                    "kind": "Subtract",
                    "operand0": {
                        "$type": "SExp_BinaryOp",
                        "kind": "Add",
                        "operand0": {
                            "$type": "SExp_BinaryOp",
                            "kind": "Multiply",
                            "operand0": {
                                "$type": "SExp_UnaryOp",
                                "kind": "LogicalNot",
                                "operand": {
                                    "$type": "SExp_UnaryOp",
                                    "kind": "LogicalNot",
                                    "operand": {
                                        "$type": "SExp_UnaryOp",
                                        "kind": "PostfixInc",
                                        "operand": {
                                            "$type": "SExp_BinaryOp",
                                            "kind": "Modulo",
                                            "operand0": {
                                                "$type": "SExp_Identifier",
                                                "value": "c",
                                                "typeArgs": []
                                            },
                                            "operand1": {
                                                "$type": "SExp_Identifier",
                                                "value": "d",
                                                "typeArgs": []
                                            }
                                        }
                                    }
                                }
                            },
                            "operand1": {
                                "$type": "SExp_Identifier",
                                "value": "e",
                                "typeArgs": []
                            }
                        },
                        "operand1": {
                            "$type": "SExp_Identifier",
                            "value": "f",
                            "typeArgs": []
                        }
                    },
                    "operand1": {
                        "$type": "SExp_BinaryOp",
                        "kind": "Modulo",
                        "operand0": {
                            "$type": "SExp_BinaryOp",
                            "kind": "Divide",
                            "operand0": {
                                "$type": "SExp_Identifier",
                                "value": "g",
                                "typeArgs": []
                            },
                            "operand1": {
                                "$type": "SExp_Identifier",
                                "value": "h",
                                "typeArgs": []
                            }
                        },
                        "operand1": {
                            "$type": "SExp_Identifier",
                            "value": "i",
                            "typeArgs": []
                        }
                    }
                },
                "operand1": {
                    "$type": "SExp_IntLiteral",
                    "value": 3
                }
            },
            "operand1": {
                "$type": "SExp_BoolLiteral",
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
    "$type": "SExp_Member",
    "parent": {
        "$type": "SExp_Call",
        "callable": {
            "$type": "SExp_Member",
            "parent": {
                "$type": "SExp_Member",
                "parent": {
                    "$type": "SExp_Identifier",
                    "value": "a",
                    "typeArgs": []
                },
                "memberName": "b",
                "memberTypeArgs": []
            },
            "memberName": "c",
            "memberTypeArgs": [
                {
                    "$type": "STypeExp_Id",
                    "name": "int",
                    "typeArgs": []
                },
                {
                    "$type": "STypeExp_Id",
                    "name": "list",
                    "typeArgs": [
                        {
                            "$type": "STypeExp_Id",
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
                    "$type": "SExp_IntLiteral",
                    "value": 1
                }
            },
            {
                "$type": "SArgument",
                "bOut": false,
                "bParams": false,
                "exp": {
                    "$type": "SExp_String",
                    "elements": [
                        {
                            "$type": "SStringExpElement_Text",
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
    "$type": "SExp_Identifier",
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
    "$type": "SExp_Identifier",
    "value": "x",
    "typeArgs": [
        {
            "$type": "STypeExp_Id",
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
    "$type": "SExp_IndirectMember",
    "parent": {
        "$type": "SExp_Identifier",
        "value": "a",
        "typeArgs": []
    },
    "memberName": "b",
    "memberTypeArgs": [
        {
            "$type": "STypeExp_Id",
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
    "$type": "SExp_IntLiteral",
    "value": 1234
})---";

    EXPECT_SYNTAX_EQ(oExp, expected);
}

TEST(ExpParser, ParseLambdaExp)
{
    auto [buffer, lexer] = Prepare(UR"---(a = b => (c, int d) => e)---");

    auto oExp = ParseExp(&lexer);

    auto expected = R"---({
    "$type": "SExp_BinaryOp",
    "kind": "Assign",
    "operand0": {
        "$type": "SExp_Identifier",
        "value": "a",
        "typeArgs": []
    },
    "operand1": {
        "$type": "SExp_Lambda",
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
            "$type": "SLambdaExpBody_Exp",
            "exp": {
                "$type": "SExp_Lambda",
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
                            "$type": "STypeExp_Id",
                            "name": "int",
                            "typeArgs": []
                        },
                        "name": "d",
                        "hasOut": false,
                        "hasParams": false
                    }
                ],
                "body": {
                    "$type": "SLambdaExpBody_Exp",
                    "exp": {
                        "$type": "SExp_Identifier",
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
    "$type": "SExp_List",
    "elements": [
        {
            "$type": "SExp_IntLiteral",
            "value": 1
        },
        {
            "$type": "SExp_IntLiteral",
            "value": 2
        },
        {
            "$type": "SExp_IntLiteral",
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
    "$type": "SExp_New",
    "type": {
        "$type": "STypeExp_Id",
        "name": "MyType",
        "typeArgs": [
            {
                "$type": "STypeExp_Id",
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
                "$type": "SExp_IntLiteral",
                "value": 2
            }
        },
        {
            "$type": "SArgument",
            "bOut": false,
            "bParams": false,
            "exp": {
                "$type": "SExp_BoolLiteral",
                "value": false
            }
        },
        {
            "$type": "SArgument",
            "bOut": false,
            "bParams": false,
            "exp": {
                "$type": "SExp_String",
                "elements": [
                    {
                        "$type": "SStringExpElement_Text",
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
    "$type": "SExp_UnaryOp",
    "kind": "PostfixInc",
    "operand": {
        "$type": "SExp_BinaryOp",
        "kind": "Modulo",
        "operand0": {
            "$type": "SExp_Call",
            "callable": {
                "$type": "SExp_UnaryOp",
                "kind": "PostfixInc",
                "operand": {
                    "$type": "SExp_Identifier",
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
                        "$type": "SExp_Identifier",
                        "value": "e",
                        "typeArgs": []
                    }
                },
                {
                    "$type": "SArgument",
                    "bOut": false,
                    "bParams": false,
                    "exp": {
                        "$type": "SExp_Identifier",
                        "value": "f",
                        "typeArgs": []
                    }
                }
            ]
        },
        "operand1": {
            "$type": "SExp_Identifier",
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
    "$type": "SExp_String",
    "elements": [
        {
            "$type": "SStringExpElement_Text",
            "text": "aaa bbb "
        },
        {
            "$type": "SStringExpElement_Exp",
            "exp": {
                "$type": "SExp_String",
                "elements": [
                    {
                        "$type": "SStringExpElement_Text",
                        "text": "xxx "
                    },
                    {
                        "$type": "SStringExpElement_Exp",
                        "exp": {
                            "$type": "SExp_Identifier",
                            "value": "ddd",
                            "typeArgs": []
                        }
                    }
                ]
            }
        },
        {
            "$type": "SStringExpElement_Text",
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
    "$type": "SExp_Is",
    "exp": {
        "$type": "SExp_BinaryOp",
        "kind": "LessThan",
        "operand0": {
            "$type": "SExp_Is",
            "exp": {
                "$type": "SExp_BinaryOp",
                "kind": "Add",
                "operand0": {
                    "$type": "SExp_Identifier",
                    "value": "e",
                    "typeArgs": []
                },
                "operand1": {
                    "$type": "SExp_IntLiteral",
                    "value": 1
                }
            },
            "type": {
                "$type": "STypeExp_Id",
                "name": "X",
                "typeArgs": [
                    {
                        "$type": "STypeExp_Id",
                        "name": "int",
                        "typeArgs": []
                    }
                ]
            }
        },
        "operand1": {
            "$type": "SExp_BinaryOp",
            "kind": "Add",
            "operand0": {
                "$type": "SExp_Identifier",
                "value": "d",
                "typeArgs": []
            },
            "operand1": {
                "$type": "SExp_IntLiteral",
                "value": 1
            }
        }
    },
    "type": {
        "$type": "STypeExp_Id",
        "name": "T",
        "typeArgs": []
    }
})---";

    EXPECT_SYNTAX_EQ(oExp, expected);
}

