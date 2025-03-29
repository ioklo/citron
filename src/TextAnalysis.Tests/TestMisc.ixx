export module Citron.TestMisc;

import <tuple>;
import <memory>;
import <string>;
import <optional>;

import Citron.StringWriter;
import Citron.make_vector;

import Citron.Buffer;
import Citron.Lexer;
import Citron.Syntax;
import Citron.Json;

namespace Citron {

export std::tuple<std::shared_ptr<Buffer>, Lexer> Prepare(std::u32string str);

export template<typename TSyntax>
std::string ToJsonString(TSyntax& syntax)
{
    StringWriter writer;
    ToString(ToJson(syntax), writer);
    return writer.ToString();
}


export template<typename TSyntax>
std::string ToJsonString(std::optional<TSyntax>& oSyntax)
{
    if (oSyntax)
        return ToJsonString(*oSyntax);
    else
        return "null";
}

}


