#include "pch.h"

#include <Syntax/Syntax.h>
#include <TextAnalysis/TypeExpParser.h>

#include "TestMisc.h"

using namespace std;
using namespace Citron;

TEST(TypeExpParser, BoxPtr_ParseAmbiguousLocalAndBoxPtrs)
{
    auto [buffer, lexer] = Prepare(UR"---(box T**)---");

    auto oTypeExp = ParseTypeExp(&lexer);

    EXPECT_TRUE(!lexer.IsReachedEnd());
}

TEST(TypeExpParser, BoxPtr_ParseBoxPtrOfIdChain)
{
    auto [buffer, lexer] = Prepare(UR"---(box A.B<int>.C*)---");

    auto oTypeExp = ParseTypeExp(&lexer);

    auto expected = R"---({
    "$type": "STypeExp_BoxPtr",
    "innerType": {
        "$type": "STypeExp_Member",
        "parentType": {
            "$type": "STypeExp_Member",
            "parentType": {
                "$type": "STypeExp_Id",
                "name": "A",
                "typeArgs": []
            },
            "name": "B",
            "typeArgs": [
                {
                    "$type": "STypeExp_Id",
                    "name": "int",
                    "typeArgs": []
                }
            ]
        },
        "name": "C",
        "typeArgs": []
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(oTypeExp, expected);
}

TEST(TypeExpParser, BoxPtr_ParseBoxPtrOfNullableRaw)
{
    auto [buffer, lexer] = Prepare(UR"---(box T?*)---");

    auto oTypeExp = ParseTypeExp(&lexer);

    EXPECT_TRUE(!lexer.IsReachedEnd());
}

TEST(TypeExpParser, BoxPtr_ParseBoxPtrOfParen)
{
    auto [buffer, lexer] = Prepare(UR"---(box (T?)*)---");

    auto oTypeExp = ParseTypeExp(&lexer);

    auto expected = R"---({
    "$type": "STypeExp_BoxPtr",
    "innerType": {
        "$type": "STypeExp_Nullable",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "T",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(oTypeExp, expected);
}

TEST(TypeExpParser, BoxPtr_ParseNestedBoxPtrs)
{
    auto [buffer, lexer] = Prepare(UR"---(box box T**)---");

    auto oTypeExp = ParseTypeExp(&lexer);

    EXPECT_TRUE(!lexer.IsReachedEnd());
}

TEST(TypeExpParser, LocalPtr_ParseLocalPtrOfIdChain)
{
    auto [buffer, lexer] = Prepare(UR"---(A.B<int>.C*)---");

    auto oTypeExp = ParseTypeExp(&lexer);

    auto expected = R"---({
    "$type": "STypeExp_LocalPtr",
    "innerType": {
        "$type": "STypeExp_Member",
        "parentType": {
            "$type": "STypeExp_Member",
            "parentType": {
                "$type": "STypeExp_Id",
                "name": "A",
                "typeArgs": []
            },
            "name": "B",
            "typeArgs": [
                {
                    "$type": "STypeExp_Id",
                    "name": "int",
                    "typeArgs": []
                }
            ]
        },
        "name": "C",
        "typeArgs": []
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(oTypeExp, expected);
}

TEST(TypeExpParser, LocalPtr_ParseLocalPtrOfNullableRaw)
{
    auto [buffer, lexer] = Prepare(UR"---(T?*)---");

    auto oTypeExp = ParseTypeExp(&lexer);

    EXPECT_TRUE(!lexer.IsReachedEnd());
}

TEST(TypeExpParser, LocalPtr_ParseLocalPtrOfParen)
{
    auto [buffer, lexer] = Prepare(UR"---((T?)*)---");

    auto oTypeExp = ParseTypeExp(&lexer);

    auto expected = R"---({
    "$type": "STypeExp_LocalPtr",
    "innerType": {
        "$type": "STypeExp_Nullable",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "T",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(oTypeExp, expected);
}

TEST(TypeExpParser, LocalPtr_ParseNestedLocalPtrs)
{
    auto [buffer, lexer] = Prepare(UR"---(T**)---");

    auto oTypeExp = ParseTypeExp(&lexer);

    auto expected = R"---({
    "$type": "STypeExp_LocalPtr",
    "innerType": {
        "$type": "STypeExp_LocalPtr",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "T",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(oTypeExp, expected);
}

TEST(TypeExpParser, Nullable_ParseDoubleQuestionMark)
{
    auto [buffer, lexer] = Prepare(UR"---(T??)---");

    auto oTypeExp = ParseTypeExp(&lexer);

    EXPECT_TRUE(!lexer.IsReachedEnd());
}

TEST(TypeExpParser, Nullable_ParseIdChainNullable)
{
    auto [buffer, lexer] = Prepare(UR"---(A.B<int>.C?)---");

    auto oTypeExp = ParseTypeExp(&lexer);

    auto expected = R"---({
    "$type": "STypeExp_Nullable",
    "innerType": {
        "$type": "STypeExp_Member",
        "parentType": {
            "$type": "STypeExp_Member",
            "parentType": {
                "$type": "STypeExp_Id",
                "name": "A",
                "typeArgs": []
            },
            "name": "B",
            "typeArgs": [
                {
                    "$type": "STypeExp_Id",
                    "name": "int",
                    "typeArgs": []
                }
            ]
        },
        "name": "C",
        "typeArgs": []
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(oTypeExp, expected);
}

TEST(TypeExpParser, Nullable_ParseNullableBoxPtr)
{
    auto [buffer, lexer] = Prepare(UR"---(box T*?)---");

    auto oTypeExp = ParseTypeExp(&lexer);

    auto expected = R"---({
    "$type": "STypeExp_Nullable",
    "innerType": {
        "$type": "STypeExp_BoxPtr",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "T",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(oTypeExp, expected);
}

TEST(TypeExpParser, Nullable_ParseNullableLocalPtr)
{
    auto [buffer, lexer] = Prepare(UR"---(T*?)---");

    auto oTypeExp = ParseTypeExp(&lexer);

    auto expected = R"---({
    "$type": "STypeExp_Nullable",
    "innerType": {
        "$type": "STypeExp_LocalPtr",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "T",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(oTypeExp, expected);
}

TEST(TypeExpParser, Nullable_ParseNullableParen)
{
    auto [buffer, lexer] = Prepare(UR"---((T*)?)---");

    auto oTypeExp = ParseTypeExp(&lexer);

    auto expected = R"---({
    "$type": "STypeExp_Nullable",
    "innerType": {
        "$type": "STypeExp_LocalPtr",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "T",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(oTypeExp, expected);
}

TEST(TypeExpParser, Paren_ParseWrappedBoxPtr)
{
    auto [buffer, lexer] = Prepare(UR"---((box T*)?)---");

    auto oTypeExp = ParseTypeExp(&lexer);

    auto expected = R"---({
    "$type": "STypeExp_Nullable",
    "innerType": {
        "$type": "STypeExp_BoxPtr",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "T",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(oTypeExp, expected);
}

TEST(TypeExpParser, Paren_ParseWrappedIdChain)
{
    auto [buffer, lexer] = Prepare(UR"---((A.B<int>.C)?)---");

    auto oTypeExp = ParseTypeExp(&lexer);

    EXPECT_TRUE(!lexer.IsReachedEnd());
}

TEST(TypeExpParser, Paren_ParseWrappedLocalPtr)
{
    auto [buffer, lexer] = Prepare(UR"---((T*)*)---");

    auto oTypeExp = ParseTypeExp(&lexer);

    auto expected = R"---({
    "$type": "STypeExp_LocalPtr",
    "innerType": {
        "$type": "STypeExp_LocalPtr",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "T",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(oTypeExp, expected);
}

TEST(TypeExpParser, Paren_ParseWrappedNested)
{
    auto [buffer, lexer] = Prepare(UR"---(((T*))?)---");

    auto oTypeExp = ParseTypeExp(&lexer);

    EXPECT_TRUE(!lexer.IsReachedEnd());
}

TEST(TypeExpParser, Paren_ParseWrappedNullable)
{
    auto [buffer, lexer] = Prepare(UR"---((T?)?)---");

    auto oTypeExp = ParseTypeExp(&lexer);

    auto expected = R"---({
    "$type": "STypeExp_Nullable",
    "innerType": {
        "$type": "STypeExp_Nullable",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "T",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(oTypeExp, expected);
}

TEST(TypeExpParser, TopLevel_ParseBoxPtr)
{
    auto [buffer, lexer] = Prepare(UR"---(box T*)---");

    auto oTypeExp = ParseTypeExp(&lexer);

    auto expected = R"---({
    "$type": "STypeExp_BoxPtr",
    "innerType": {
        "$type": "STypeExp_Id",
        "name": "T",
        "typeArgs": []
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(oTypeExp, expected);
}

TEST(TypeExpParser, TopLevel_ParseIdChain)
{
    auto [buffer, lexer] = Prepare(UR"---(A.B<int>.C)---");

    auto oTypeExp = ParseTypeExp(&lexer);

    auto expected = R"---({
    "$type": "STypeExp_Member",
    "parentType": {
        "$type": "STypeExp_Member",
        "parentType": {
            "$type": "STypeExp_Id",
            "name": "A",
            "typeArgs": []
        },
        "name": "B",
        "typeArgs": [
            {
                "$type": "STypeExp_Id",
                "name": "int",
                "typeArgs": []
            }
        ]
    },
    "name": "C",
    "typeArgs": []
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(oTypeExp, expected);
}

TEST(TypeExpParser, TopLevel_ParseLocalPtr)
{
    auto [buffer, lexer] = Prepare(UR"---(int*)---");

    auto oTypeExp = ParseTypeExp(&lexer);

    auto expected = R"---({
    "$type": "STypeExp_LocalPtr",
    "innerType": {
        "$type": "STypeExp_Id",
        "name": "int",
        "typeArgs": []
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(oTypeExp, expected);
}

TEST(TypeExpParser, TopLevel_ParseNullable)
{
    auto [buffer, lexer] = Prepare(UR"---(T?)---");

    auto oTypeExp = ParseTypeExp(&lexer);

    auto expected = R"---({
    "$type": "STypeExp_Nullable",
    "innerType": {
        "$type": "STypeExp_Id",
        "name": "T",
        "typeArgs": []
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(oTypeExp, expected);
}

TEST(TypeExpParser, TopLevel_ParseParenSolo)
{
    auto [buffer, lexer] = Prepare(UR"---((T))---");

    auto oTypeExp = ParseTypeExp(&lexer);

    EXPECT_TRUE(!lexer.IsReachedEnd());
}

