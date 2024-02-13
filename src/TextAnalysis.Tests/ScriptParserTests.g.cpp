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
    "$type": "ScriptSyntax",
    "elements": [
        {
            "$type": "GlobalFuncDeclSyntax",
            "accessModifier": null,
            "bSequence": false,
            "retType": {
                "$type": "IdTypeExpSyntax",
                "name": "void",
                "typeArgs": []
            },
            "name": "Main",
            "typeParams": [],
            "parameters": [],
            "body": [
                {
                    "$type": "VarDeclStmtSyntax",
                    "varDecl": {
                        "$type": "VarDeclSyntax",
                        "type": {
                            "$type": "IdTypeExpSyntax",
                            "name": "int",
                            "typeArgs": []
                        },
                        "elements": [
                            {
                                "$type": "VarDeclSyntaxElement",
                                "varName": "sum",
                                "initExp": {
                                    "$type": "IntLiteralExpSyntax",
                                    "value": 0
                                }
                            }
                        ]
                    }
                },
                {
                    "$type": "ForStmtSyntax",
                    "initializer": {
                        "$type": "VarDeclForStmtInitializerSyntax",
                        "varDecl": {
                            "$type": "VarDeclSyntax",
                            "type": {
                                "$type": "IdTypeExpSyntax",
                                "name": "int",
                                "typeArgs": []
                            },
                            "elements": [
                                {
                                    "$type": "VarDeclSyntaxElement",
                                    "varName": "i",
                                    "initExp": {
                                        "$type": "IntLiteralExpSyntax",
                                        "value": 0
                                    }
                                }
                            ]
                        }
                    },
                    "cond": {
                        "$type": "BinaryOpExpSyntax",
                        "kind": "LessThan",
                        "operand0": {
                            "$type": "IdentifierExpSyntax",
                            "value": "i",
                            "typeArgs": []
                        },
                        "operand1": {
                            "$type": "IntLiteralExpSyntax",
                            "value": 5
                        }
                    },
                    "cont": {
                        "$type": "UnaryOpExpSyntax",
                        "kind": "PostfixInc",
                        "operand": {
                            "$type": "IdentifierExpSyntax",
                            "value": "i",
                            "typeArgs": []
                        }
                    },
                    "body": {
                        "$type": "BlockEmbeddableStmtSyntax",
                        "stmts": [
                            {
                                "$type": "IfStmtSyntax",
                                "cond": {
                                    "$type": "BinaryOpExpSyntax",
                                    "kind": "Equal",
                                    "operand0": {
                                        "$type": "BinaryOpExpSyntax",
                                        "kind": "Modulo",
                                        "operand0": {
                                            "$type": "IdentifierExpSyntax",
                                            "value": "i",
                                            "typeArgs": []
                                        },
                                        "operand1": {
                                            "$type": "IntLiteralExpSyntax",
                                            "value": 2
                                        }
                                    },
                                    "operand1": {
                                        "$type": "IntLiteralExpSyntax",
                                        "value": 0
                                    }
                                },
                                "body": {
                                    "$type": "SingleEmbeddableStmtSyntax",
                                    "stmt": {
                                        "$type": "ExpStmtSyntax",
                                        "exp": {
                                            "$type": "BinaryOpExpSyntax",
                                            "kind": "Assign",
                                            "operand0": {
                                                "$type": "IdentifierExpSyntax",
                                                "value": "sum",
                                                "typeArgs": []
                                            },
                                            "operand1": {
                                                "$type": "BinaryOpExpSyntax",
                                                "kind": "Add",
                                                "operand0": {
                                                    "$type": "IdentifierExpSyntax",
                                                    "value": "sum",
                                                    "typeArgs": []
                                                },
                                                "operand1": {
                                                    "$type": "IdentifierExpSyntax",
                                                    "value": "i",
                                                    "typeArgs": []
                                                }
                                            }
                                        }
                                    }
                                },
                                "elseBody": {
                                    "$type": "SingleEmbeddableStmtSyntax",
                                    "stmt": {
                                        "$type": "CommandStmtSyntax",
                                        "commands": [
                                            {
                                                "$type": "StringExpSyntax",
                                                "elements": [
                                                    {
                                                        "$type": "TextStringExpSyntaxElement",
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
                    "$type": "CommandStmtSyntax",
                    "commands": [
                        {
                            "$type": "StringExpSyntax",
                            "elements": [
                                {
                                    "$type": "TextStringExpSyntaxElement",
                                    "text": "echo "
                                },
                                {
                                    "$type": "ExpStringExpSyntaxElement",
                                    "exp": {
                                        "$type": "IdentifierExpSyntax",
                                        "value": "sum",
                                        "typeArgs": []
                                    }
                                },
                                {
                                    "$type": "TextStringExpSyntaxElement",
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
    "$type": "ScriptSyntax",
    "elements": [
        {
            "$type": "TypeDeclScriptSyntaxElement",
            "typeDecl": {
                "$type": "EnumDeclSyntax",
                "accessModifier": null,
                "name": "X",
                "typeParams": [],
                "elements": [
                    {
                        "$type": "EnumElemDeclSyntax",
                        "name": "First",
                        "memberVars": []
                    },
                    {
                        "$type": "EnumElemDeclSyntax",
                        "name": "Second",
                        "memberVars": [
                            {
                                "$type": "EnumElemMemberVarDeclSyntax",
                                "type": {
                                    "$type": "IdTypeExpSyntax",
                                    "name": "int",
                                    "typeArgs": []
                                },
                                "name": "i"
                            }
                        ]
                    },
                    {
                        "$type": "EnumElemDeclSyntax",
                        "name": "Third",
                        "memberVars": []
                    }
                ]
            }
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
    "$type": "ScriptSyntax",
    "elements": [
        {
            "$type": "GlobalFuncDeclSyntax",
            "accessModifier": null,
            "bSequence": false,
            "retType": {
                "$type": "IdTypeExpSyntax",
                "name": "void",
                "typeArgs": []
            },
            "name": "Func",
            "typeParams": [],
            "parameters": [
                {
                    "$type": "FuncParamSyntax",
                    "hasOut": false,
                    "hasParams": false,
                    "type": {
                        "$type": "IdTypeExpSyntax",
                        "name": "int",
                        "typeArgs": []
                    },
                    "name": "x"
                },
                {
                    "$type": "FuncParamSyntax",
                    "hasOut": false,
                    "hasParams": false,
                    "type": {
                        "$type": "IdTypeExpSyntax",
                        "name": "string",
                        "typeArgs": []
                    },
                    "name": "y"
                },
                {
                    "$type": "FuncParamSyntax",
                    "hasOut": false,
                    "hasParams": true,
                    "type": {
                        "$type": "IdTypeExpSyntax",
                        "name": "int",
                        "typeArgs": []
                    },
                    "name": "z"
                }
            ],
            "body": [
                {
                    "$type": "VarDeclStmtSyntax",
                    "varDecl": {
                        "$type": "VarDeclSyntax",
                        "type": {
                            "$type": "IdTypeExpSyntax",
                            "name": "int",
                            "typeArgs": []
                        },
                        "elements": [
                            {
                                "$type": "VarDeclSyntaxElement",
                                "varName": "a",
                                "initExp": {
                                    "$type": "IntLiteralExpSyntax",
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
    "$type": "ScriptSyntax",
    "elements": [
        {
            "$type": "NamespaceDeclSyntax",
            "names": [
                "NS1"
            ],
            "elements": [
                {
                    "$type": "NamespaceDeclSyntax",
                    "names": [
                        "NS2",
                        "NS3"
                    ],
                    "elements": [
                        {   
                            "$type": "GlobalFuncDeclSyntax",
                            "accessModifier": null,
                            "bSequence": false,
                            "retType": {
                                "$type": "IdTypeExpSyntax",
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
    "$type": "ScriptSyntax",
    "elements": [
        {   
            "$type": "GlobalFuncDeclSyntax",
            "accessModifier": null,
            "bSequence": false,
            "retType": {
                "$type": "IdTypeExpSyntax",
                "name": "void",
                "typeArgs": []
            },
            "name": "Main",
            "typeParams": [],
            "parameters": [],
            "body": [
                {
                    "$type": "CommandStmtSyntax",
                    "commands": [
                        {
                            "$type": "StringExpSyntax",
                            "elements": [
                                {
                                    "$type": "TextStringExpSyntaxElement",
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
    "$type": "ScriptSyntax",
    "elements": [
        {
            "$type": "StructDeclSyntax",
            "accessModifier": "Public",
            "name": "S",
            "typeParams": [
                {
                    "$type": "TypeParamSyntax",
                    "name": "T"
                }
            ],
            "baseTypes": [
                {
                    "$type": "IdTypeExpSyntax",
                    "name": "B",
                    "typeArgs": []
                },
                {
                    "$type": "IdTypeExpSyntax",
                    "name": "I",
                    "typeArgs": []
                }
            ],
            "memberDecls": [
                {
                    "$type": "StructMemberVarDeclSyntax",
                    "accessModifier": null,
                    "varType": {
                        "$type": "IdTypeExpSyntax",
                        "name": "int",
                        "typeArgs": []
                    },
                    "varNames": [
                        "x1"
                    ]
                },
                {
                    "$type": "StructMemberVarDeclSyntax",
                    "accessModifier": "Public",
                    "varType": {
                        "$type": "IdTypeExpSyntax",
                        "name": "int",
                        "typeArgs": []
                    },
                    "varNames": [
                        "x2"
                    ]
                },
                {
                    "$type": "StructMemberVarDeclSyntax",
                    "accessModifier": "Protected",
                    "varType": {
                        "$type": "IdTypeExpSyntax",
                        "name": "string",
                        "typeArgs": []
                    },
                    "varNames": [
                        "y"
                    ]
                },
                {
                    "$type": "StructMemberVarDeclSyntax",
                    "accessModifier": "Private",
                    "varType": {
                        "$type": "IdTypeExpSyntax",
                        "name": "int",
                        "typeArgs": []
                    },
                    "varNames": [
                        "z"
                    ]
                },
                {
                    "$type": "StructMemberTypeDeclSyntax",
                    "typeDecl": {
                        "$type": "StructDeclSyntax",
                        "accessModifier": "Public",
                        "name": "Nested",
                        "typeParams": [
                            {
                                "$type": "TypeParamSyntax",
                                "name": "U"
                            }
                        ],
                        "baseTypes": [
                            {
                                "$type": "IdTypeExpSyntax",
                                "name": "B",
                                "typeArgs": []
                            },
                            {
                                "$type": "IdTypeExpSyntax",
                                "name": "I",
                                "typeArgs": []
                            }
                        ],
                        "memberDecls": [
                            {
                                "$type": "StructMemberVarDeclSyntax",
                                "accessModifier": null,
                                "varType": {
                                    "$type": "IdTypeExpSyntax",
                                    "name": "int",
                                    "typeArgs": []
                                },
                                "varNames": [
                                    "x"
                                ]
                            }
                        ]
                    }
                },
                {
                    "$type": "StructMemberFuncDeclSyntax",
                    "accessModifier": null,
                    "bStatic": true,
                    "bSequence": false,
                    "retType": {
                        "$type": "IdTypeExpSyntax",
                        "name": "void",
                        "typeArgs": []
                    },
                    "name": "Func",
                    "typeParams": [
                        {
                            "$type": "TypeParamSyntax",
                            "name": "X"
                        }
                    ],
                    "parameters": [
                        {
                            "$type": "FuncParamSyntax",
                            "hasOut": false,
                            "hasParams": false,
                            "type": {
                                "$type": "IdTypeExpSyntax",
                                "name": "string",
                                "typeArgs": []
                            },
                            "name": "s"
                        }
                    ],
                    "body": []
                },
                {
                    "$type": "StructMemberFuncDeclSyntax",
                    "accessModifier": "Private",
                    "bStatic": false,
                    "bSequence": true,
                    "retType": {
                        "$type": "IdTypeExpSyntax",
                        "name": "int",
                        "typeArgs": []
                    },
                    "name": "F2",
                    "typeParams": [
                        {
                            "$type": "TypeParamSyntax",
                            "name": "T"
                        }
                    ],
                    "parameters": [],
                    "body": [
                        {
                            "$type": "YieldStmtSyntax",
                            "value": {
                                "$type": "IntLiteralExpSyntax",
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

