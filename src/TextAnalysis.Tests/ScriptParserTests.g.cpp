#include <gtest/gtest.h>

#include "Syntax/Syntax.h"
#include "TextAnalysis/ScriptParser.h"

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
    SFactory factory;

    auto* script = ParseScript(&lexer, factory);

    auto expected = R"---({
    "$type": "SScript",
    "elements": [
        {
            "$type": "SGlobalFuncDecl",
            "accessModifier": null,
            "bSequence": false,
            "funcRet": {
                "$type": "SFuncReturn_Normal",
                "type": {
                    "$type": "STypeExp_Id",
                    "name": "void",
                    "typeArgs": []
                }
            },
            "name": "Main",
            "typeParams": [],
            "parameters": [],
            "body": [
                {
                    "$type": "SStmt_VarDecl",
                    "varDecl": {
                        "$type": "SVarDecl",
                        "type": {
                            "$type": "SVarDeclType_Normal",
                            "typeExp": {
                                "$type": "STypeExp_Id",
                                "name": "int",
                                "typeArgs": []
                            }
                        },
                        "elements": [
                            {
                                "$type": "SVarDeclElement",
                                "varName": "sum",
                                "init": {
                                    "$type": "SVarDeclElementInit_Exp",
                                    "exp": {
                                        "$type": "SExp_IntLiteral",
                                        "value": 0
                                    }
                                }
                            }
                        ]
                    }
                },
                {
                    "$type": "SStmt_For",
                    "initializer": {
                        "$type": "SForStmtInitializer_VarDecl",
                        "varDecl": {
                            "$type": "SVarDecl",
                            "type": {
                                "$type": "SVarDeclType_Normal",
                                "typeExp": {
                                    "$type": "STypeExp_Id",
                                    "name": "int",
                                    "typeArgs": []
                                }
                            },
                            "elements": [
                                {
                                    "$type": "SVarDeclElement",
                                    "varName": "i",
                                    "init": {
                                        "$type": "SVarDeclElementInit_Exp",
                                        "exp": {
                                            "$type": "SExp_IntLiteral",
                                            "value": 0
                                        }
                                    }
                                }
                            ]
                        }
                    },
                    "cond": {
                        "$type": "SExp_BinaryOp",
                        "kind": "LessThan",
                        "operand0": {
                            "$type": "SExp_Identifier",
                            "value": "i",
                            "typeArgs": []
                        },
                        "operand1": {
                            "$type": "SExp_IntLiteral",
                            "value": 5
                        }
                    },
                    "cont": {
                        "$type": "SExp_UnaryOp",
                        "kind": "PostfixInc",
                        "operand": {
                            "$type": "SExp_Identifier",
                            "value": "i",
                            "typeArgs": []
                        }
                    },
                    "body": {
                        "$type": "SEmbeddableStmt_Block",
                        "stmts": [
                            {
                                "$type": "SStmt_If",
                                "cond": {
                                    "$type": "SExp_BinaryOp",
                                    "kind": "Equal",
                                    "operand0": {
                                        "$type": "SExp_BinaryOp",
                                        "kind": "Modulo",
                                        "operand0": {
                                            "$type": "SExp_Identifier",
                                            "value": "i",
                                            "typeArgs": []
                                        },
                                        "operand1": {
                                            "$type": "SExp_IntLiteral",
                                            "value": 2
                                        }
                                    },
                                    "operand1": {
                                        "$type": "SExp_IntLiteral",
                                        "value": 0
                                    }
                                },
                                "body": {
                                    "$type": "SEmbeddableStmt_Single",
                                    "stmt": {
                                        "$type": "SStmt_Exp",
                                        "exp": {
                                            "$type": "SExp_BinaryOp",
                                            "kind": "Assign",
                                            "operand0": {
                                                "$type": "SExp_Identifier",
                                                "value": "sum",
                                                "typeArgs": []
                                            },
                                            "operand1": {
                                                "$type": "SExp_BinaryOp",
                                                "kind": "Add",
                                                "operand0": {
                                                    "$type": "SExp_Identifier",
                                                    "value": "sum",
                                                    "typeArgs": []
                                                },
                                                "operand1": {
                                                    "$type": "SExp_Identifier",
                                                    "value": "i",
                                                    "typeArgs": []
                                                }
                                            }
                                        }
                                    }
                                },
                                "elseBody": {
                                    "$type": "SEmbeddableStmt_Single",
                                    "stmt": {
                                        "$type": "SStmt_Command",
                                        "commands": [
                                            {
                                                "$type": "SExp_String",
                                                "elements": [
                                                    {
                                                        "$type": "SStringExpElement_Text",
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
                    "$type": "SStmt_Command",
                    "commands": [
                        {
                            "$type": "SExp_String",
                            "elements": [
                                {
                                    "$type": "SStringExpElement_Text",
                                    "text": "echo "
                                },
                                {
                                    "$type": "SStringExpElement_Exp",
                                    "exp": {
                                        "$type": "SExp_Identifier",
                                        "value": "sum",
                                        "typeArgs": []
                                    }
                                },
                                {
                                    "$type": "SStringExpElement_Text",
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

    EXPECT_SYNTAX_EQ(script, expected);
}

TEST(ScriptParser, ParseEnumDecl)
{
    auto [buffer, lexer] = Prepare(UR"---(enum X
{
    First,
    Second(int i),
    Third
})---");
    SFactory factory;

    auto* script = ParseScript(&lexer, factory);

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
                    "vars": []
                },
                {
                    "$type": "SEnumElemDecl",
                    "name": "Second",
                    "vars": [
                        {
                            "$type": "SEnumElemVarDecl",
                            "type": {
                                "$type": "STypeExp_Id",
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
                    "vars": []
                }
            ]
        }
    ]
})---";

    EXPECT_SYNTAX_EQ(script, expected);
}

TEST(ScriptParser, ParseExtend_Basic)
{
    auto [buffer, lexer] = Prepare(UR"---(extend S : MyTrait
{
    void Func()
    {
    }
})---");
    SFactory factory;

    auto* script = ParseScript(&lexer, factory);

    auto expected = R"---({
    "$type": "SScript",
    "elements": [
        {
            "$type": "SExtendDecl",
            "accessModifier": null,
            "name": "S",
            "trait": {
                "$type": "STypeExp_Id",
                "name": "MyTrait",
                "typeArgs": []
            },
            "memberDecls": [
                {
                    "$type": "SExtendFuncDecl",
                    "bStatic": false,
                    "funcReturn": {
                        "$type": "SFuncReturn_Normal",
                        "type": {
                            "$type": "STypeExp_Id",
                            "name": "void",
                            "typeArgs": []
                        }
                    },
                    "name": "Func",
                    "typeParams": [],
                    "parameters": [],
                    "body": []
                }
            ]
        }
    ]
})---";

    EXPECT_SYNTAX_EQ(script, expected);
}

TEST(ScriptParser, ParseExtend_Complex)
{
    auto [buffer, lexer] = Prepare(UR"---(public extend S : MyTrait<T, U>
{
    some T Func([in]U& u)
    {

    }
}
)---");
    SFactory factory;

    auto* script = ParseScript(&lexer, factory);

    auto expected = R"---({
    "$type": "SScript",
    "elements": [
        {
            "$type": "SExtendDecl",
            "accessModifier": "Public",
            "name": "S",
            "trait": {
                "$type": "STypeExp_Id",
                "name": "MyTrait",
                "typeArgs": [
                    {
                        "$type": "STypeExp_Id",
                        "name": "T",
                        "typeArgs": []
                    },
                    {
                        "$type": "STypeExp_Id",
                        "name": "U",
                        "typeArgs": []
                    }
                ]
            },
            "memberDecls": [
                {
                    "$type": "SExtendFuncDecl",
                    "bStatic": false,
                    "funcReturn": {
                        "$type": "SFuncReturn_Opaque",
                        "type": {
                            "$type": "STypeExp_Id",
                            "name": "T",
                            "typeArgs": []
                        }
                    },
                    "name": "Func",
                    "typeParams": [],
                    "parameters": [
                        {
                            "$type": "SFuncParam",
                            "o_modifier": "In",
                            "bRef": true,
                            "type": {
                                "$type": "STypeExp_Id",
                                "name": "U",
                                "typeArgs": []
                            },
                            "name": "u"
                        }
                    ],
                    "body": []
                }
            ]
        }
    ]
})---";

    EXPECT_SYNTAX_EQ(script, expected);
}

TEST(ScriptParser, ParseExtend_Empty)
{
    auto [buffer, lexer] = Prepare(UR"---(extend S : MyTrait
{
}
)---");
    SFactory factory;

    auto* script = ParseScript(&lexer, factory);

    auto expected = R"---({
    "$type": "SScript",
    "elements": [
        {
            "$type": "SExtendDecl",
            "accessModifier": null,
            "name": "S",
            "trait": {
                "$type": "STypeExp_Id",
                "name": "MyTrait",
                "typeArgs": []
            },
            "memberDecls": []
        }
    ]
})---";

    EXPECT_SYNTAX_EQ(script, expected);
}

TEST(ScriptParser, ParseFuncDecl)
{
    auto [buffer, lexer] = Prepare(UR"---(void Func(int x, string y, [params] int z) { int a = 0; })---");
    SFactory factory;

    auto* script = ParseScript(&lexer, factory);

    auto expected = R"---({
    "$type": "SScript",
    "elements": [
        {
            "$type": "SGlobalFuncDecl",
            "accessModifier": null,
            "bSequence": false,
            "funcRet": {
                "$type": "SFuncReturn_Normal",
                "type": {
                    "$type": "STypeExp_Id",
                    "name": "void",
                    "typeArgs": []
                }
            },
            "name": "Func",
            "typeParams": [],
            "parameters": [
                {
                    "$type": "SFuncParam",
                    "o_modifier": null,
                    "bRef": false,
                    "type": {
                        "$type": "STypeExp_Id",
                        "name": "int",
                        "typeArgs": []
                    },
                    "name": "x"
                },
                {
                    "$type": "SFuncParam",
                    "o_modifier": null,
                    "bRef": false,
                    "type": {
                        "$type": "STypeExp_Id",
                        "name": "string",
                        "typeArgs": []
                    },
                    "name": "y"
                },
                {
                    "$type": "SFuncParam",
                    "o_modifier": "Params",
                    "bRef": false,
                    "type": {
                        "$type": "STypeExp_Id",
                        "name": "int",
                        "typeArgs": []
                    },
                    "name": "z"
                }
            ],
            "body": [
                {
                    "$type": "SStmt_VarDecl",
                    "varDecl": {
                        "$type": "SVarDecl",
                        "type": {
                            "$type": "SVarDeclType_Normal",
                            "typeExp": {
                                "$type": "STypeExp_Id",
                                "name": "int",
                                "typeArgs": []
                            }
                        },
                        "elements": [
                            {
                                "$type": "SVarDeclElement",
                                "varName": "a",
                                "init": {
                                    "$type": "SVarDeclElementInit_Exp",
                                    "exp": {
                                        "$type": "SExp_IntLiteral",
                                        "value": 0
                                    }
                                }
                            }
                        ]
                    }
                }
            ]
        }
    ]
})---";

    EXPECT_SYNTAX_EQ(script, expected);
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
    SFactory factory;

    auto* script = ParseScript(&lexer, factory);

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
                            "funcRet": {
                                "$type": "SFuncReturn_Normal",
                                "type": {
                                    "$type": "STypeExp_Id",
                                    "name": "void",
                                    "typeArgs": []
                                }
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

    EXPECT_SYNTAX_EQ(script, expected);
}

TEST(ScriptParser, ParseSimpleScript)
{
    auto [buffer, lexer] = Prepare(UR"---(void Main()
{
    @ls -al
})---");
    SFactory factory;

    auto* script = ParseScript(&lexer, factory);

    auto expected = R"---({
    "$type": "SScript",
    "elements": [
        {
            "$type": "SGlobalFuncDecl",
            "accessModifier": null,
            "bSequence": false,
            "funcRet": {
                "$type": "SFuncReturn_Normal",
                "type": {
                    "$type": "STypeExp_Id",
                    "name": "void",
                    "typeArgs": []
                }
            },
            "name": "Main",
            "typeParams": [],
            "parameters": [],
            "body": [
                {
                    "$type": "SStmt_Command",
                    "commands": [
                        {
                            "$type": "SExp_String",
                            "elements": [
                                {
                                    "$type": "SStringExpElement_Text",
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

    EXPECT_SYNTAX_EQ(script, expected);
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
    SFactory factory;

    auto* script = ParseScript(&lexer, factory);

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
                    "$type": "STypeExp_Id",
                    "name": "B",
                    "typeArgs": []
                },
                {
                    "$type": "STypeExp_Id",
                    "name": "I",
                    "typeArgs": []
                }
            ],
            "memberDecls": [
                {
                    "$type": "SStructVarDecl",
                    "accessModifier": null,
                    "varType": {
                        "$type": "STypeExp_Id",
                        "name": "int",
                        "typeArgs": []
                    },
                    "varNames": [
                        "x1"
                    ]
                },
                {
                    "$type": "SStructVarDecl",
                    "accessModifier": "Public",
                    "varType": {
                        "$type": "STypeExp_Id",
                        "name": "int",
                        "typeArgs": []
                    },
                    "varNames": [
                        "x2"
                    ]
                },
                {
                    "$type": "SStructVarDecl",
                    "accessModifier": "Protected",
                    "varType": {
                        "$type": "STypeExp_Id",
                        "name": "string",
                        "typeArgs": []
                    },
                    "varNames": [
                        "y"
                    ]
                },
                {
                    "$type": "SStructVarDecl",
                    "accessModifier": "Private",
                    "varType": {
                        "$type": "STypeExp_Id",
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
                            "$type": "STypeExp_Id",
                            "name": "B",
                            "typeArgs": []
                        },
                        {
                            "$type": "STypeExp_Id",
                            "name": "I",
                            "typeArgs": []
                        }
                    ],
                    "memberDecls": [
                        {
                            "$type": "SStructVarDecl",
                            "accessModifier": null,
                            "varType": {
                                "$type": "STypeExp_Id",
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
                    "$type": "SStructFuncDecl",
                    "accessModifier": null,
                    "bStatic": true,
                    "bSequence": false,
                    "funcRet": {
                        "$type": "SFuncReturn_Normal",
                        "type": {
                            "$type": "STypeExp_Id",
                            "name": "void",
                            "typeArgs": []
                        }
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
                            "o_modifier": null,
                            "bRef": false,
                            "type": {
                                "$type": "STypeExp_Id",
                                "name": "string",
                                "typeArgs": []
                            },
                            "name": "s"
                        }
                    ],
                    "body": []
                },
                {
                    "$type": "SStructFuncDecl",
                    "accessModifier": "Private",
                    "bStatic": false,
                    "bSequence": true,
                    "funcRet": {
                        "$type": "SFuncReturn_Normal",
                        "type": {
                            "$type": "STypeExp_Id",
                            "name": "int",
                            "typeArgs": []
                        }
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
                            "$type": "SStmt_Yield",
                            "value": {
                                "$type": "SExp_IntLiteral",
                                "value": 4
                            }
                        }
                    ]
                }
            ]
        }
    ]
})---";

    EXPECT_SYNTAX_EQ(script, expected);
}

TEST(ScriptParser, ParseTrait_Basic)
{
    auto [buffer, lexer] = Prepare(UR"---(trait MyTrait
{
    void Func();
})---");
    SFactory factory;

    auto* script = ParseScript(&lexer, factory);

    auto expected = R"---({
    "$type": "SScript",
    "elements": [
        {
            "$type": "STraitDecl",
            "accessModifier": null,
            "name": "MyTrait",
            "typeParams": [],
            "memberDecls": [
                {
                    "$type": "STraitFuncDecl",
                    "bStatic": false,
                    "funcRet": {
                        "$type": "SFuncReturn_Normal",
                        "type": {
                            "$type": "STypeExp_Id",
                            "name": "void",
                            "typeArgs": []
                        }
                    },
                    "name": "Func",
                    "typeParams": [],
                    "parameters": []
                }
            ]
        }
    ]
})---";

    EXPECT_SYNTAX_EQ(script, expected);
}

TEST(ScriptParser, ParseTrait_Complex)
{
    auto [buffer, lexer] = Prepare(UR"---(
trait MyTrait<T, U>
{
    some T Func([in]U& u);
}
)---");
    SFactory factory;

    auto* script = ParseScript(&lexer, factory);

    auto expected = R"---({
    "$type": "SScript",
    "elements": [
        {
            "$type": "STraitDecl",
            "accessModifier": null,
            "name": "MyTrait",
            "typeParams": [
                {
                    "$type": "STypeParam",
                    "name": "T"
                },
                {
                    "$type": "STypeParam",
                    "name": "U"
                }
            ],
            "memberDecls": [
                {
                    "$type": "STraitFuncDecl",
                    "bStatic": false,
                    "funcRet": {
                        "$type": "SFuncReturn_Opaque",
                        "type": {
                            "$type": "STypeExp_Id",
                            "name": "T",
                            "typeArgs": []
                        }
                    },
                    "name": "Func",
                    "typeParams": [],
                    "parameters": [
                        {
                            "$type": "SFuncParam",
                            "o_modifier": "In",
                            "bRef": true,
                            "type": {
                                "$type": "STypeExp_Id",
                                "name": "U",
                                "typeArgs": []
                            },
                            "name": "u"
                        }
                    ]
                }
            ]
        }
    ]
})---";

    EXPECT_SYNTAX_EQ(script, expected);
}

TEST(ScriptParser, ParseTrait_Empty)
{
    auto [buffer, lexer] = Prepare(UR"---(trait Empty { })---");
    SFactory factory;

    auto* script = ParseScript(&lexer, factory);

    auto expected = R"---({
    "$type": "SScript",
    "elements": [
        {
            "$type": "STraitDecl",
            "accessModifier": null,
            "name": "Empty",
            "typeParams": [],
            "memberDecls": []
        }
    ]
})---";

    EXPECT_SYNTAX_EQ(script, expected);
}

