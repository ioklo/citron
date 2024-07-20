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

inline ScriptSyntax SScript(std::vector<StmtSyntax> stmts)
{
    // void Main() { stmts }
    return ScriptSyntax(tcb::make_vector<ScriptSyntaxElement>(
        GlobalFuncDeclSyntax(
            std::nullopt, false, IdTypeExpSyntax("void"), "Main", {}, {}, std::move(stmts)
        )
    ));
}

inline IdTypeExpSyntax SVoidTypeExp()
{
    return IdTypeExpSyntax("void");
}

inline IdTypeExpSyntax SIntTypeExp()
{
    return IdTypeExpSyntax("int");
}

inline IdTypeExpSyntax SIdTypeExp(std::string name)
{
    return IdTypeExpSyntax(std::move(name));
}

inline ScriptSyntax SScript(std::vector<ScriptSyntaxElement> elems)
{
    // void Main() { stmts }
    return Citron::ScriptSyntax(std::move(elems));
}

inline VarDeclSyntax SVarDecl(TypeExpSyntax typeExp, std::string name, std::optional<SExpPtr> initExp = std::nullopt)
{
    return VarDeclSyntax{ std::move(typeExp), tcb::make_vector(VarDeclSyntaxElement(std::move(name), std::move(initExp))) };
}

inline VarDeclStmtSyntax SVarDeclStmt(TypeExpSyntax typeExp, std::string name, std::optional<SExpPtr> initExp = std::nullopt)
{
    return VarDeclStmtSyntax(SVarDecl(std::move(typeExp), std::move(name), std::move(initExp)));
}

inline IdentifierExpSyntax SId(std::string name)
{
    return IdentifierExpSyntax(std::move(name));
}

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
