#include "pch.h"
#include "StmtParser.h"

#include <optional>
#include <algorithm>
#include <cassert>

#include <unicode/uchar.h>

#include <Infra/Ptr.h>
#include <Syntax/Syntax.h>

#include "ExpParser.h"
#include "TypeExpParser.h"
#include "ParserMisc.h"

using namespace std;

namespace Citron {

shared_ptr<SEmbeddableStmt> ParseEmbeddableStmt(Lexer* lexer);

// typeExp id = exp)
shared_ptr<SStmt_IfTest> ParseIfTestFragment(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    auto testTypeExp = ParseTypeExp(&curLexer);
    if (!testTypeExp)
        return nullptr;

    auto oVarNameToken = Accept<IdentifierToken>(&curLexer);
    if (!oVarNameToken)
        return nullptr;

    if (!Accept<EqualToken>(&curLexer))
        return nullptr;

    auto exp = ParseExp(&curLexer);
    if (!exp)
        return nullptr;

    if (!Accept<RParenToken>(&curLexer))
        return nullptr;
    
    // right assoc, conflict는 별다른 처리를 하지 않고 지나가면 될 것 같다
    auto body = ParseEmbeddableStmt(&curLexer);
    if (!body)
        return nullptr;

    shared_ptr<SEmbeddableStmt> elseBody;

    if (Accept<ElseToken>(&curLexer))
    {
        elseBody = ParseEmbeddableStmt(&curLexer);
        if (!elseBody)
            return nullptr;
    }

    *lexer = std::move(curLexer);
    return MakePtr<SStmt_IfTest>(std::move(testTypeExp), std::move(oVarNameToken->text), std::move(exp), std::move(body), std::move(elseBody));
}

// 리턴은 SStmt_If와 SStmt_IfTest
SStmtPtr ParseIfStmt(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    // if (exp) stmt => If(exp, stmt, null)
    // if (exp) stmt0 else stmt1 => If(exp, stmt0, stmt1)
    // if (exp0) if (exp1) stmt1 else stmt2 => If(exp0, If(exp1, stmt1, stmt2))
    // if (typeExp name = exp) => IfTestStmt(TypeExp, name, exp)

    if (!Accept<IfToken>(&curLexer))
        return nullptr;

    if (!Accept<LParenToken>(&curLexer))
        return nullptr;

    // typeExp varName = exp꼴인지 먼저 확인
    if (auto ifTestStmt = ParseIfTestFragment(&curLexer))
    {
        *lexer = std::move(curLexer);
        return ifTestStmt;
    }

    // 아니라면
    auto cond = ParseExp(&curLexer);
    if (!cond)
        return nullptr;

    if (!Accept<RParenToken>(&curLexer))
        return nullptr;

    // right assoc, conflict는 별다른 처리를 하지 않고 지나가면 될 것 같다
    auto body = ParseEmbeddableStmt(&curLexer);
    if (!body)
        return nullptr;

    SEmbeddableStmtPtr elseBody;
    
    if (Accept<ElseToken>(&curLexer))
    {
        elseBody = ParseEmbeddableStmt(&curLexer);
        if (!elseBody)
            return nullptr;
    }

    *lexer = std::move(curLexer);
    return MakePtr<SStmt_If>(std::move(cond), std::move(body), std::move(elseBody));
}

optional<SVarDecl> ParseVarDecl(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    auto varType = ParseTypeExp(&curLexer);
    if (!varType)
        return nullopt;

    vector<SVarDeclElement> elems;
        
    do
    {
        auto oVarIdToken = Accept<IdentifierToken>(&curLexer);
        if (!oVarIdToken)
            return nullopt;

        SExpPtr initExp;

        if (Accept<EqualToken>(&curLexer))
        {
            // TODO: ;나 ,가 나올때까지라는걸 명시해주면 좋겠다
            initExp = ParseExp(&curLexer);
            if (!initExp)
                return nullopt;
        }

        elems.push_back(SVarDeclElement{ std::move(oVarIdToken->text), std::move(initExp) });

    } while (Accept<CommaToken>(&curLexer)); // ,가 나오면 계속한다

    *lexer = std::move(curLexer);
    return SVarDecl(std::move(varType), std::move(elems));
}

// int x = 0;
shared_ptr<SStmt_VarDecl> ParseVarDeclStmt(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    auto oVarDecl = ParseVarDecl(&curLexer);
    if (!oVarDecl)
        return nullptr;

    if (!Accept<SemiColonToken>(&curLexer))
        return nullptr;

    *lexer = std::move(curLexer);
    return MakePtr<SStmt_VarDecl>(std::move(*oVarDecl));
}

SForStmtInitializerPtr ParseForStmtInitializer(Lexer* lexer)
{
    if (auto oVarDecl = ParseVarDecl(lexer))
        return MakePtr<SForStmtInitializer_VarDecl>(std::move(*oVarDecl));

    if (auto exp = ParseExp(lexer))
        return MakePtr<SForStmtInitializer_Exp>(std::move(exp));

    return nullptr;
}

shared_ptr<SStmt_For> ParseForStmt(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    if (!Accept<ForToken>(&curLexer))
        return nullptr;

    if (!Accept<LParenToken>(&curLexer))
        return nullptr;

    // TODO: 이 Initializer의 끝은 ';' 이다
    auto initializer = ParseForStmtInitializer(&curLexer);

    if (!Accept<SemiColonToken>(&curLexer))
        return nullptr;

    // TODO: 이 CondExp의 끝은 ';' 이다
    auto cond = ParseExp(&curLexer);

    if (!Accept<SemiColonToken>(&curLexer))
        return nullptr;

    // TODO: 이 CondExp의 끝은 ')' 이다            
    auto cont = ParseExp(&curLexer);

    if (!Accept<RParenToken>(&curLexer))
        return nullptr;

    auto bodyStmt = ParseEmbeddableStmt(&curLexer);
    if (!bodyStmt)
        return nullptr;

    *lexer = std::move(curLexer);
    return MakePtr<SStmt_For>(std::move(initializer), std::move(cond), std::move(cont), std::move(bodyStmt));
}

shared_ptr<SStmt_Continue> ParseContinueStmt(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    if (!Accept<ContinueToken>(&curLexer))
        return nullptr;

    if (!Accept<SemiColonToken>(&curLexer))
        return nullptr;

    *lexer = std::move(curLexer);
    return MakePtr<SStmt_Continue>();
}

shared_ptr<SStmt_Break> ParseBreakStmt(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    if (!Accept<BreakToken>(&curLexer))
        return nullptr;

    if (!Accept<SemiColonToken>(&curLexer))
        return nullptr;

    *lexer = std::move(curLexer);
    return MakePtr<SStmt_Break>();
}

shared_ptr<SStmt_Return> ParseReturnStmt(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    if (!Accept<ReturnToken>(&curLexer))
        return nullptr;

    SExpPtr returnValue;

    if (auto returnExp = ParseExp(&curLexer))
        returnValue = std::move(returnExp);

    if (!Accept<SemiColonToken>(&curLexer))
        return nullptr;

    *lexer = std::move(curLexer);
    return MakePtr<SStmt_Return>(std::move(returnValue));
}

shared_ptr<SStmt_Block> ParseBlockStmt(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    if (!Accept<LBraceToken>(&curLexer))
        return nullptr;

    vector<SStmtPtr> stmts;
    
    while (!Accept<RBraceToken>(&curLexer))
    {
        auto stmt = ParseStmt(&curLexer);
        if (!stmt) return nullptr;

        stmts.push_back(std::move(stmt));
    }

    *lexer = std::move(curLexer);
    return MakePtr<SStmt_Block>(std::move(stmts));
}

shared_ptr<SStmt_Blank> ParseBlankStmt(Lexer* lexer)
{
    if (!Accept<SemiColonToken>(lexer))
        return nullptr;

    return MakePtr<SStmt_Blank>();
}

// TODO: Assign, Call만 가능하게 해야 한다
shared_ptr<SStmt_Exp> ParseExpStmt(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    auto exp = ParseExp(&curLexer);
    if (!exp)
        return nullptr;
    
    if (!Accept<SemiColonToken>(&curLexer))
        return nullptr;

    *lexer = std::move(curLexer);
    return MakePtr<SStmt_Exp>(std::move(exp));
}

shared_ptr<SStmt_Task> ParseTaskStmt(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    if (!Accept<TaskToken>(&curLexer))
        return nullptr;

    auto oBody = ParseBody(&curLexer);
    
    if (!oBody)
        return nullptr;

    *lexer = std::move(curLexer);
    return MakePtr<SStmt_Task>(std::move(*oBody));
}

shared_ptr<SStmt_Await> ParseAwaitStmt(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    if (!Accept<AwaitToken>(&curLexer))
        return nullptr;
    
    auto oBody = ParseBody(&curLexer);
    if (!oBody)
        return nullptr;

    *lexer = std::move(curLexer);
    return MakePtr<SStmt_Await>(std::move(*oBody));
}

shared_ptr<SStmt_Async> ParseAsyncStmt(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    if (!Accept<AsyncToken>(&curLexer))
        return nullptr;

    auto oBody = ParseBody(&curLexer);
    if (!oBody)
        return nullptr;

    *lexer = std::move(curLexer);
    return MakePtr<SStmt_Async>(std::move(*oBody));
}

shared_ptr<SStmt_Yield> ParseYieldStmt(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    if (!Accept<YieldToken>(&curLexer))
        return nullptr;

    auto yieldValue = ParseExp(&curLexer);

    if (!yieldValue)
        return nullptr;

    if (!Accept<SemiColonToken>(&curLexer))
        return nullptr;

    *lexer = std::move(curLexer);
    return MakePtr<SStmt_Yield>(std::move(yieldValue));
}

shared_ptr<SExp_String> ParseSingleCommand(bool bStopRBrace, Lexer* lexer)
{
    Lexer curLexer = *lexer;

    vector<SStringExpElementPtr> elems;

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
            auto exp = ParseExp(&curLexer);
            if (!exp)
                return nullptr;

            if (!Accept<RBraceToken>(&curLexer))
                return nullptr;

            elems.push_back(MakePtr<SStringExpElement_Exp>(std::move(exp)));
            continue;
        }

        // aa$b => $b 이야기
        if (auto oIdToken = Accept<IdentifierToken>(&curLexer, curLexer.LexCommandMode()))
        {
            elems.push_back(MakePtr<SStringExpElement_Exp>(MakePtr<SExp_Identifier>(std::move(oIdToken->text), std::vector<STypeExpPtr>{})));
            continue;
        }

        
        if (auto oTextToken = Accept<TextToken>(&curLexer, curLexer.LexCommandMode()))
        {
            elems.push_back(MakePtr<SStringExpElement_Text>(std::move(oTextToken->text)));
            continue;
        }

        return nullptr;
    }

    *lexer = std::move(curLexer);
    return MakePtr<SExp_String>(std::move(elems));
}

shared_ptr<SStmt_Foreach> ParseForeachStmt(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    // foreach
    if (!Accept<ForeachToken>(&curLexer))
        return nullptr;

    // (
    if (!Accept<LParenToken>(&curLexer))
        return nullptr;
    

    // var 
    auto typeExp = ParseTypeExp(&curLexer);
    if (!typeExp)
        return nullptr;

    // x
    auto oVarNameToken = Accept<IdentifierToken>(&curLexer);
    if (!oVarNameToken)
        return nullptr;

    // in
    if (!Accept<InToken>(&curLexer))
        return nullptr;

    // obj
    auto obj = ParseExp(&curLexer);
    if (!obj)
        return nullptr;

    // )
    if (!Accept<RParenToken>(&curLexer))
        return nullptr;

    // stmt
    auto stmt = ParseEmbeddableStmt(&curLexer);
    if (!stmt)
        return nullptr;

    *lexer = std::move(curLexer);
    return MakePtr<SStmt_Foreach>(std::move(typeExp), std::move(oVarNameToken->text), std::move(obj), std::move(stmt));
}

// 
shared_ptr<SStmt_Command> ParseCommandStmt(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    // @로 시작한다
    if (!Accept<AtToken>(&curLexer))
        return nullptr;

    // TODO: optional ()

    // {로 시작한다면 MultiCommand, } 가 나오면 끝난다
    if (Accept<LBraceToken>(&curLexer))
    {
        // 새줄이거나 끝에 다다르거나 }가 나오면 종료,
        vector<shared_ptr<SExp_String>> cmds;
        while (true)
        {
            if (Accept<RBraceToken>(&curLexer, curLexer.LexCommandMode()))
                break;

            auto singleCommand = ParseSingleCommand(true, &curLexer);

            if (singleCommand)
            {
                // singleCommand Skip 조건
                size_t elemCount = singleCommand->elements.size();

                if (elemCount == 0)
                    continue;

                if (elemCount == 1)
                {
                    auto* elem = singleCommand->elements[0].get();
                    if (SStringExpElement_Text* textElem = dynamic_cast<SStringExpElement_Text*>(elem))
                    {
                        if (all_of(textElem->text.begin(), textElem->text.end(), [](char32_t c) { return u_isWhitespace(c); }))
                            continue;
                    }
                }

                cmds.push_back(std::move(singleCommand));
                continue;
            }

            return nullptr;
        }

        *lexer = std::move(curLexer);
        return MakePtr<SStmt_Command>(std::move(cmds));
    }
    else // 싱글 커맨드, 엔터가 나오면 끝난다
    {
        auto singleCommand = ParseSingleCommand(false, &curLexer);

        if (!singleCommand)
            return nullptr;

        if (singleCommand->elements.empty())
            return nullptr;
        
        *lexer = std::move(curLexer);

        vector<shared_ptr<SExp_String>> strs;
        strs.push_back(std::move(singleCommand));

        return MakePtr<SStmt_Command>(std::move(strs));
    }
}

shared_ptr<SStmt_Directive> ParseDirectiveStmt(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    // ` <id> ( exp... );
    if (!Accept<BacktickToken>(&curLexer))
        return nullptr;

    auto oIdToken = Accept<IdentifierToken>(&curLexer);
    if (!oIdToken)
        return nullptr;

    if (!Accept<LParenToken>(&curLexer))
        return nullptr;

    vector<SExpPtr> args;    
    while (!Accept<RParenToken>(&curLexer))
    {
        if (!args.empty())
            if (!Accept<CommaToken>(&curLexer))
                return nullptr;

        auto arg = ParseExp(&curLexer);
        if (!arg)
            return nullptr;

        args.push_back(std::move(arg));
    }

    if (!Accept<SemiColonToken>(&curLexer))
        return nullptr;

    *lexer = std::move(curLexer);
    return MakePtr<SStmt_Directive>(std::move(oIdToken->text), std::move(args));
}

// if (...) 'x;' // 단일이냐
// if (...) '{ }' // 묶음이냐
SEmbeddableStmtPtr ParseEmbeddableStmt(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    // { 가 없다면, Embeddable.Single
    if (!Accept<LBraceToken>(&curLexer))
    {
        auto stmt = ParseStmt(&curLexer);
        if (!stmt)
            return nullptr;
        
        // block stmt는 제외되서 들어올 것이다
        assert(dynamic_cast<SStmt_Block*>(stmt.get()) == nullptr);

        *lexer = std::move(curLexer);
        return MakePtr<SEmbeddableStmt_Single>(std::move(stmt));
    }
    else // 있다면 Embeddable.Multiple
    {
        vector<SStmtPtr> stmts;

        // } 가 나올때까지
        while (!Accept<RBraceToken>(&curLexer))
        {
            auto stmt = ParseStmt(&curLexer);

            if (!stmt)
                return nullptr;

            stmts.push_back(std::move(stmt));
        }

        *lexer = std::move(curLexer);
        return MakePtr<SEmbeddableStmt_Block>(std::move(stmts));
    }
}

optional<vector<SStmtPtr>> ParseBody(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    if (!Accept<LBraceToken>(&curLexer))
        return nullopt;

    vector<SStmtPtr> stmts;
    while (!Accept<RBraceToken>(&curLexer))
    {
        auto stmt = ParseStmt(&curLexer);
        if (!stmt)
            return nullopt;

        stmts.push_back(std::move(stmt));
    }

    *lexer = std::move(curLexer);
    return stmts;
}

SStmtPtr ParseStmt(Lexer* lexer)
{
    if (auto stmt = ParseDirectiveStmt(lexer))
        return stmt;

    if (auto stmt = ParseBlankStmt(lexer))
        return stmt;

    if (auto stmt = ParseBlockStmt(lexer))
        return stmt;

    if (auto stmt = ParseContinueStmt(lexer))
        return stmt;

    if (auto stmt = ParseBreakStmt(lexer))
        return stmt;

    if (auto stmt = ParseReturnStmt(lexer))
        return stmt;

    if (auto stmt = ParseVarDeclStmt(lexer))
        return stmt;

    if (auto stmt = ParseIfStmt(lexer))
        return stmt;

    if (auto stmt = ParseForStmt(lexer))
        return stmt;

    if (auto stmt = ParseExpStmt(lexer))
        return stmt;

    if (auto stmt = ParseTaskStmt(lexer))
        return stmt;

    if (auto stmt = ParseAwaitStmt(lexer))
        return stmt;

    if (auto stmt = ParseAsyncStmt(lexer))
        return stmt;

    if (auto stmt = ParseForeachStmt(lexer))
        return stmt;

    if (auto stmt = ParseYieldStmt(lexer))
        return stmt;

    if (auto stmt = ParseCommandStmt(lexer))
        return stmt;

    return nullptr;
}



}
