#include "pch.h"

#include <Syntax/Syntax.h>
#include <TextAnalysis/ScriptParser.h>

#include "TestMisc.h"

using namespace std;
using namespace Citron;

TEST(ScriptParser, ParseComplexScript)
{
    auto [buffer, lexer] = Prepare(UR"---(void Main()
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
})---");

    auto oScript = ParseScript(&lexer);

    auto expected = R"---({
    "$type": "SScript",
    "elements": [
        {
            "$type": "SGlobalFuncDecl",
            "accessModifier": null,
            "bSequence": false,
            "retType": {
                "$type": "SIdTypeExp",
                "name": "void",
                "typeArgs": []
            },
            "name": "Main",
            "typeParams": [],
            "parameters": [],
            "body": [
                {
                    "$type": "SVarDeclStmt",
                    "varDecl": {
                        "$type": "SVarDecl",
                        "type": {
                            "$type": "SIdTypeExp",
                            "name": "int",
                            "typeArgs": []
                        },
                        "elements": [
                            {
                                "$type": "SVarDeclElement",
                                "varName": "sum",
                                "initExp": {
                                    "$type": "SIntLiteralExp",
                                    "value": 0
                                }
                            }
                        ]
                    }
                },
                {
                    "$type": "SForStmt",
                    "initializer": {
                        "$type": "SVarDeclForStmtInitializer",
                        "varDecl": {
                            "$type": "SVarDecl",
                            "type": {
                                "$type": "SIdTypeExp",
                                "name": "int",
                                "typeArgs": []
                            },
                            "elements": [
                                {
                                    "$type": "SVarDeclElement",
                                    "varName": "i",
                                    "initExp": {
                                        "$type": "SIntLiteralExp",
                                        "value": 0
                                    }
                                }
                            ]
                        }
                    },
                    "cond": {
                        "$type": "SBinaryOpExp",
                        "kind": "LessThan",
                        "operand0": {
                            "$type": "SIdentifierExp",
                            "value": "i",
                            "typeArgs": []
                        },
                        "operand1": {
                            "$type": "SIntLiteralExp",
                            "value": 5
                        }
                    },
                    "cont": {
                        "$type": "SUnaryOpExp",
                        "kind": "PostfixInc",
                        "operand": {
                            "$type": "SIdentifierExp",
                            "value": "i",
                            "typeArgs": []
                        }
                    },
                    "body": {
                        "$type": "SBlockEmbeddableStmt",
                        "stmts": [
                            {
                                "$type": "SIfStmt",
                                "cond": {
                                    "$type": "SBinaryOpExp",
                                    "kind": "Equal",
                                    "operand0": {
                                        "$type": "SBinaryOpExp",
                                        "kind": "Modulo",
                                        "operand0": {
                                            "$type": "SIdentifierExp",
                                            "value": "i",
                                            "typeArgs": []
                                        },
                                        "operand1": {
                                            "$type": "SIntLiteralExp",
                                            "value": 2
                                        }
                                    },
                                    "operand1": {
                                        "$type": "SIntLiteralExp",
                                        "value": 0
                                    }
                                },
                                "body": {
                                    "$type": "SSingleEmbeddableStmt",
                                    "stmt": {
                                        "$type": "SExpStmt",
                                        "exp": {
                                            "$type": "SBinaryOpExp",
                                            "kind": "Assign",
                                            "operand0": {
                                                "$type": "SIdentifierExp",
                                                "value": "sum",
                                                "typeArgs": []
                                            },
                                            "operand1": {
                                                "$type": "SBinaryOpExp",
                                                "kind": "Add",
                                                "operand0": {
                                                    "$type": "SIdentifierExp",
                                                    "value": "sum",
                                                    "typeArgs": []
                                                },
                                                "operand1": {
                                                    "$type": "SIdentifierExp",
                                                    "value": "i",
                                                    "typeArgs": []
                                                }
                                            }
                                        }
                                    }
                                },
                                "elseBody": {
                                    "$type": "SSingleEmbeddableStmt",
                                    "stmt": {
                                        "$type": "SCommandStmt",
                                        "commands": [
                                            {
                                                "$type": "SStringExp",
                                                "elements": [
                                                    {
                                                        "$type": "STextStringExpElement",
                                                        "text": "            echo hi "
                                                    }
                                                ]
                                            }
                                        ]
                                    }
                                }
                            }
                        ]
                    }
                },
                {
                    "$type": "SCommandStmt",
                    "commands": [
                        {
                            "$type": "SStringExp",
                            "elements": [
                                {
                                    "$type": "STextStringExpElement",
                                    "text": "echo "
                                },
                                {
                                    "$type": "SExpStringExpElement",
                                    "exp": {
                                        "$type": "SIdentifierExp",
                                        "value": "sum",
                                        "typeArgs": []
                                    }
                                },
                                {
                                    "$type": "STextStringExpElement",
                                    "text": " Completed!"
                                }
                            ]
                        }
                    ]
                }
            ]
        }
    ]
})---";

    EXPECT_SYNTAX_EQ(oScript, expected);
}

TEST(ScriptParser, ParseEnumDecl)
{
    auto [buffer, lexer] = Prepare(UR"---(enum X
{
    First,
    Second(int i),
    Third
})---");

    auto oScript = ParseScript(&lexer);

    auto expected = R"---({
    "$type": "SScript",
    "elements": [
        {
            "$type": "SEnumDecl",
            "accessModifier": null,
            "name": "X",
            "typeParams": [],
            "elements": [
                {
                    "$type": "SEnumElemDecl",
                    "name": "First",
                    "memberVars": []
                },
                {
                    "$type": "SEnumElemDecl",
                    "name": "Second",
                    "memberVars": [
                        {
                            "$type": "SEnumElemMemberVarDecl",
                            "type": {
                                "$type": "SIdTypeExp",
                                "name": "int",
                                "typeArgs": []
                            },
                            "name": "i"
                        }
                    ]
                },
                {
                    "$type": "SEnumElemDecl",
                    "name": "Third",
                    "memberVars": []
                }
            ]
        }
    ]
})---";

    EXPECT_SYNTAX_EQ(oScript, expected);
}

TEST(ScriptParser, ParseFuncDecl)
{
    auto [buffer, lexer] = Prepare(UR"---(void Func(int x, string y, params int z) { int a = 0; })---");

    auto oScript = ParseScript(&lexer);

    auto expected = R"---({
    "$type": "SScript",
    "elements": [
        {
            "$type": "SGlobalFuncDecl",
            "accessModifier": null,
            "bSequence": false,
            "retType": {
                "$type": "SIdTypeExp",
                "name": "void",
                "typeArgs": []
            },
            "name": "Func",
            "typeParams": [],
            "parameters": [
                {
                    "$type": "SFuncParam",
                    "hasOut": false,
                    "hasParams": false,
                    "type": {
                        "$type": "SIdTypeExp",
                        "name": "int",
                        "typeArgs": []
                    },
                    "name": "x"
                },
                {
                    "$type": "SFuncParam",
                    "hasOut": false,
                    "hasParams": false,
                    "type": {
                        "$type": "SIdTypeExp",
                        "name": "string",
                        "typeArgs": []
                    },
                    "name": "y"
                },
                {
                    "$type": "SFuncParam",
                    "hasOut": false,
                    "hasParams": true,
                    "type": {
                        "$type": "SIdTypeExp",
                        "name": "int",
                        "typeArgs": []
                    },
                    "name": "z"
                }
            ],
            "body": [
                {
                    "$type": "SVarDeclStmt",
                    "varDecl": {
                        "$type": "SVarDecl",
                        "type": {
                            "$type": "SIdTypeExp",
                            "name": "int",
                            "typeArgs": []
                        },
                        "elements": [
                            {
                                "$type": "SVarDeclElement",
                                "varName": "a",
                                "initExp": {
                                    "$type": "SIntLiteralExp",
                                    "value": 0
                                }
                            }
                        ]
                    }
                }
            ]
        }
    ]
})---";

    EXPECT_SYNTAX_EQ(oScript, expected);
}

TEST(ScriptParser, ParseNamespaceDecl)
{
    auto [buffer, lexer] = Prepare(UR"---(namespace NS1
{
    namespace NS2.NS3
    {
        void F()
        {

        }
    }
})---");

    auto oScript = ParseScript(&lexer);

    auto expected = R"---({
    "$type": "SScript",
    "elements": [
        {
            "$type": "SNamespaceDecl",
            "names": [
                "NS1"
            ],
            "elements": [
                {
                    "$type": "SNamespaceDecl",
                    "names": [
                        "NS2",
                        "NS3"
                    ],
                    "elements": [
                        {
                            "$type": "SGlobalFuncDecl",
                            "accessModifier": null,
                            "bSequence": false,
                            "retType": {
                                "$type": "SIdTypeExp",
                                "name": "void",
                                "typeArgs": []
                            },
                            "name": "F",
                            "typeParams": [],
                            "parameters": [],
                            "body": []
                        }
                    ]
                }
            ]
        }
    ]
})---";

    EXPECT_SYNTAX_EQ(oScript, expected);
}

TEST(ScriptParser, ParseSimpleScript)
{
    auto [buffer, lexer] = Prepare(UR"---(void Main()
{
    @ls -al
})---");

    auto oScript = ParseScript(&lexer);

    auto expected = R"---({
    "$type": "SScript",
    "elements": [
        {
            "$type": "SGlobalFuncDecl",
            "accessModifier": null,
            "bSequence": false,
            "retType": {
                "$type": "SIdTypeExp",
                "name": "void",
                "typeArgs": []
            },
            "name": "Main",
            "typeParams": [],
            "parameters": [],
            "body": [
                {
                    "$type": "SCommandStmt",
                    "commands": [
                        {
                            "$type": "SStringExp",
                            "elements": [
                                {
                                    "$type": "STextStringExpElement",
                                    "text": "ls -al"
                                }
                            ]
                        }
                    ]
                }
            ]
        }
    ]
})---";

    EXPECT_SYNTAX_EQ(oScript, expected);
}

TEST(ScriptParser, ParseStructDecl)
{
    auto [buffer, lexer] = Prepare(UR"---(public struct S<T> : B, I
{
    int x1;
    public int x2;
    protected string y;
    private int z;

    public struct Nested<U> : B, I { int x; }

    static void Func<X>(string s) { }
    private seq int F2<T>() { yield 4; }
})---");

    auto oScript = ParseScript(&lexer);

    auto expected = R"---({
    "$type": "SScript",
    "elements": [
        {
            "$type": "SStructDecl",
            "accessModifier": "Public",
            "name": "S",
            "typeParams": [
                {
                    "$type": "STypeParam",
                    "name": "T"
                }
            ],
            "baseTypes": [
                {
                    "$type": "SIdTypeExp",
                    "name": "B",
                    "typeArgs": []
                },
                {
                    "$type": "SIdTypeExp",
                    "name": "I",
                    "typeArgs": []
                }
            ],
            "memberDecls": [
                {
                    "$type": "SStructMemberVarDecl",
                    "accessModifier": null,
                    "varType": {
                        "$type": "SIdTypeExp",
                        "name": "int",
                        "typeArgs": []
                    },
                    "varNames": [
                        "x1"
                    ]
                },
                {
                    "$type": "SStructMemberVarDecl",
                    "accessModifier": "Public",
                    "varType": {
                        "$type": "SIdTypeExp",
                        "name": "int",
                        "typeArgs": []
                    },
                    "varNames": [
                        "x2"
                    ]
                },
                {
                    "$type": "SStructMemberVarDecl",
                    "accessModifier": "Protected",
                    "varType": {
                        "$type": "SIdTypeExp",
                        "name": "string",
                        "typeArgs": []
                    },
                    "varNames": [
                        "y"
                    ]
                },
                {
                    "$type": "SStructMemberVarDecl",
                    "accessModifier": "Private",
                    "varType": {
                        "$type": "SIdTypeExp",
                        "name": "int",
                        "typeArgs": []
                    },
                    "varNames": [
                        "z"
                    ]
                },
                {
                    "$type": "SStructDecl",
                    "accessModifier": "Public",
                    "name": "Nested",
                    "typeParams": [
                        {
                            "$type": "STypeParam",
                            "name": "U"
                        }
                    ],
                    "baseTypes": [
                        {
                            "$type": "SIdTypeExp",
                            "name": "B",
                            "typeArgs": []
                        },
                        {
                            "$type": "SIdTypeExp",
                            "name": "I",
                            "typeArgs": []
                        }
                    ],
                    "memberDecls": [
                        {
                            "$type": "SStructMemberVarDecl",
                            "accessModifier": null,
                            "varType": {
                                "$type": "SIdTypeExp",
                                "name": "int",
                                "typeArgs": []
                            },
                            "varNames": [
                                "x"
                            ]
                        }
                    ]
                },
                {
                    "$type": "SStructMemberFuncDecl",
                    "accessModifier": null,
                    "bStatic": true,
                    "bSequence": false,
                    "retType": {
                        "$type": "SIdTypeExp",
                        "name": "void",
                        "typeArgs": []
                    },
                    "name": "Func",
                    "typeParams": [
                        {
                            "$type": "STypeParam",
                            "name": "X"
                        }
                    ],
                    "parameters": [
                        {
                            "$type": "SFuncParam",
                            "hasOut": false,
                            "hasParams": false,
                            "type": {
                                "$type": "SIdTypeExp",
                                "name": "string",
                                "typeArgs": []
                            },
                            "name": "s"
                        }
                    ],
                    "body": []
                },
                {
                    "$type": "SStructMemberFuncDecl",
                    "accessModifier": "Private",
                    "bStatic": false,
                    "bSequence": true,
                    "retType": {
                        "$type": "SIdTypeExp",
                        "name": "int",
                        "typeArgs": []
                    },
                    "name": "F2",
                    "typeParams": [
                        {
                            "$type": "STypeParam",
                            "name": "T"
                        }
                    ],
                    "parameters": [],
                    "body": [
                        {
                            "$type": "SYieldStmt",
                            "value": {
                                "$type": "SIntLiteralExp",
                                "value": 4
                            }
                        }
                    ]
                }
            ]
        }
    ]
})---";

    EXPECT_SYNTAX_EQ(oScript, expected);
}

