#pragma once

#define EXPECT_SYNTAX_EQ(x, expected) EXPECT_EQ(ToJsonString(x), expected)