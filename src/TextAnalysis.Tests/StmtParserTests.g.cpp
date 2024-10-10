#include "pch.h"

#include <Syntax/Syntax.h>
#include <TextAnalysis/StmtParser.h>

#include "TestMisc.h"

using namespace std;
using namespace Citron;

TEST(StmtParser, ParseBlankStmt)
{
    auto [buffer, lexer] = Prepare(UR"---(  ;  )---");

    auto oStmt = ParseStmt(&lexer);

    auto expected = R"---({
    "$type": "SStmt_Blank"
})---";

    EXPECT_SYNTAX_EQ(oStmt, expected);
}

TEST(StmtParser, ParseBlockCommandStmt)
{
    auto [buffer, lexer] = Prepare(UR"---(@{ 
    echo ${ a } bbb   
xxx
})---");

    auto oStmt = ParseStmt(&lexer);

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

    EXPECT_SYNTAX_EQ(oStmt, expected);
}

TEST(StmtParser, ParseBlockStmt)
{
    auto [buffer, lexer] = Prepare(UR"---({ { } { ; } ; })---");

    auto oStmt = ParseStmt(&lexer);

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

    EXPECT_SYNTAX_EQ(oStmt, expected);
}

TEST(StmtParser, ParseBoxPtrVarDeclStmt)
{
    auto [buffer, lexer] = Prepare(UR"---(box int* p;)---");

    auto oStmt = ParseStmt(&lexer);

    auto expected = R"---({
    "$type": "SStmt_VarDecl",
    "varDecl": {
        "$type": "SVarDecl",
        "type": {
            "$type": "STypeExp_BoxPtr",
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

    EXPECT_SYNTAX_EQ(oStmt, expected);
}

TEST(StmtParser, ParseBreakStmt)
{
    auto [buffer, lexer] = Prepare(UR"---(break;)---");

    auto oStmt = ParseStmt(&lexer);

    auto expected = R"---({
    "$type": "SStmt_Break"
})---";

    EXPECT_SYNTAX_EQ(oStmt, expected);
}

TEST(StmtParser, ParseContinueStmt)
{
    auto [buffer, lexer] = Prepare(UR"---(continue;)---");

    auto oStmt = ParseStmt(&lexer);

    auto expected = R"---({
    "$type": "SStmt_Continue"
})---";

    EXPECT_SYNTAX_EQ(oStmt, expected);
}

TEST(StmtParser, ParseDirectiveStmt)
{
    auto [buffer, lexer] = Prepare(UR"---(`notnull(a);)---");

    auto oStmt = ParseStmt(&lexer);

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

    EXPECT_SYNTAX_EQ(oStmt, expected);
}

TEST(StmtParser, ParseExpStmt)
{
    auto [buffer, lexer] = Prepare(UR"---(a = b * c(1);)---");

    auto oStmt = ParseStmt(&lexer);

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
                "args": [
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
})---";

    EXPECT_SYNTAX_EQ(oStmt, expected);
}

TEST(StmtParser, ParseForStmt)
{
    auto [buffer, lexer] = Prepare(UR"---(for (f(); g; h + g) ;)---");

    auto oStmt = ParseStmt(&lexer);

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
            "args": []
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

    EXPECT_SYNTAX_EQ(oStmt, expected);
}

TEST(StmtParser, ParseForeachStmt)
{
    auto [buffer, lexer] = Prepare(UR"---(foreach( var x in l ) { } )---");

    auto oStmt = ParseStmt(&lexer);

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

    EXPECT_SYNTAX_EQ(oStmt, expected);
}

TEST(StmtParser, ParseIfIsExpCondStmt)
{
    auto [buffer, lexer] = Prepare(UR"---(if (b is T) {} else if (c) {} else {})---");

    auto oStmt = ParseStmt(&lexer);

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

    EXPECT_SYNTAX_EQ(oStmt, expected);
}

TEST(StmtParser, ParseIfStmt)
{
    auto [buffer, lexer] = Prepare(UR"---(if (b) {} else if (c) {} else {})---");

    auto oStmt = ParseStmt(&lexer);

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

    EXPECT_SYNTAX_EQ(oStmt, expected);
}

TEST(StmtParser, ParseIfTestStmtWithVarName)
{
    auto [buffer, lexer] = Prepare(UR"---(if (T t = b) {} else if (c) {} else {})---");

    auto oStmt = ParseStmt(&lexer);

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

    EXPECT_SYNTAX_EQ(oStmt, expected);
}

TEST(StmtParser, ParseInlineCommandStmt)
{
    auto [buffer, lexer] = Prepare(UR"---(@echo ${a}bbb  )---");

    auto oStmt = ParseStmt(&lexer);

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

    EXPECT_SYNTAX_EQ(oStmt, expected);
}

TEST(StmtParser, ParseLocalPtrVarDeclStmt)
{
    auto [buffer, lexer] = Prepare(UR"---(int* p;)---");

    auto oStmt = ParseStmt(&lexer);

    auto expected = R"---({
    "$type": "SStmt_VarDecl",
    "varDecl": {
        "$type": "SVarDecl",
        "type": {
            "$type": "STypeExp_LocalPtr",
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

    EXPECT_SYNTAX_EQ(oStmt, expected);
}

TEST(StmtParser, ParseNullableVarDeclStmt)
{
    auto [buffer, lexer] = Prepare(UR"---(int? p;)---");

    auto oStmt = ParseStmt(&lexer);

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

    EXPECT_SYNTAX_EQ(oStmt, expected);
}

TEST(StmtParser, ParseVarDeclStmt)
{
    auto [buffer, lexer] = Prepare(UR"---(string a = "hello";)---");

    auto oStmt = ParseStmt(&lexer);

    auto expected = R"---({
    "$type": "SStmt_VarDecl",
    "varDecl": {
        "$type": "SVarDecl",
        "type": {
            "$type": "STypeExp_Id",
            "name": "string",
            "typeArgs": []
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
})---";

    EXPECT_SYNTAX_EQ(oStmt, expected);
}

