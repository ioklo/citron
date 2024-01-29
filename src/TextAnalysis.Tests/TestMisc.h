#pragma once

#include <tuple>
#include <memory>
#include <string>

#include <Infra/StringWriter.h>
#include <TextAnalysis/Lexer.h>
#include <Syntax/ScriptSyntax.h>
#include <Syntax/ScriptSyntaxElements.h>
#include <Syntax/GlobalFuncDeclSyntax.h>


namespace Citron {
class Buffer;
class Lexer;

std::tuple<std::shared_ptr<Buffer>, Lexer> Prepare(std::u32string str);

inline ScriptSyntax SScript(std::vector<StmtSyntax> stmts)
{
    // void Main() { stmts }
    return ScriptSyntax({
        GlobalFuncDeclScriptSyntaxElement(
            GlobalFuncDeclSyntax(
                std::nullopt, false, IdTypeExpSyntax(U"void"), U"Main", {}, {}, std::move(stmts)
            )
        )
    });
}

inline IdTypeExpSyntax SVoidTypeExp()
{
    return IdTypeExpSyntax(U"void");
}

inline IdTypeExpSyntax SIntTypeExp()
{
    return IdTypeExpSyntax(U"int");
}

inline IdTypeExpSyntax SIdTypeExp(std::u32string name)
{
    return IdTypeExpSyntax(std::move(name));
}

inline ScriptSyntax SScript(std::vector<ScriptSyntaxElement> elems)
{
    // void Main() { stmts }
    return Citron::ScriptSyntax(std::move(elems));
}

inline VarDeclSyntax SVarDecl(TypeExpSyntax typeExp, std::u32string name, std::optional<ExpSyntax> initExp = std::nullopt)
{
    return VarDeclSyntax{ std::move(typeExp), {VarDeclSyntaxElement{std::move(name), std::move(initExp)}}};
}

inline VarDeclStmtSyntax SVarDeclStmt(TypeExpSyntax typeExp, std::u32string name, std::optional<ExpSyntax> initExp = std::nullopt)
{
    return VarDeclStmtSyntax(SVarDecl(typeExp, name, initExp));
}

inline IdentifierExpSyntax SId(std::u32string name)
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

#define EXPECT_SYNTAX_EQ(x, y) EXPECT_EQ(ToJsonString(x), ToJsonString(y))
