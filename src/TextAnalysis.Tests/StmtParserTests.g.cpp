#include <gtest/gtest.h>

#include "Syntax/Syntax.h"
#include "TextAnalysis/StmtParser.h"

#include "TestMisc.h"

using namespace std;
using namespace Citron;

TEST(StmtParser, ParseBlankStmt)
{
    auto [buffer, lexer] = Prepare(UR"---(  ;  )---");
    SFactory factory;

    auto* stmt = ParseStmt(&lexer, factory);

    auto expected = R"---({
    "$type": "SStmt_Blank"
})---";

    EXPECT_SYNTAX_EQ(stmt, expected);
}

TEST(StmtParser, ParseBlockCommandStmt)
{
    auto [buffer, lexer] = Prepare(UR"---(@{ 
    echo ${ a } bbb   
xxx
})---");
    SFactory factory;

    auto* stmt = ParseStmt(&lexer, factory);

    auto expected = R"---({
    "$type": "SStmt_Command",
    "commands": [
        {
            "$type": "SExp_String",
            "elements": [
                {
                    "$type": "SStringExpElement_Text",
                    "text": "    echo "
                },
                {
                    "$type": "SStringExpElement_Exp",
                    "exp": {
                        "$type": "SExp_Identifier",
                        "value": "a",
                        "typeArgs": []
                    }
                },
                {
                    "$type": "SStringExpElement_Text",
                    "text": " bbb   "
                }
            ]
        },
        {
            "$type": "SExp_String",
            "elements": [
                {
                    "$type": "SStringExpElement_Text",
                    "text": "xxx"
                }
            ]
        }
    ]
})---";

    EXPECT_SYNTAX_EQ(stmt, expected);
}

TEST(StmtParser, ParseBlockStmt)
{
    auto [buffer, lexer] = Prepare(UR"---({ { } { ; } ; })---");
    SFactory factory;

    auto* stmt = ParseStmt(&lexer, factory);

    auto expected = R"---({
    "$type": "SStmt_Block",
    "stmts": [
        {
            "$type": "SStmt_Block",
            "stmts": []
        },
        {
            "$type": "SStmt_Block",
            "stmts": [
                {
                    "$type": "SStmt_Blank"
                }
            ]
        },
        {
            "$type": "SStmt_Blank"
        }
    ]
})---";

    EXPECT_SYNTAX_EQ(stmt, expected);
}

TEST(StmtParser, ParseBoxVarDeclStmt)
{
    auto [buffer, lexer] = Prepare(UR"---(box int* p;)---");
    SFactory factory;

    auto* stmt = ParseStmt(&lexer, factory);

    auto expected = R"---({
    "$type": "SStmt_VarDecl",
    "varDecl": {
        "$type": "SVarDecl",
        "type": {
            "$type": "STypeExp_Box",
            "innerType": {
                "$type": "STypeExp_Id",
                "name": "int",
                "typeArgs": []
            }
        },
        "elements": [
            {
                "$type": "SVarDeclElement",
                "varName": "p",
                "initExp": null
            }
        ]
    }
})---";

    EXPECT_SYNTAX_EQ(stmt, expected);
}

TEST(StmtParser, ParseBreakStmt)
{
    auto [buffer, lexer] = Prepare(UR"---(break;)---");
    SFactory factory;

    auto* stmt = ParseStmt(&lexer, factory);

    auto expected = R"---({
    "$type": "SStmt_Break"
})---";

    EXPECT_SYNTAX_EQ(stmt, expected);
}

TEST(StmtParser, ParseContinueStmt)
{
    auto [buffer, lexer] = Prepare(UR"---(continue;)---");
    SFactory factory;

    auto* stmt = ParseStmt(&lexer, factory);

    auto expected = R"---({
    "$type": "SStmt_Continue"
})---";

    EXPECT_SYNTAX_EQ(stmt, expected);
}

TEST(StmtParser, ParseDirectiveStmt)
{
    auto [buffer, lexer] = Prepare(UR"---(`notnull(a);)---");
    SFactory factory;

    auto* stmt = ParseStmt(&lexer, factory);

    auto expected = R"---({
    "$type": "SStmt_Directive",
    "name": "notnull",
    "args": [
        {
            "$type": "SExp_Identifier",
            "value": "a",
            "typeArgs": []
        }
    ]
})---";

    EXPECT_SYNTAX_EQ(stmt, expected);
}

TEST(StmtParser, ParseExpStmt)
{
    auto [buffer, lexer] = Prepare(UR"---(a = b * c(1);)---");
    SFactory factory;

    auto* stmt = ParseStmt(&lexer, factory);

    auto expected = R"---({
    "$type": "SStmt_Exp",
    "exp": {
        "$type": "SExp_BinaryOp",
        "kind": "Assign",
        "operand0": {
            "$type": "SExp_Identifier",
            "value": "a",
            "typeArgs": []
        },
        "operand1": {
            "$type": "SExp_BinaryOp",
            "kind": "Multiply",
            "operand0": {
                "$type": "SExp_Identifier",
                "value": "b",
                "typeArgs": []
            },
            "operand1": {
                "$type": "SExp_Call",
                "callable": {
                    "$type": "SExp_Identifier",
                    "value": "c",
                    "typeArgs": []
                },
                "args": {
                    "$type": "SArguments",
                    "items": [
                        {
                            "$type": "SArgument",
                            "bOut": false,
                            "bParams": false,
                            "exp": {
                                "$type": "SExp_IntLiteral",
                                "value": 1
                            }
                        }
                    ]
                }
            }
        }
    }
})---";

    EXPECT_SYNTAX_EQ(stmt, expected);
}

TEST(StmtParser, ParseForStmt)
{
    auto [buffer, lexer] = Prepare(UR"---(for (f(); g; h + g) ;)---");
    SFactory factory;

    auto* stmt = ParseStmt(&lexer, factory);

    auto expected = R"---({
    "$type": "SStmt_For",
    "initializer": {
        "$type": "SForStmtInitializer_Exp",
        "exp": {
            "$type": "SExp_Call",
            "callable": {
                "$type": "SExp_Identifier",
                "value": "f",
                "typeArgs": []
            },
            "args": {
                "$type": "SArguments",
                "items": []
            }
        }
    },
    "cond": {
        "$type": "SExp_Identifier",
        "value": "g",
        "typeArgs": []
    },
    "cont": {
        "$type": "SExp_BinaryOp",
        "kind": "Add",
        "operand0": {
            "$type": "SExp_Identifier",
            "value": "h",
            "typeArgs": []
        },
        "operand1": {
            "$type": "SExp_Identifier",
            "value": "g",
            "typeArgs": []
        }
    },
    "body": {
        "$type": "SEmbeddableStmt_Single",
        "stmt": {
            "$type": "SStmt_Blank"
        }
    }
})---";

    EXPECT_SYNTAX_EQ(stmt, expected);
}

TEST(StmtParser, ParseForeachStmt)
{
    auto [buffer, lexer] = Prepare(UR"---(foreach( var x in l ) { } )---");
    SFactory factory;

    auto* stmt = ParseStmt(&lexer, factory);

    auto expected = R"---({
    "$type": "SStmt_Foreach",
    "type": {
        "$type": "STypeExp_Id",
        "name": "var",
        "typeArgs": []
    },
    "varName": "x",
    "enumerable": {
        "$type": "SExp_Identifier",
        "value": "l",
        "typeArgs": []
    },
    "body": {
        "$type": "SEmbeddableStmt_Block",
        "stmts": []
    }
})---";

    EXPECT_SYNTAX_EQ(stmt, expected);
}

TEST(StmtParser, ParseIfIsExpCondStmt)
{
    auto [buffer, lexer] = Prepare(UR"---(if (b is T) {} else if (c) {} else {})---");
    SFactory factory;

    auto* stmt = ParseStmt(&lexer, factory);

    auto expected = R"---({
    "$type": "SStmt_If",
    "cond": {
        "$type": "SExp_Is",
        "exp": {
            "$type": "SExp_Identifier",
            "value": "b",
            "typeArgs": []
        },
        "type": {
            "$type": "STypeExp_Id",
            "name": "T",
            "typeArgs": []
        }
    },
    "body": {
        "$type": "SEmbeddableStmt_Block",
        "stmts": []
    },
    "elseBody": {
        "$type": "SEmbeddableStmt_Single",
        "stmt": {
            "$type": "SStmt_If",
            "cond": {
                "$type": "SExp_Identifier",
                "value": "c",
                "typeArgs": []
            },
            "body": {
                "$type": "SEmbeddableStmt_Block",
                "stmts": []
            },
            "elseBody": {
                "$type": "SEmbeddableStmt_Block",
                "stmts": []
            }
        }
    }
})---";

    EXPECT_SYNTAX_EQ(stmt, expected);
}

TEST(StmtParser, ParseIfStmt)
{
    auto [buffer, lexer] = Prepare(UR"---(if (b) {} else if (c) {} else {})---");
    SFactory factory;

    auto* stmt = ParseStmt(&lexer, factory);

    auto expected = R"---({
    "$type": "SStmt_If",
    "cond": {
        "$type": "SExp_Identifier",
        "value": "b",
        "typeArgs": []
    },
    "body": {
        "$type": "SEmbeddableStmt_Block",
        "stmts": []
    },
    "elseBody": {
        "$type": "SEmbeddableStmt_Single",
        "stmt": {
            "$type": "SStmt_If",
            "cond": {
                "$type": "SExp_Identifier",
                "value": "c",
                "typeArgs": []
            },
            "body": {
                "$type": "SEmbeddableStmt_Block",
                "stmts": []
            },
            "elseBody": {
                "$type": "SEmbeddableStmt_Block",
                "stmts": []
            }
        }
    }
})---";

    EXPECT_SYNTAX_EQ(stmt, expected);
}

TEST(StmtParser, ParseIfTestStmtWithVarName)
{
    auto [buffer, lexer] = Prepare(UR"---(if (T t = b) {} else if (c) {} else {})---");
    SFactory factory;

    auto* stmt = ParseStmt(&lexer, factory);

    auto expected = R"---({
    "$type": "SStmt_IfTest",
    "testType": {
        "$type": "STypeExp_Id",
        "name": "T",
        "typeArgs": []
    },
    "varName": "t",
    "exp": {
        "$type": "SExp_Identifier",
        "value": "b",
        "typeArgs": []
    },
    "body": {
        "$type": "SEmbeddableStmt_Block",
        "stmts": []
    },
    "elseBody": {
        "$type": "SEmbeddableStmt_Single",
        "stmt": {
            "$type": "SStmt_If",
            "cond": {
                "$type": "SExp_Identifier",
                "value": "c",
                "typeArgs": []
            },
            "body": {
                "$type": "SEmbeddableStmt_Block",
                "stmts": []
            },
            "elseBody": {
                "$type": "SEmbeddableStmt_Block",
                "stmts": []
            }
        }
    }
})---";

    EXPECT_SYNTAX_EQ(stmt, expected);
}

TEST(StmtParser, ParseInlineCommandStmt)
{
    auto [buffer, lexer] = Prepare(UR"---(@echo ${a}bbb  )---");
    SFactory factory;

    auto* stmt = ParseStmt(&lexer, factory);

    auto expected = R"---({
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
                        "value": "a",
                        "typeArgs": []
                    }
                },
                {
                    "$type": "SStringExpElement_Text",
                    "text": "bbb  "
                }
            ]
        }
    ]
})---";

    EXPECT_SYNTAX_EQ(stmt, expected);
}

TEST(StmtParser, ParseNullableVarDeclStmt)
{
    auto [buffer, lexer] = Prepare(UR"---(int? p;)---");
    SFactory factory;

    auto* stmt = ParseStmt(&lexer, factory);

    auto expected = R"---({
    "$type": "SStmt_VarDecl",
    "varDecl": {
        "$type": "SVarDecl",
        "type": {
            "$type": "STypeExp_Nullable",
            "innerType": {
                "$type": "STypeExp_Id",
                "name": "int",
                "typeArgs": []
            }
        },
        "elements": [
            {
                "$type": "SVarDeclElement",
                "varName": "p",
                "initExp": null
            }
        ]
    }
})---";

    EXPECT_SYNTAX_EQ(stmt, expected);
}

TEST(StmtParser, ParsePtrVarDeclStmt)
{
    auto [buffer, lexer] = Prepare(UR"---(int* p;)---");
    SFactory factory;

    auto* stmt = ParseStmt(&lexer, factory);

    auto expected = R"---({
    "$type": "SStmt_VarDecl",
    "varDecl": {
        "$type": "SVarDecl",
        "type": {
            "$type": "STypeExp_Ptr",
            "innerType": {
                "$type": "STypeExp_Id",
                "name": "int",
                "typeArgs": []
            }
        },
        "elements": [
            {
                "$type": "SVarDeclElement",
                "varName": "p",
                "initExp": null
            }
        ]
    }
})---";

    EXPECT_SYNTAX_EQ(stmt, expected);
}

TEST(StmtParser, ParseVarDeclStmt)
{
    auto [buffer, lexer] = Prepare(UR"---({
	string a = "hello";
	var b = 3;
	var* c = &b;
	var& d = b;
	var? e = 1;
	shared var f = F();
	shared<int>& g = f;
	nullable<int>& h = e;
})---");
    SFactory factory;

    auto* stmt = ParseStmt(&lexer, factory);

    auto expected = R"---({
    "$type": "SStmt_Block",
    "stmts": [
        {
            "$type": "SStmt_VarDecl",
            "varDecl": {
                "$type": "SVarDecl",
                "type": {
                    "$type": "SVarDeclType_Normal",
                    "typeExp": {
                        "$type": "STypeExp_Id",
                        "name": "string",
                        "typeArgs": []
                    }
                },
                "elements": [
                    {
                        "$type": "SVarDeclElement",
                        "varName": "a",
                        "initExp": {
                            "$type": "SExp_String",
                            "elements": [
                                {
                                    "$type": "SStringExpElement_Text",
                                    "text": "hello"
                                }
                            ]
                        }
                    }
                ]
            }
        },
        {
            "$type": "SStmt_VarDecl",
            "varDecl": {
                "$type": "SVarDecl",
                "type": {
                    "$type": "SVarDeclType_Var",
                    "kind": "Normal"
                },
                "elements": [
                    {
                        "$type": "SVarDeclElement",
                        "varName": "b",
                        "initExp": {
                            "$type": "SExp_IntLiteral",
                            "value": 3
                        }
                    }
                ]
            }
        },
        {
            "$type": "SStmt_VarDecl",
            "varDecl": {
                "$type": "SVarDecl",
                "type": {
                    "$type": "SVarDeclType_Var",
                    "kind": "Ptr"
                },
                "elements": [
                    {
                        "$type": "SVarDeclElement",
                        "varName": "c",
                        "initExp": {
                            "$type": "SExp_UnaryOp",
                            "kind": "Ref",
                            "operand": {
                                "$type": "SExp_Identifier",
                                "value": "b",
                                "typeArgs": []
                            }
                        }
                    }
                ]
            }
        },
        {
            "$type": "SStmt_VarDecl",
            "varDecl": {
                "$type": "SVarDecl",
                "type": {
                    "$type": "SVarDeclType_VarRef"
                },
                "elements": [
                    {
                        "$type": "SVarDeclElement",
                        "varName": "d",
                        "initExp": {
                            "$type": "SExp_Identifier",
                            "value": "b",
                            "typeArgs": []
                        }
                    }
                ]
            }
        },
        {
            "$type": "SStmt_VarDecl",
            "varDecl": {
                "$type": "SVarDecl",
                "type": {
                    "$type": "SVarDeclType_Var",
                    "kind": "Nullable"
                },
                "elements": [
                    {
                        "$type": "SVarDeclElement",
                        "varName": "e",
                        "initExp": {
                            "$type": "SExp_IntLiteral",
                            "value": 1
                        }
                    }
                ]
            }
        },
        {
            "$type": "SStmt_VarDecl",
            "varDecl": {
                "$type": "SVarDecl",
                "type": {
                    "$type": "SVarDeclType_Var",
                    "kind": "Shared"
                },
                "elements": [
                    {
                        "$type": "SVarDeclElement",
                        "varName": "f",
                        "initExp": {
                            "$type": "SExp_Call",
                            "callable": {
                                "$type": "SExp_Identifier",
                                "value": "F",
                                "typeArgs": []
                            },
                            "args": {
                                "$type": "SArguments",
                                "items": []
                            }
                        }
                    }
                ]
            }
        },
        {
            "$type": "SStmt_VarDecl",
            "varDecl": {
                "$type": "SVarDecl",
                "type": {
                    "$type": "SVarDeclType_Ref",
                    "typeExp": {
                        "$type": "STypeExp_Shared",
                        "innerType": {
                            "$type": "STypeExp_Id",
                            "name": "int",
                            "typeArgs": []
                        }
                    }
                },
                "elements": [
                    {
                        "$type": "SVarDeclElement",
                        "varName": "g",
                        "initExp": {
                            "$type": "SExp_Identifier",
                            "value": "f",
                            "typeArgs": []
                        }
                    }
                ]
            }
        },
        {
            "$type": "SStmt_VarDecl",
            "varDecl": {
                "$type": "SVarDecl",
                "type": {
                    "$type": "SVarDeclType_Ref",
                    "typeExp": {
                        "$type": "STypeExp_Nullable",
                        "innerType": {
                            "$type": "STypeExp_Id",
                            "name": "int",
                            "typeArgs": []
                        }
                    }
                },
                "elements": [
                    {
                        "$type": "SVarDeclElement",
                        "varName": "h",
                        "initExp": {
                            "$type": "SExp_Identifier",
                            "value": "e",
                            "typeArgs": []
                        }
                    }
                ]
            }
        }
    ]
})---";

    EXPECT_SYNTAX_EQ(stmt, expected);
}

