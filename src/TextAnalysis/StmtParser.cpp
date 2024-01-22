#include "pch.h"
#include "StmtParser.h"

#include <optional>
#include <algorithm>

#include "Syntax/StmtSyntaxes.h"
#include "Syntax/StringExpElementSyntaxes.h"
#include "Syntax/ExpSyntaxes.h"

#include "TypeExpParser.h"
#include "ExpParser.h"
#include "ParserMisc.h"
#include <cassert>

using namespace std;

namespace Citron {

optional<EmbeddableStmtSyntax> ParseEmbeddableStmt(Lexer* lexer);

// typeExp id = exp)
optional<IfTestStmtSyntax> ParseIfTestFragment(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    auto oTestTypeExp = ParseTypeExp(&curLexer);
    if (!oTestTypeExp)
        return nullopt;

    auto oVarNameToken = Accept<IdentifierToken>(&curLexer);
    if (!oVarNameToken)
        return nullopt;

    if (!Accept<EqualToken>(&curLexer))
        return nullopt;

    auto oExp = ParseExp(&curLexer);
    if (!oExp)
        return nullopt;

    if (!Accept<RParenToken>(&curLexer))
        return nullopt;
    
    // right assoc, conflict는 별다른 처리를 하지 않고 지나가면 될 것 같다
    auto oBody = ParseEmbeddableStmt(&curLexer);
    if (!oBody)
        return nullopt;

    optional<EmbeddableStmtSyntax> oElseBody;

    if (Accept<ElseToken>(&curLexer))
    {
        oElseBody = ParseEmbeddableStmt(&curLexer);
        if (!oElseBody)
            return nullopt;
    }

    *lexer = std::move(curLexer);
    return IfTestStmtSyntax(std::move(*oTestTypeExp), oVarNameToken->text, std::move(*oExp), std::move(*oBody), std::move(oElseBody));
}

// 리턴은 IfStmtSyntax와 IfTestStmtSyntax
optional<StmtSyntax> ParseIfStmt(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    // if (exp) stmt => If(exp, stmt, null)
    // if (exp) stmt0 else stmt1 => If(exp, stmt0, stmt1)
    // if (exp0) if (exp1) stmt1 else stmt2 => If(exp0, If(exp1, stmt1, stmt2))
    // if (typeExp name = exp) => IfTestStmt(TypeExp, name, exp)

    if (!Accept<IfToken>(&curLexer))
        return nullopt;

    if (!Accept<LParenToken>(&curLexer))
        return nullopt;

    // typeExp varName = exp꼴인지 먼저 확인
    if (auto oIfTestStmtSyntax = ParseIfTestFragment(&curLexer))
    {
        *lexer = std::move(curLexer);
        return oIfTestStmtSyntax;
    }

    // 아니라면
    auto oCond = ParseExp(&curLexer);
    if (!oCond)
        return nullopt;

    if (!Accept<RParenToken>(&curLexer))
        return nullopt;

    // right assoc, conflict는 별다른 처리를 하지 않고 지나가면 될 것 같다
    auto oBody = ParseEmbeddableStmt(&curLexer);
    if (!oBody)
        return nullopt;

    optional<EmbeddableStmtSyntax> oElseBody;
    
    if (Accept<ElseToken>(&curLexer))
    {
        oElseBody = ParseEmbeddableStmt(&curLexer);
        if (!oElseBody)
            return nullopt;
    }

    *lexer = std::move(curLexer);
    return IfStmtSyntax(std::move(*oCond), std::move(*oBody), std::move(oElseBody));
}

optional<VarDeclSyntax> ParseVarDecl(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    auto oVarType = ParseTypeExp(&curLexer);
    if (!oVarType)
        return nullopt;

    vector<VarDeclElementSyntax> elems;
        
    do
    {
        auto oVarIdToken = Accept<IdentifierToken>(&curLexer);
        if (!oVarIdToken)
            return nullopt;

        optional<ExpSyntax> oInitExp;

        if (Accept<EqualToken>(&curLexer))
        {
            // TODO: ;나 ,가 나올때까지라는걸 명시해주면 좋겠다
            oInitExp = ParseExp(&curLexer);
            if (!oInitExp)
                return nullopt;
        }

        elems.push_back(VarDeclElementSyntax{ std::move(oVarIdToken->text), std::move(oInitExp) });

    } while (Accept<CommaToken>(&curLexer)); // ,가 나오면 계속한다

    *lexer = std::move(curLexer);
    return VarDeclSyntax{ *oVarType, std::move(elems) };
}

// int x = 0;
optional<VarDeclStmtSyntax> ParseVarDeclStmt(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    auto oVarDecl = ParseVarDecl(&curLexer);
    if (!oVarDecl)
        return nullopt;

    if (!Accept<SemiColonToken>(&curLexer))
        return nullopt;

    *lexer = std::move(curLexer);
    return VarDeclStmtSyntax(std::move(*oVarDecl));
}

optional<ForStmtInitializerSyntax> ParseForStmtInitializer(Lexer* lexer)
{
    if (auto oVarDecl = ParseVarDecl(lexer))
        return VarDeclForStmtInitializerSyntax{ std::move(*oVarDecl) };

    if (auto oExp = ParseExp(lexer))
        return ExpForStmtInitializerSyntax{ std::move(*oExp) };    

    return nullopt;
}

optional<ForStmtSyntax> ParseForStmt(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    if (!Accept<ForToken>(&curLexer))
        return nullopt;

    if (!Accept<LParenToken>(&curLexer))
        return nullopt;

    // TODO: 이 Initializer의 끝은 ';' 이다
    auto oInitializer = ParseForStmtInitializer(&curLexer);

    if (!Accept<SemiColonToken>(&curLexer))
        return nullopt;

    // TODO: 이 CondExp의 끝은 ';' 이다
    auto oCond = ParseExp(&curLexer);

    if (!Accept<SemiColonToken>(&curLexer))
        return nullopt;

    // TODO: 이 CondExp의 끝은 ')' 이다            
    auto oCont = ParseExp(&curLexer);

    if (!Accept<RParenToken>(&curLexer))
        return nullopt;

    auto oBodyStmt = ParseEmbeddableStmt(&curLexer);
    if (!oBodyStmt)
        return nullopt;

    *lexer = std::move(curLexer);
    return ForStmtSyntax(std::move(oInitializer), std::move(oCond), std::move(oCont), std::move(*oBodyStmt));
}

optional<ContinueStmtSyntax> ParseContinueStmt(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    if (!Accept<ContinueToken>(&curLexer))
        return nullopt;

    if (!Accept<SemiColonToken>(&curLexer))
        return nullopt;

    *lexer = std::move(curLexer);
    return ContinueStmtSyntax();
}

optional<BreakStmtSyntax> ParseBreakStmt(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    if (!Accept<BreakToken>(&curLexer))
        return nullopt;

    if (!Accept<SemiColonToken>(&curLexer))
        return nullopt;

    *lexer = std::move(curLexer);
    return BreakStmtSyntax();
}

optional<ReturnStmtSyntax> ParseReturnStmt(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    if (!Accept<ReturnToken>(&curLexer))
        return nullopt;

    optional<ReturnValueInfoSyntax> oReturnValue;

    if (auto oReturnExp = ParseExp(&curLexer))
        oReturnValue = ReturnValueInfoSyntax{ std::move(*oReturnExp) };

    if (!Accept<SemiColonToken>(&curLexer))
        return nullopt;

    *lexer = std::move(curLexer);
    return ReturnStmtSyntax(std::move(oReturnValue));
}

optional<BlockStmtSyntax> ParseBlockStmt(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    if (!Accept<LBraceToken>(&curLexer))
        return nullopt;

    vector<StmtSyntax> stmts;
    
    while (!Accept<RBraceToken>(&curLexer))
    {
        auto oStmt = ParseStmt(&curLexer);
        if (!oStmt) return nullopt;

        stmts.push_back(std::move(*oStmt));
    }

    *lexer = std::move(curLexer);
    return BlockStmtSyntax(std::move(stmts));
}

optional<BlankStmtSyntax> ParseBlankStmt(Lexer* lexer)
{
    if (!Accept<SemiColonToken>(lexer))
        return nullopt;

    return BlankStmtSyntax();
}

// TODO: Assign, Call만 가능하게 해야 한다
optional<ExpStmtSyntax> ParseExpStmt(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    auto oExp = ParseExp(&curLexer);
    if (!oExp)
        return nullopt;
    
    if (!Accept<SemiColonToken>(&curLexer))
        return nullopt;

    *lexer = std::move(curLexer);
    return ExpStmtSyntax(std::move(*oExp));
}

optional<TaskStmtSyntax> ParseTaskStmt(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    if (!Accept<TaskToken>(&curLexer))
        return nullopt;

    auto oBody = ParseBody(&curLexer);
    
    if (!oBody)
        return nullopt;

    *lexer = std::move(curLexer);
    return TaskStmtSyntax(std::move(*oBody));
}

optional<AwaitStmtSyntax> ParseAwaitStmt(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    if (!Accept<AwaitToken>(&curLexer))
        return nullopt;
    
    auto oBody = ParseBody(&curLexer);
    if (!oBody)
        return nullopt;

    *lexer = std::move(curLexer);
    return AwaitStmtSyntax(std::move(*oBody));
}

optional<AsyncStmtSyntax> ParseAsyncStmt(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    if (!Accept<AsyncToken>(&curLexer))
        return nullopt;

    auto oBody = ParseBody(&curLexer);
    if (!oBody)
        return nullopt;

    *lexer = std::move(curLexer);
    return AsyncStmtSyntax(std::move(*oBody));
}

optional<YieldStmtSyntax> ParseYieldStmt(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    if (!Accept<YieldToken>(&curLexer))
        return nullopt;

    auto oYieldValue = ParseExp(&curLexer);

    if (!oYieldValue)
        return nullopt;

    if (!Accept<SemiColonToken>(&curLexer))
        return nullopt;

    *lexer = std::move(curLexer);
    return YieldStmtSyntax(std::move(*oYieldValue));
}

optional<StringExpSyntax> ParseSingleCommand(bool bStopRBrace, Lexer* lexer)
{
    Lexer curLexer = *lexer;

    vector<StringExpElementSyntax> elems;

    // 새 줄이거나 끝에 다다르면 종료
    while (!curLexer.IsReachedEnd())
    {
        if (bStopRBrace && Peek<RBraceToken>(curLexer.LexCommandMode()))
            break;

        if (Accept<NewLineToken>(&curLexer, curLexer.LexCommandMode()))
            break;

        // ${ 이 나오면 
        if (Accept<DollarLBraceToken>(&curLexer, curLexer.LexCommandMode()))
        {
            // TODO: EndInnerExpToken 일때 빠져나와야 한다는 표시를 해줘야 한다
            auto oExp = ParseExp(&curLexer);
            if (!oExp)
                return nullopt;

            if (!Accept<RBraceToken>(&curLexer))
                return nullopt;

            elems.push_back(ExpStringExpElementSyntax{ std::move(*oExp) });
            continue;
        }

        // aa$b => $b 이야기
        if (auto oIdToken = Accept<IdentifierToken>(&curLexer, curLexer.LexCommandMode()))
        {
            elems.push_back(ExpStringExpElementSyntax{ IdentifierExpSyntax(std::move(oIdToken->text), {}) });
            continue;
        }

        
        if (auto oTextToken = Accept<TextToken>(&curLexer, curLexer.LexCommandMode()))
        {
            elems.push_back(TextStringExpElementSyntax{ std::move(oTextToken->text) });
            continue;
        }

        return nullopt;
    }

    *lexer = std::move(curLexer);
    return StringExpSyntax(std::move(elems));
}

optional<ForeachStmtSyntax> ParseForeachStmt(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    // foreach
    if (!Accept<ForeachToken>(&curLexer))
        return nullopt;

    // (
    if (!Accept<LParenToken>(&curLexer))
        return nullopt;
    

    // var 
    auto oTypeExp = ParseTypeExp(&curLexer);
    if (!oTypeExp)
        return nullopt;

    // x
    auto oVarNameToken = Accept<IdentifierToken>(&curLexer);
    if (!oVarNameToken)
        return nullopt;

    // in
    if (!Accept<InToken>(&curLexer))
        return nullopt;

    // obj
    auto oObj = ParseExp(&curLexer);
    if (!oObj)
        return nullopt;

    // )
    if (!Accept<RParenToken>(&curLexer))
        return nullopt;

    // stmt
    auto oStmt = ParseEmbeddableStmt(&curLexer);
    if (!oStmt)
        return nullopt;

    *lexer = std::move(curLexer);
    return ForeachStmtSyntax(std::move(*oTypeExp), std::move(oVarNameToken->text), std::move(*oObj), std::move(*oStmt));
}

// 
optional<CommandStmtSyntax> ParseCommandStmt(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    // @로 시작한다
    if (!Accept<AtToken>(&curLexer))
        return nullopt;

    // TODO: optional ()

    // {로 시작한다면 MultiCommand, } 가 나오면 끝난다
    if (Accept<LBraceToken>(&curLexer))
    {
        // 새줄이거나 끝에 다다르거나 }가 나오면 종료,
        vector<StringExpSyntax> cmds;
        while (true)
        {
            if (Accept<RBraceToken>(&curLexer, curLexer.LexCommandMode()))
                break;

            auto oSingleCommand = ParseSingleCommand(true, &curLexer);

            if (oSingleCommand)
            {
                // singleCommand Skip 조건
                size_t elemCount = oSingleCommand->GetElements().size();

                if (elemCount == 0)
                    continue;

                if (elemCount == 1)
                {
                    if (auto* textElem = get_if<TextStringExpElementSyntax>(&oSingleCommand->GetElements()[0]))
                    {
                        if (all_of(textElem->text.begin(), textElem->text.end(), [](char32_t c) { return u_isWhitespace(c); }))
                            continue;
                    }
                }

                cmds.push_back(std::move(*oSingleCommand));
                continue;
            }

            return nullopt;
        }

        *lexer = std::move(curLexer);
        return CommandStmtSyntax(std::move(cmds));
    }
    else // 싱글 커맨드, 엔터가 나오면 끝난다
    {
        auto oSingleCommand = ParseSingleCommand(false, &curLexer);

        if (!oSingleCommand)
            return nullopt;

        if (oSingleCommand->GetElements().empty())
            return nullopt;
        
        *lexer = std::move(curLexer);
        return CommandStmtSyntax({ std::move(*oSingleCommand) });
    }
}

optional<DirectiveStmtSyntax> ParseDirectiveStmt(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    // ` <id> ( exp... );
    if (!Accept<BacktickToken>(&curLexer))
        return nullopt;

    auto oIdToken = Accept<IdentifierToken>(&curLexer);
    if (!oIdToken)
        return nullopt;

    if (!Accept<LParenToken>(&curLexer))
        return nullopt;

    vector<ExpSyntax> args;    
    while (!Accept<RParenToken>(&curLexer))
    {
        if (!args.empty())
            if (!Accept<CommaToken>(&curLexer))
                return nullopt;

        auto oArg = ParseExp(&curLexer);
        if (!oArg)
            return nullopt;

        args.push_back(std::move(*oArg));
    }

    if (!Accept<SemiColonToken>(&curLexer))
        return nullopt;

    *lexer = std::move(curLexer);
    return DirectiveStmtSyntax(std::move(oIdToken->text), std::move(args));
}

// if (...) 'x;' // 단일이냐
// if (...) '{ }' // 묶음이냐
optional<EmbeddableStmtSyntax> ParseEmbeddableStmt(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    // { 가 없다면, Embeddable.Single
    if (!Accept<LBraceToken>(&curLexer))
    {
        auto oStmt = ParseStmt(&curLexer);
        if (!oStmt)
            return nullopt;
        
        // block stmt는 제외되서 들어올 것이다
        assert(!holds_alternative<BlockStmtSyntax>(*oStmt));

        *lexer = std::move(curLexer);
        return SingleEmbeddableStmtSyntax(std::move(*oStmt));
    }
    else // 있다면 Embeddable.Multiple
    {
        vector<StmtSyntax> stmts;

        // } 가 나올때까지
        while (!Accept<RBraceToken>(&curLexer))
        {
            auto oStmt = ParseStmt(&curLexer);

            if (!oStmt)
                return nullopt;

            stmts.push_back(std::move(*oStmt));
        }

        *lexer = std::move(curLexer);
        return BlockEmbeddableStmtSyntax(std::move(stmts));
    }
}

optional<vector<StmtSyntax>> ParseBody(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    if (!Accept<LBraceToken>(&curLexer))
        return nullopt;

    vector<StmtSyntax> stmts;
    while (!Accept<RBraceToken>(&curLexer))
    {
        auto oStmt = ParseStmt(&curLexer);
        if (!oStmt)
            return nullopt;

        stmts.push_back(std::move(*oStmt));
    }

    *lexer = std::move(curLexer);
    return stmts;
}

optional<StmtSyntax> ParseStmt(Lexer* lexer)
{
    if (auto oStmt = ParseDirectiveStmt(lexer))
        return oStmt;
    

    if (auto oStmt = ParseBlankStmt(lexer))
        return oStmt;

    if (auto oStmt = ParseBlockStmt(lexer))
        return oStmt;

    if (auto oStmt = ParseContinueStmt(lexer))
        return oStmt;

    if (auto oStmt = ParseBreakStmt(lexer))
        return oStmt;

    if (auto oStmt = ParseReturnStmt(lexer))
        return oStmt;

    if (auto oStmt = ParseVarDeclStmt(lexer))
        return oStmt;

    if (auto oStmt = ParseIfStmt(lexer))
        return oStmt;

    if (auto oStmt = ParseForStmt(lexer))
        return oStmt;

    if (auto oStmt = ParseExpStmt(lexer))
        return oStmt;

    if (auto oStmt = ParseTaskStmt(lexer))
        return oStmt;

    if (auto oStmt = ParseAwaitStmt(lexer))
        return oStmt;

    if (auto oStmt = ParseAsyncStmt(lexer))
        return oStmt;

    if (auto oStmt = ParseForeachStmt(lexer))
        return oStmt;

    if (auto oStmt = ParseYieldStmt(lexer))
        return oStmt;

    if (auto oStmt = ParseCommandStmt(lexer))
        return oStmt;

    return nullopt;
}



}
