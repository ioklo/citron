#pragma once

#include <tuple>
#include <memory>
#include <string>

#include <Infra/StringWriter.h>
#include <Infra/make_vector.h>
#include <TextAnalysis/Lexer.h>
#include <Syntax/Syntax.h>

namespace Citron {
class Buffer;
class Lexer;

std::tuple<std::shared_ptr<Buffer>, Lexer> Prepare(std::u32string str);

template<typename TSyntax>
std::string ToJsonString(TSyntax& syntax)
{
    StringWriter writer;
    ToString(ToJson(syntax), writer);
    return writer.ToString();
}


template<typename TSyntax>
std::string ToJsonString(std::optional<TSyntax>& oSyntax)
{
    if (oSyntax)
        return ToJsonString(*oSyntax);
    else
        return "null";
}

}

#define EXPECT_SYNTAX_EQ(x, expected) EXPECT_EQ(ToJsonString(x), expected)
