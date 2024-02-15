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
    "$type": "BlankStmtSyntax"
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
    "$type": "CommandStmtSyntax",
    "commands": [
        {
            "$type": "StringExpSyntax",
            "elements": [
                {
                    "$type": "TextStringExpSyntaxElement",
                    "text": "    echo "
                },
                {
                    "$type": "ExpStringExpSyntaxElement",
                    "exp": {
                        "$type": "IdentifierExpSyntax",
                        "value": "a",
                        "typeArgs": []
                    }
                },
                {
                    "$type": "TextStringExpSyntaxElement",
                    "text": " bbb   "
                }
            ]
        },
        {
            "$type": "StringExpSyntax",
            "elements": [
                {
                    "$type": "TextStringExpSyntaxElement",
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
    "$type": "BlockStmtSyntax",
    "stmts": [
        {
            "$type": "BlockStmtSyntax",
            "stmts": []
        },
        {
            "$type": "BlockStmtSyntax",
            "stmts": [
                {
                    "$type": "BlankStmtSyntax"
                }
            ]
        },
        {
            "$type": "BlankStmtSyntax"
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
    "$type": "VarDeclStmtSyntax",
    "varDecl": {
        "$type": "VarDeclSyntax",
        "type": {
            "$type": "BoxPtrTypeExpSyntax",
            "innerType": {
                "$type": "IdTypeExpSyntax",
                "name": "int",
                "typeArgs": []
            }
        },
        "elements": [
            {
                "$type": "VarDeclSyntaxElement",
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
    "$type": "BreakStmtSyntax"
})---";

    EXPECT_SYNTAX_EQ(oStmt, expected);
}

TEST(StmtParser, ParseContinueStmt)
{
    auto [buffer, lexer] = Prepare(UR"---(continue;)---");

    auto oStmt = ParseStmt(&lexer);

    auto expected = R"---({
    "$type": "ContinueStmtSyntax"
})---";

    EXPECT_SYNTAX_EQ(oStmt, expected);
}

TEST(StmtParser, ParseDirectiveStmt)
{
    auto [buffer, lexer] = Prepare(UR"---(`notnull(a);)---");

    auto oStmt = ParseStmt(&lexer);

    auto expected = R"---({
    "$type": "DirectiveStmtSyntax",
    "name": "notnull",
    "args": [
        {
            "$type": "IdentifierExpSyntax",
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
    "$type": "ExpStmtSyntax",
    "exp": {
        "$type": "BinaryOpExpSyntax",
        "kind": "Assign",
        "operand0": {
            "$type": "IdentifierExpSyntax",
            "value": "a",
            "typeArgs": []
        },
        "operand1": {
            "$type": "BinaryOpExpSyntax",
            "kind": "Multiply",
            "operand0": {
                "$type": "IdentifierExpSyntax",
                "value": "b",
                "typeArgs": []
            },
            "operand1": {
                "$type": "CallExpSyntax",
                "callable": {
                    "$type": "IdentifierExpSyntax",
                    "value": "c",
                    "typeArgs": []
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
    "$type": "ForStmtSyntax",
    "initializer": {
        "$type": "ExpForStmtInitializerSyntax",
        "exp": {
            "$type": "CallExpSyntax",
            "callable": {
                "$type": "IdentifierExpSyntax",
                "value": "f",
                "typeArgs": []
            },
            "args": []
        }
    },
    "cond": {
        "$type": "IdentifierExpSyntax",
        "value": "g",
        "typeArgs": []
    },
    "cont": {
        "$type": "BinaryOpExpSyntax",
        "kind": "Add",
        "operand0": {
            "$type": "IdentifierExpSyntax",
            "value": "h",
            "typeArgs": []
        },
        "operand1": {
            "$type": "IdentifierExpSyntax",
            "value": "g",
            "typeArgs": []
        }
    },
    "body": {
        "$type": "SingleEmbeddableStmtSyntax",
        "stmt": {
            "$type": "BlankStmtSyntax"
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
    "$type": "ForeachStmtSyntax",
    "type": {
        "$type": "IdTypeExpSyntax",
        "name": "var",
        "typeArgs": []
    },
    "varName": "x",
    "enumerable": {
        "$type": "IdentifierExpSyntax",
        "value": "l",
        "typeArgs": []
    },
    "body": {
        "$type": "BlockEmbeddableStmtSyntax",
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
    "$type": "IfStmtSyntax",
    "cond": {
        "$type": "IsExpSyntax",
        "exp": {
            "$type": "IdentifierExpSyntax",
            "value": "b",
            "typeArgs": []
        },
        "type": {
            "$type": "IdTypeExpSyntax",
            "name": "T",
            "typeArgs": []
        }
    },
    "body": {
        "$type": "BlockEmbeddableStmtSyntax",
        "stmts": []
    },
    "elseBody": {
        "$type": "SingleEmbeddableStmtSyntax",
        "stmt": {
            "$type": "IfStmtSyntax",
            "cond": {
                "$type": "IdentifierExpSyntax",
                "value": "c",
                "typeArgs": []
            },
            "body": {
                "$type": "BlockEmbeddableStmtSyntax",
                "stmts": []
            },
            "elseBody": {
                "$type": "BlockEmbeddableStmtSyntax",
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
    "$type": "IfStmtSyntax",
    "cond": {
        "$type": "IdentifierExpSyntax",
        "value": "b",
        "typeArgs": []
    },
    "body": {
        "$type": "BlockEmbeddableStmtSyntax",
        "stmts": []
    },
    "elseBody": {
        "$type": "SingleEmbeddableStmtSyntax",
        "stmt": {
            "$type": "IfStmtSyntax",
            "cond": {
                "$type": "IdentifierExpSyntax",
                "value": "c",
                "typeArgs": []
            },
            "body": {
                "$type": "BlockEmbeddableStmtSyntax",
                "stmts": []
            },
            "elseBody": {
                "$type": "BlockEmbeddableStmtSyntax",
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
    "$type": "IfTestStmtSyntax",
    "testType": {
        "$type": "IdTypeExpSyntax",
        "name": "T",
        "typeArgs": []
    },
    "varName": "t",
    "exp": {
        "$type": "IdentifierExpSyntax",
        "value": "b",
        "typeArgs": []
    },
    "body": {
        "$type": "BlockEmbeddableStmtSyntax",
        "stmts": []
    },
    "elseBody": {
        "$type": "SingleEmbeddableStmtSyntax",
        "stmt": {
            "$type": "IfStmtSyntax",
            "cond": {
                "$type": "IdentifierExpSyntax",
                "value": "c",
                "typeArgs": []
            },
            "body": {
                "$type": "BlockEmbeddableStmtSyntax",
                "stmts": []
            },
            "elseBody": {
                "$type": "BlockEmbeddableStmtSyntax",
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
                        "value": "a",
                        "typeArgs": []
                    }
                },
                {
                    "$type": "TextStringExpSyntaxElement",
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
    "$type": "VarDeclStmtSyntax",
    "varDecl": {
        "$type": "VarDeclSyntax",
        "type": {
            "$type": "LocalPtrTypeExpSyntax",
            "innerType": {
                "$type": "IdTypeExpSyntax",
                "name": "int",
                "typeArgs": []
            }
        },
        "elements": [
            {
                "$type": "VarDeclSyntaxElement",
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
    "$type": "VarDeclStmtSyntax",
    "varDecl": {
        "$type": "VarDeclSyntax",
        "type": {
            "$type": "NullableTypeExpSyntax",
            "innerType": {
                "$type": "IdTypeExpSyntax",
                "name": "int",
                "typeArgs": []
            }
        },
        "elements": [
            {
                "$type": "VarDeclSyntaxElement",
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
    "$type": "VarDeclStmtSyntax",
    "varDecl": {
        "$type": "VarDeclSyntax",
        "type": {
            "$type": "IdTypeExpSyntax",
            "name": "string",
            "typeArgs": []
        },
        "elements": [
            {
                "$type": "VarDeclSyntaxElement",
                "varName": "a",
                "initExp": {
                    "$type": "StringExpSyntax",
                    "elements": [
                        {
                            "$type": "TextStringExpSyntaxElement",
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

