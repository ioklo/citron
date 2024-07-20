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
    "$type": "SBlankStmt"
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
    "$type": "SCommandStmt",
    "commands": [
        {
            "$type": "SStringExp",
            "elements": [
                {
                    "$type": "STextStringExpElement",
                    "text": "    echo "
                },
                {
                    "$type": "SExpStringExpElement",
                    "exp": {
                        "$type": "SIdentifierExp",
                        "value": "a",
                        "typeArgs": []
                    }
                },
                {
                    "$type": "STextStringExpElement",
                    "text": " bbb   "
                }
            ]
        },
        {
            "$type": "SStringExp",
            "elements": [
                {
                    "$type": "STextStringExpElement",
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
    "$type": "SBlockStmt",
    "stmts": [
        {
            "$type": "SBlockStmt",
            "stmts": []
        },
        {
            "$type": "SBlockStmt",
            "stmts": [
                {
                    "$type": "SBlankStmt"
                }
            ]
        },
        {
            "$type": "SBlankStmt"
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
    "$type": "SVarDeclStmt",
    "varDecl": {
        "$type": "SVarDecl",
        "type": {
            "$type": "SBoxPtrTypeExp",
            "innerType": {
                "$type": "SIdTypeExp",
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
    "$type": "SBreakStmt"
})---";

    EXPECT_SYNTAX_EQ(oStmt, expected);
}

TEST(StmtParser, ParseContinueStmt)
{
    auto [buffer, lexer] = Prepare(UR"---(continue;)---");

    auto oStmt = ParseStmt(&lexer);

    auto expected = R"---({
    "$type": "SContinueStmt"
})---";

    EXPECT_SYNTAX_EQ(oStmt, expected);
}

TEST(StmtParser, ParseDirectiveStmt)
{
    auto [buffer, lexer] = Prepare(UR"---(`notnull(a);)---");

    auto oStmt = ParseStmt(&lexer);

    auto expected = R"---({
    "$type": "SDirectiveStmt",
    "name": "notnull",
    "args": [
        {
            "$type": "SIdentifierExp",
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
    "$type": "SExpStmt",
    "exp": {
        "$type": "SBinaryOpExp",
        "kind": "Assign",
        "operand0": {
            "$type": "SIdentifierExp",
            "value": "a",
            "typeArgs": []
        },
        "operand1": {
            "$type": "SBinaryOpExp",
            "kind": "Multiply",
            "operand0": {
                "$type": "SIdentifierExp",
                "value": "b",
                "typeArgs": []
            },
            "operand1": {
                "$type": "SCallExp",
                "callable": {
                    "$type": "SIdentifierExp",
                    "value": "c",
                    "typeArgs": []
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
    "$type": "SForStmt",
    "initializer": {
        "$type": "SExpForStmtInitializer",
        "exp": {
            "$type": "SCallExp",
            "callable": {
                "$type": "SIdentifierExp",
                "value": "f",
                "typeArgs": []
            },
            "args": []
        }
    },
    "cond": {
        "$type": "SIdentifierExp",
        "value": "g",
        "typeArgs": []
    },
    "cont": {
        "$type": "SBinaryOpExp",
        "kind": "Add",
        "operand0": {
            "$type": "SIdentifierExp",
            "value": "h",
            "typeArgs": []
        },
        "operand1": {
            "$type": "SIdentifierExp",
            "value": "g",
            "typeArgs": []
        }
    },
    "body": {
        "$type": "SSingleEmbeddableStmt",
        "stmt": {
            "$type": "SBlankStmt"
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
    "$type": "SForeachStmt",
    "type": {
        "$type": "SIdTypeExp",
        "name": "var",
        "typeArgs": []
    },
    "varName": "x",
    "enumerable": {
        "$type": "SIdentifierExp",
        "value": "l",
        "typeArgs": []
    },
    "body": {
        "$type": "SBlockEmbeddableStmt",
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
    "$type": "SIfStmt",
    "cond": {
        "$type": "SIsExp",
        "exp": {
            "$type": "SIdentifierExp",
            "value": "b",
            "typeArgs": []
        },
        "type": {
            "$type": "SIdTypeExp",
            "name": "T",
            "typeArgs": []
        }
    },
    "body": {
        "$type": "SBlockEmbeddableStmt",
        "stmts": []
    },
    "elseBody": {
        "$type": "SSingleEmbeddableStmt",
        "stmt": {
            "$type": "SIfStmt",
            "cond": {
                "$type": "SIdentifierExp",
                "value": "c",
                "typeArgs": []
            },
            "body": {
                "$type": "SBlockEmbeddableStmt",
                "stmts": []
            },
            "elseBody": {
                "$type": "SBlockEmbeddableStmt",
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
    "$type": "SIfStmt",
    "cond": {
        "$type": "SIdentifierExp",
        "value": "b",
        "typeArgs": []
    },
    "body": {
        "$type": "SBlockEmbeddableStmt",
        "stmts": []
    },
    "elseBody": {
        "$type": "SSingleEmbeddableStmt",
        "stmt": {
            "$type": "SIfStmt",
            "cond": {
                "$type": "SIdentifierExp",
                "value": "c",
                "typeArgs": []
            },
            "body": {
                "$type": "SBlockEmbeddableStmt",
                "stmts": []
            },
            "elseBody": {
                "$type": "SBlockEmbeddableStmt",
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
    "$type": "SIfTestStmt",
    "testType": {
        "$type": "SIdTypeExp",
        "name": "T",
        "typeArgs": []
    },
    "varName": "t",
    "exp": {
        "$type": "SIdentifierExp",
        "value": "b",
        "typeArgs": []
    },
    "body": {
        "$type": "SBlockEmbeddableStmt",
        "stmts": []
    },
    "elseBody": {
        "$type": "SSingleEmbeddableStmt",
        "stmt": {
            "$type": "SIfStmt",
            "cond": {
                "$type": "SIdentifierExp",
                "value": "c",
                "typeArgs": []
            },
            "body": {
                "$type": "SBlockEmbeddableStmt",
                "stmts": []
            },
            "elseBody": {
                "$type": "SBlockEmbeddableStmt",
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
                        "value": "a",
                        "typeArgs": []
                    }
                },
                {
                    "$type": "STextStringExpElement",
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
    "$type": "SVarDeclStmt",
    "varDecl": {
        "$type": "SVarDecl",
        "type": {
            "$type": "SLocalPtrTypeExp",
            "innerType": {
                "$type": "SIdTypeExp",
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
    "$type": "SVarDeclStmt",
    "varDecl": {
        "$type": "SVarDecl",
        "type": {
            "$type": "SNullableTypeExp",
            "innerType": {
                "$type": "SIdTypeExp",
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
    "$type": "SVarDeclStmt",
    "varDecl": {
        "$type": "SVarDecl",
        "type": {
            "$type": "SIdTypeExp",
            "name": "string",
            "typeArgs": []
        },
        "elements": [
            {
                "$type": "SVarDeclElement",
                "varName": "a",
                "initExp": {
                    "$type": "SStringExp",
                    "elements": [
                        {
                            "$type": "STextStringExpElement",
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

