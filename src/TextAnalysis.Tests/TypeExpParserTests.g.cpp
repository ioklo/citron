#include <gtest/gtest.h>

#include "Syntax/Syntax.h"
#include "TextAnalysis/TypeExpParser.h"

#include "TestMisc.h"

using namespace std;
using namespace Citron;

TEST(TypeExpParser, FormalLocal_FormalLocal)
{
    auto [buffer, lexer] = Prepare(UR"---(local<local<I>>)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Local",
    "innerType": {
        "$type": "STypeExp_Local",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "I",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, FormalLocal_FormalNullable)
{
    auto [buffer, lexer] = Prepare(UR"---(local<nullable<int>>)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Local",
    "innerType": {
        "$type": "STypeExp_Nullable",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "int",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, FormalLocal_FormalPtr)
{
    auto [buffer, lexer] = Prepare(UR"---(local<ptr<int>>)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Local",
    "innerType": {
        "$type": "STypeExp_Ptr",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "int",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, FormalLocal_FormalShared)
{
    auto [buffer, lexer] = Prepare(UR"---(local<shared<int>>)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Local",
    "innerType": {
        "$type": "STypeExp_Shared",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "int",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, FormalLocal_Id)
{
    auto [buffer, lexer] = Prepare(UR"---(local<A.B.C<int>.D>)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Local",
    "innerType": {
        "$type": "STypeExp_Member",
        "parentType": {
            "$type": "STypeExp_Member",
            "parentType": {
                "$type": "STypeExp_Member",
                "parentType": {
                    "$type": "STypeExp_Id",
                    "name": "A",
                    "typeArgs": []
                },
                "name": "B",
                "typeArgs": []
            },
            "name": "C",
            "typeArgs": [
                {
                    "$type": "STypeExp_Id",
                    "name": "int",
                    "typeArgs": []
                }
            ]
        },
        "name": "D",
        "typeArgs": []
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, FormalLocal_SugarLocal)
{
    auto [buffer, lexer] = Prepare(UR"---(local<local I>)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Local",
    "innerType": {
        "$type": "STypeExp_Local",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "I",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, FormalLocal_SugarNullable)
{
    auto [buffer, lexer] = Prepare(UR"---(local<int?>)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Local",
    "innerType": {
        "$type": "STypeExp_Nullable",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "int",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, FormalLocal_SugarPtr)
{
    auto [buffer, lexer] = Prepare(UR"---(local<int*>)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Local",
    "innerType": {
        "$type": "STypeExp_Ptr",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "int",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, FormalLocal_SugarShared)
{
    auto [buffer, lexer] = Prepare(UR"---(local<shared int>)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Local",
    "innerType": {
        "$type": "STypeExp_Shared",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "int",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, FormalNullable_FormalLocal)
{
    auto [buffer, lexer] = Prepare(UR"---(nullable<local<I>>)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Nullable",
    "innerType": {
        "$type": "STypeExp_Local",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "I",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, FormalNullable_FormalNullable)
{
    auto [buffer, lexer] = Prepare(UR"---(nullable<nullable<int>>)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Nullable",
    "innerType": {
        "$type": "STypeExp_Nullable",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "int",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, FormalNullable_FormalPtr)
{
    auto [buffer, lexer] = Prepare(UR"---(nullable<ptr<int>>)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Nullable",
    "innerType": {
        "$type": "STypeExp_Ptr",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "int",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, FormalNullable_FormalShared)
{
    auto [buffer, lexer] = Prepare(UR"---(nullable<shared<int>>)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Nullable",
    "innerType": {
        "$type": "STypeExp_Shared",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "int",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, FormalNullable_Id)
{
    auto [buffer, lexer] = Prepare(UR"---(nullable<A.B.C<int>.D>)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Nullable",
    "innerType": {
        "$type": "STypeExp_Member",
        "parentType": {
            "$type": "STypeExp_Member",
            "parentType": {
                "$type": "STypeExp_Member",
                "parentType": {
                    "$type": "STypeExp_Id",
                    "name": "A",
                    "typeArgs": []
                },
                "name": "B",
                "typeArgs": []
            },
            "name": "C",
            "typeArgs": [
                {
                    "$type": "STypeExp_Id",
                    "name": "int",
                    "typeArgs": []
                }
            ]
        },
        "name": "D",
        "typeArgs": []
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, FormalNullable_SugarLocal)
{
    auto [buffer, lexer] = Prepare(UR"---(nullable<local I>)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Nullable",
    "innerType": {
        "$type": "STypeExp_Local",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "I",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, FormalNullable_SugarNullable)
{
    auto [buffer, lexer] = Prepare(UR"---(nullable<int?>)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Nullable",
    "innerType": {
        "$type": "STypeExp_Nullable",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "int",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, FormalNullable_SugarPtr)
{
    auto [buffer, lexer] = Prepare(UR"---(nullable<int*>)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Nullable",
    "innerType": {
        "$type": "STypeExp_Ptr",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "int",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, FormalNullable_SugarShared)
{
    auto [buffer, lexer] = Prepare(UR"---(nullable<shared int>)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Nullable",
    "innerType": {
        "$type": "STypeExp_Shared",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "int",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, FormalPtr_FormalLocal)
{
    auto [buffer, lexer] = Prepare(UR"---(ptr<local<I>>)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Ptr",
    "innerType": {
        "$type": "STypeExp_Local",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "I",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, FormalPtr_FormalNullable)
{
    auto [buffer, lexer] = Prepare(UR"---(ptr<nullable<int>>)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Ptr",
    "innerType": {
        "$type": "STypeExp_Nullable",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "int",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, FormalPtr_FormalPtr)
{
    auto [buffer, lexer] = Prepare(UR"---(ptr<ptr<int>>)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Ptr",
    "innerType": {
        "$type": "STypeExp_Ptr",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "int",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, FormalPtr_FormalShared)
{
    auto [buffer, lexer] = Prepare(UR"---(ptr<shared<int>>)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Ptr",
    "innerType": {
        "$type": "STypeExp_Shared",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "int",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, FormalPtr_Id)
{
    auto [buffer, lexer] = Prepare(UR"---(ptr<A.B.C<int>.D>)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Ptr",
    "innerType": {
        "$type": "STypeExp_Member",
        "parentType": {
            "$type": "STypeExp_Member",
            "parentType": {
                "$type": "STypeExp_Member",
                "parentType": {
                    "$type": "STypeExp_Id",
                    "name": "A",
                    "typeArgs": []
                },
                "name": "B",
                "typeArgs": []
            },
            "name": "C",
            "typeArgs": [
                {
                    "$type": "STypeExp_Id",
                    "name": "int",
                    "typeArgs": []
                }
            ]
        },
        "name": "D",
        "typeArgs": []
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, FormalPtr_SugarLocal)
{
    auto [buffer, lexer] = Prepare(UR"---(ptr<local I>)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Ptr",
    "innerType": {
        "$type": "STypeExp_Local",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "I",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, FormalPtr_SugarNullable)
{
    auto [buffer, lexer] = Prepare(UR"---(ptr<int?>)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Ptr",
    "innerType": {
        "$type": "STypeExp_Nullable",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "int",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, FormalPtr_SugarPtr)
{
    auto [buffer, lexer] = Prepare(UR"---(ptr<int*>)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Ptr",
    "innerType": {
        "$type": "STypeExp_Ptr",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "int",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, FormalPtr_SugarShared)
{
    auto [buffer, lexer] = Prepare(UR"---(ptr<shared int>)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Ptr",
    "innerType": {
        "$type": "STypeExp_Shared",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "int",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, FormalShared_FormalLocal)
{
    auto [buffer, lexer] = Prepare(UR"---(shared<local<I>>)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Shared",
    "innerType": {
        "$type": "STypeExp_Local",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "I",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, FormalShared_FormalNullable)
{
    auto [buffer, lexer] = Prepare(UR"---(shared<nullable<int>>)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Shared",
    "innerType": {
        "$type": "STypeExp_Nullable",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "int",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, FormalShared_FormalPtr)
{
    auto [buffer, lexer] = Prepare(UR"---(shared<ptr<int>>)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Shared",
    "innerType": {
        "$type": "STypeExp_Ptr",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "int",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, FormalShared_FormalShared)
{
    auto [buffer, lexer] = Prepare(UR"---(shared<shared<int>>)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Shared",
    "innerType": {
        "$type": "STypeExp_Shared",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "int",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, FormalShared_Id)
{
    auto [buffer, lexer] = Prepare(UR"---(shared<A.B.C<int>.D>)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Shared",
    "innerType": {
        "$type": "STypeExp_Member",
        "parentType": {
            "$type": "STypeExp_Member",
            "parentType": {
                "$type": "STypeExp_Member",
                "parentType": {
                    "$type": "STypeExp_Id",
                    "name": "A",
                    "typeArgs": []
                },
                "name": "B",
                "typeArgs": []
            },
            "name": "C",
            "typeArgs": [
                {
                    "$type": "STypeExp_Id",
                    "name": "int",
                    "typeArgs": []
                }
            ]
        },
        "name": "D",
        "typeArgs": []
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, FormalShared_SugarLocal)
{
    auto [buffer, lexer] = Prepare(UR"---(shared<local I>)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Shared",
    "innerType": {
        "$type": "STypeExp_Local",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "I",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, FormalShared_SugarNullable)
{
    auto [buffer, lexer] = Prepare(UR"---(shared<int?>)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Shared",
    "innerType": {
        "$type": "STypeExp_Nullable",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "int",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, FormalShared_SugarPtr)
{
    auto [buffer, lexer] = Prepare(UR"---(shared<int*>)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Shared",
    "innerType": {
        "$type": "STypeExp_Ptr",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "int",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, FormalShared_SugarShared)
{
    auto [buffer, lexer] = Prepare(UR"---(shared<shared int>)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Shared",
    "innerType": {
        "$type": "STypeExp_Shared",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "int",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, SugarNullable_FormalLocal)
{
    auto [buffer, lexer] = Prepare(UR"---(local<I>?)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Nullable",
    "innerType": {
        "$type": "STypeExp_Local",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "I",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, SugarNullable_FormalNullable)
{
    auto [buffer, lexer] = Prepare(UR"---(nullable<int>?)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Nullable",
    "innerType": {
        "$type": "STypeExp_Nullable",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "int",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, SugarNullable_FormalPtr)
{
    auto [buffer, lexer] = Prepare(UR"---(ptr<int>?)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Nullable",
    "innerType": {
        "$type": "STypeExp_Ptr",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "int",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, SugarNullable_FormalShared)
{
    auto [buffer, lexer] = Prepare(UR"---(shared<int>?)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Nullable",
    "innerType": {
        "$type": "STypeExp_Shared",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "int",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, SugarNullable_Id)
{
    auto [buffer, lexer] = Prepare(UR"---(A.B.C<int>.D?)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Nullable",
    "innerType": {
        "$type": "STypeExp_Member",
        "parentType": {
            "$type": "STypeExp_Member",
            "parentType": {
                "$type": "STypeExp_Member",
                "parentType": {
                    "$type": "STypeExp_Id",
                    "name": "A",
                    "typeArgs": []
                },
                "name": "B",
                "typeArgs": []
            },
            "name": "C",
            "typeArgs": [
                {
                    "$type": "STypeExp_Id",
                    "name": "int",
                    "typeArgs": []
                }
            ]
        },
        "name": "D",
        "typeArgs": []
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, SugarNullable_SugarLocal)
{
    auto [buffer, lexer] = Prepare(UR"---(local I?)---");
    SFactory factory;

    ParseTypeExp(&lexer, factory);

    EXPECT_TRUE(!lexer.IsReachedEnd());
}

TEST(TypeExpParser, SugarNullable_SugarNullable)
{
    auto [buffer, lexer] = Prepare(UR"---(int??)---");
    SFactory factory;

    ParseTypeExp(&lexer, factory);

    EXPECT_TRUE(!lexer.IsReachedEnd());
}

TEST(TypeExpParser, SugarNullable_SugarPtr)
{
    auto [buffer, lexer] = Prepare(UR"---(int*?)---");
    SFactory factory;

    ParseTypeExp(&lexer, factory);

    EXPECT_TRUE(!lexer.IsReachedEnd());
}

TEST(TypeExpParser, SugarNullable_SugarShared)
{
    auto [buffer, lexer] = Prepare(UR"---(shared int?)---");
    SFactory factory;

    ParseTypeExp(&lexer, factory);

    EXPECT_TRUE(!lexer.IsReachedEnd());
}

TEST(TypeExpParser, SugarPtr_FormalLocal)
{
    auto [buffer, lexer] = Prepare(UR"---(local<I>*)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Ptr",
    "innerType": {
        "$type": "STypeExp_Local",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "I",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, SugarPtr_FormalNullable)
{
    auto [buffer, lexer] = Prepare(UR"---(nullable<int>*)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Ptr",
    "innerType": {
        "$type": "STypeExp_Nullable",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "int",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, SugarPtr_FormalPtr)
{
    auto [buffer, lexer] = Prepare(UR"---(ptr<int>*)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Ptr",
    "innerType": {
        "$type": "STypeExp_Ptr",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "int",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, SugarPtr_FormalShared)
{
    auto [buffer, lexer] = Prepare(UR"---(shared<int>*)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Ptr",
    "innerType": {
        "$type": "STypeExp_Shared",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "int",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, SugarPtr_Id)
{
    auto [buffer, lexer] = Prepare(UR"---(A.B.C<int>.D*)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Ptr",
    "innerType": {
        "$type": "STypeExp_Member",
        "parentType": {
            "$type": "STypeExp_Member",
            "parentType": {
                "$type": "STypeExp_Member",
                "parentType": {
                    "$type": "STypeExp_Id",
                    "name": "A",
                    "typeArgs": []
                },
                "name": "B",
                "typeArgs": []
            },
            "name": "C",
            "typeArgs": [
                {
                    "$type": "STypeExp_Id",
                    "name": "int",
                    "typeArgs": []
                }
            ]
        },
        "name": "D",
        "typeArgs": []
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, SugarPtr_SugarLocal)
{
    auto [buffer, lexer] = Prepare(UR"---(local I*)---");
    SFactory factory;

    ParseTypeExp(&lexer, factory);

    EXPECT_TRUE(!lexer.IsReachedEnd());
}

TEST(TypeExpParser, SugarPtr_SugarNullable)
{
    auto [buffer, lexer] = Prepare(UR"---(int?*)---");
    SFactory factory;

    ParseTypeExp(&lexer, factory);

    EXPECT_TRUE(!lexer.IsReachedEnd());
}

TEST(TypeExpParser, SugarPtr_SugarPtr)
{
    auto [buffer, lexer] = Prepare(UR"---(int**)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Ptr",
    "innerType": {
        "$type": "STypeExp_Ptr",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "int",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, SugarPtr_SugarShared)
{
    auto [buffer, lexer] = Prepare(UR"---(shared int*)---");
    SFactory factory;

    ParseTypeExp(&lexer, factory);

    EXPECT_TRUE(!lexer.IsReachedEnd());
}

TEST(TypeExpParser, SugarShared_FormalLocal)
{
    auto [buffer, lexer] = Prepare(UR"---(shared local<I>)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Shared",
    "innerType": {
        "$type": "STypeExp_Local",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "I",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, SugarShared_FormalNullable)
{
    auto [buffer, lexer] = Prepare(UR"---(shared nullable<int>)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Shared",
    "innerType": {
        "$type": "STypeExp_Nullable",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "int",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, SugarShared_FormalPtr)
{
    auto [buffer, lexer] = Prepare(UR"---(shared ptr<int>)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Shared",
    "innerType": {
        "$type": "STypeExp_Ptr",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "int",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, SugarShared_FormalShared)
{
    auto [buffer, lexer] = Prepare(UR"---(shared shared<int>)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Shared",
    "innerType": {
        "$type": "STypeExp_Shared",
        "innerType": {
            "$type": "STypeExp_Id",
            "name": "int",
            "typeArgs": []
        }
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, SugarShared_Id)
{
    auto [buffer, lexer] = Prepare(UR"---(shared A.B.C<int>.D)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

    auto expected = R"---({
    "$type": "STypeExp_Shared",
    "innerType": {
        "$type": "STypeExp_Member",
        "parentType": {
            "$type": "STypeExp_Member",
            "parentType": {
                "$type": "STypeExp_Member",
                "parentType": {
                    "$type": "STypeExp_Id",
                    "name": "A",
                    "typeArgs": []
                },
                "name": "B",
                "typeArgs": []
            },
            "name": "C",
            "typeArgs": [
                {
                    "$type": "STypeExp_Id",
                    "name": "int",
                    "typeArgs": []
                }
            ]
        },
        "name": "D",
        "typeArgs": []
    }
})---";

    EXPECT_TRUE(lexer.IsReachedEnd());
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, SugarShared_SugarLocal)
{
    auto [buffer, lexer] = Prepare(UR"---(shared local I)---");
    SFactory factory;

    ParseTypeExp(&lexer, factory);

    EXPECT_TRUE(!lexer.IsReachedEnd());
}

TEST(TypeExpParser, SugarShared_SugarNullable)
{
    auto [buffer, lexer] = Prepare(UR"---(shared int?)---");
    SFactory factory;

    ParseTypeExp(&lexer, factory);

    EXPECT_TRUE(!lexer.IsReachedEnd());
}

TEST(TypeExpParser, SugarShared_SugarPtr)
{
    auto [buffer, lexer] = Prepare(UR"---(shared int*)---");
    SFactory factory;

    ParseTypeExp(&lexer, factory);

    EXPECT_TRUE(!lexer.IsReachedEnd());
}

TEST(TypeExpParser, SugarShared_SugarShared)
{
    auto [buffer, lexer] = Prepare(UR"---(shared shared int)---");
    SFactory factory;

    ParseTypeExp(&lexer, factory);

    EXPECT_TRUE(!lexer.IsReachedEnd());
}

TEST(TypeExpParser, TopLevel_LocalOnly)
{
    auto [buffer, lexer] = Prepare(UR"---(local)---");
    SFactory factory;

    ParseTypeExp(&lexer, factory);

    EXPECT_TRUE(!lexer.IsReachedEnd());
}

TEST(TypeExpParser, TopLevel_NullableOnly)
{
    auto [buffer, lexer] = Prepare(UR"---(nullable)---");
    SFactory factory;

    ParseTypeExp(&lexer, factory);

    EXPECT_TRUE(!lexer.IsReachedEnd());
}

TEST(TypeExpParser, TopLevel_ParseIdChain)
{
    auto [buffer, lexer] = Prepare(UR"---(A.B<int>.C)---");
    SFactory factory;

    auto* typeExp = ParseTypeExp(&lexer, factory);

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
    EXPECT_SYNTAX_EQ(typeExp, expected);
}

TEST(TypeExpParser, TopLevel_PtrOnly)
{
    auto [buffer, lexer] = Prepare(UR"---(ptr)---");
    SFactory factory;

    ParseTypeExp(&lexer, factory);

    EXPECT_TRUE(!lexer.IsReachedEnd());
}

TEST(TypeExpParser, TopLevel_SharedOnly)
{
    auto [buffer, lexer] = Prepare(UR"---(shared)---");
    SFactory factory;

    ParseTypeExp(&lexer, factory);

    EXPECT_TRUE(!lexer.IsReachedEnd());
}

