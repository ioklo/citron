#include "StmtParser.h"

#include <optional>
#include <algorithm>
#include <cassert>

#include <unicode/uchar.h>

#include "Infra/Ptr.h"
#include "Infra/Exceptions.h"
#include "Syntax/Syntax.h"
#include "Syntax/Tokens.h"
#include "Lexer.h"
#include "ExpParser.h"
#include "TypeExpParser.h"
#include "ParserMisc.h"

using namespace std;

namespace Citron {

SEmbeddableStmt* ParseEmbeddableStmt(Lexer* lexer, SFactory& factory);

// 리턴은 SStmt_If
SStmt_If* ParseIfStmt(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    // if (exp) stmt => If(exp, stmt, null)
    // if (exp) stmt0 else stmt1 => If(exp, stmt0, stmt1)
    // if (exp0) if (exp1) stmt1 else stmt2 => If(exp0, If(exp1, stmt1, stmt2))

    if (!Accept<IfToken>(&curLexer))
        return nullptr;

    if (!Accept<LParenToken>(&curLexer))
        return nullptr;

    // 아니라면
    auto* cond = ParseExp(&curLexer, factory);
    if (!cond)
        return nullptr;

    if (!Accept<RParenToken>(&curLexer))
        return nullptr;

    // right assoc, conflict는 별다른 처리를 하지 않고 지나가면 될 것 같다
    auto* body = ParseEmbeddableStmt(&curLexer, factory);
    if (!body)
        return nullptr;

    SEmbeddableStmt* elseBody = nullptr;
    
    if (Accept<ElseToken>(&curLexer))
    {
        elseBody = ParseEmbeddableStmt(&curLexer, factory);
        if (!elseBody)
            return nullptr;
    }

    *lexer = move(curLexer);
    return factory.MakeSStmt_If(cond, body, elseBody);
}

optional<SVarDecl> ParseVarDecl(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    auto* varType = ParseVarDeclTypeExp(&curLexer, factory);
    if (!varType)
        return nullopt;

    vector<SVarDeclElement> elems;
        
    do
    {
        auto o_varIdToken = Accept<IdentifierToken>(&curLexer);
        if (!o_varIdToken)
            return nullopt;

        if (!Accept<EqualToken>(&curLexer)) return nullopt;
        
        if (Accept<MoveToken>(&curLexer))
        {
            SExp* initExp = ParseExp(&curLexer, factory);
            if (!initExp) return nullopt;

            elems.push_back(SVarDeclElement{move(o_varIdToken->text), SVarDeclElementInit_Move{initExp}});
        }
        else if (auto o_initLexResult = Peek<IdentifierToken>(curLexer); o_initLexResult && o_initLexResult->token.text == "uninit")
        {
            Accept(&curLexer, *o_initLexResult);

            elems.push_back(SVarDeclElement{move(o_varIdToken->text), SVarDeclElementInit_Uninit{}});
        }
        else
        {
            // TODO: ;나 ,가 나올때까지라는걸 명시해주면 좋겠다
            SExp* initExp = ParseExp(&curLexer, factory);
            
            // TODO: Error_VarDecl_LocalVarDeclNeedInitializer
            if (!initExp) return nullopt;

            elems.push_back(SVarDeclElement{move(o_varIdToken->text), SVarDeclElementInit_Exp{initExp}});
        }

    } while (Accept<CommaToken>(&curLexer)); // ,가 나오면 계속한다

    *lexer = move(curLexer);
    return SVarDecl(varType, move(elems));
}

// int x = 0;
SStmt_VarDecl* ParseVarDeclStmt(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    auto o_varDecl = ParseVarDecl(&curLexer, factory);
    if (!o_varDecl)
        return nullptr;

    if (!Accept<SemiColonToken>(&curLexer))
        return nullptr;

    *lexer = move(curLexer);
    return factory.MakeSStmt_VarDecl(move(*o_varDecl));
}

SForStmtInitializer* ParseForStmtInitializer(Lexer* lexer, SFactory& factory)
{
    if (auto o_varDecl = ParseVarDecl(lexer, factory))
        return factory.MakeSForStmtInitializer_VarDecl(move(*o_varDecl));

    if (auto* exp = ParseExp(lexer, factory))
        return factory.MakeSForStmtInitializer_Exp(exp);

    return nullptr;
}

// :label을 파싱한다
optional<string> ParseLabel(Lexer* lexer)
{
    Lexer curLexer{*lexer};

    if (!Accept<ColonToken>(&curLexer)) return nullopt;
    auto o_labelToken = Accept<IdentifierToken>(&curLexer);
    if (!o_labelToken) return nullopt;

    *lexer = move(curLexer);
    return o_labelToken->text;
}

SStmt_For* ParseForStmt(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    // optional label
    auto o_label = ParseLabel(&curLexer);

    if (!Accept<ForToken>(&curLexer))
        return nullptr;

    if (!Accept<LParenToken>(&curLexer))
        return nullptr;

    // TODO: 이 Initializer의 끝은 ';' 이다
    auto* initializer = ParseForStmtInitializer(&curLexer, factory);

    if (!Accept<SemiColonToken>(&curLexer))
        return nullptr;

    // TODO: 이 CondExp의 끝은 ';' 이다
    auto* cond = ParseExp(&curLexer, factory);

    if (!Accept<SemiColonToken>(&curLexer))
        return nullptr;

    // TODO: 이 CondExp의 끝은 ')' 이다            
    auto* cont = ParseExp(&curLexer, factory);

    if (!Accept<RParenToken>(&curLexer))
        return nullptr;

    auto* bodyStmt = ParseEmbeddableStmt(&curLexer, factory);
    if (!bodyStmt)
        return nullptr;

    *lexer = move(curLexer);
    return factory.MakeSStmt_For(o_label, initializer, cond, cont, bodyStmt);
}

SStmt_While* ParseWhileStmt(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    // optional label
    auto o_label = ParseLabel(&curLexer);

    if (!Accept<WhileToken>(&curLexer))
        return nullptr;

    if (!Accept<LParenToken>(&curLexer))
        return nullptr;

    // TODO: 이 CondExp의 끝은 ';' 이다
    auto* cond = ParseExp(&curLexer, factory);

    if (!Accept<RParenToken>(&curLexer))
        return nullptr;

    auto* bodyStmt = ParseEmbeddableStmt(&curLexer, factory);
    if (!bodyStmt)
        return nullptr;

    *lexer = move(curLexer);
    return factory.MakeSStmt_While(o_label, cond, bodyStmt);
}

// switch(exp) { case pattern: single-stmt; case pattern: { stmts; } default: {} }
SStmt_Switch* ParseSwitchStmt(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    auto o_label = ParseLabel(&curLexer);

    if (!Accept<SwitchToken>(&curLexer))
        return nullptr;

    if (!Accept<LParenToken>(&curLexer))
        return nullptr;

    auto* target = ParseExp(&curLexer, factory);

    // TODO: [60] switch 구현
    throw NotImplementedException{};
}

SStmt_Continue* ParseContinueStmt(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    if (!Accept<ContinueToken>(&curLexer))
        return nullptr;

    auto o_labelToken = ParseLabel(&curLexer);

    if (!Accept<SemiColonToken>(&curLexer))
        return nullptr;

    *lexer = move(curLexer);
    return factory.MakeSStmt_Continue(o_labelToken);
}

SStmt_Break* ParseBreakStmt(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    if (!Accept<BreakToken>(&curLexer))
        return nullptr;

    auto o_labelToken = ParseLabel(&curLexer);

    if (!Accept<SemiColonToken>(&curLexer))
        return nullptr;

    *lexer = move(curLexer);
    return factory.MakeSStmt_Break(o_labelToken);
}

SStmt_Leave* ParseLeaveStmt(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;
    if (!Accept<LeaveToken>(&curLexer)) return nullptr;

    // optional
    auto o_labelToken = ParseLabel(&curLexer);

    auto* o_exp = ParseExp(&curLexer, factory);
    if (!o_exp) return nullptr;

    if (!Accept<SemiColonToken>(&curLexer)) return nullptr;
    *lexer = move(curLexer);

    return factory.Make<SStmt_Leave>(o_labelToken, o_exp);
}

SStmt_Return* ParseReturnStmt(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    if (!Accept<ReturnToken>(&curLexer))
        return nullptr;

    SExp* returnValue = nullptr;

    if (auto* returnExp = ParseExp(&curLexer, factory))
        returnValue = returnExp;

    if (!Accept<SemiColonToken>(&curLexer))
        return nullptr;

    *lexer = move(curLexer);
    return factory.MakeSStmt_Return(returnValue);
}

SStmt_Block* ParseBlockStmt(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    if (!Accept<LBraceToken>(&curLexer))
        return nullptr;

    vector<SStmt*> stmts;
    
    while (!Accept<RBraceToken>(&curLexer))
    {
        auto* stmt = ParseStmt(&curLexer, factory);
        if (!stmt) return nullptr;

        stmts.push_back(stmt);
    }

    *lexer = move(curLexer);
    return factory.MakeSStmt_Block(move(stmts));
}

SStmt_Blank* ParseBlankStmt(Lexer* lexer, SFactory& factory)
{
    if (!Accept<SemiColonToken>(lexer))
        return nullptr;

    return factory.MakeSStmt_Blank();
}

// TODO: Assign, Call만 가능하게 해야 한다
SStmt_Exp* ParseExpStmt(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    auto* exp = ParseExp(&curLexer, factory);
    if (!exp)
        return nullptr;
    
    if (!Accept<SemiColonToken>(&curLexer))
        return nullptr;

    *lexer = move(curLexer);
    return factory.MakeSStmt_Exp(exp);
}

SStmt_Task* ParseTaskStmt(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    if (!Accept<TaskToken>(&curLexer))
        return nullptr;

    auto o_body = ParseBody(&curLexer, factory);
    
    if (!o_body)
        return nullptr;

    *lexer = move(curLexer);
    return factory.MakeSStmt_Task(move(*o_body));
}

SStmt_Await* ParseAwaitStmt(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    if (!Accept<AwaitToken>(&curLexer))
        return nullptr;
    
    auto o_body = ParseBody(&curLexer, factory);
    if (!o_body)
        return nullptr;

    *lexer = move(curLexer);
    return factory.MakeSStmt_Await(move(*o_body));
}

SStmt_Async* ParseAsyncStmt(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    if (!Accept<AsyncToken>(&curLexer))
        return nullptr;

    auto o_body = ParseBody(&curLexer, factory);
    if (!o_body)
        return nullptr;

    *lexer = move(curLexer);
    return factory.MakeSStmt_Async(move(*o_body));
}

SStmt_Yield* ParseYieldStmt(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    if (!Accept<YieldToken>(&curLexer))
        return nullptr;

    auto* yieldValue = ParseExp(&curLexer, factory);

    if (!yieldValue)
        return nullptr;

    if (!Accept<SemiColonToken>(&curLexer))
        return nullptr;

    *lexer = move(curLexer);
    return factory.MakeSStmt_Yield(yieldValue);
}

SExp_String* ParseSingleCommand(bool bStopRBrace, Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    vector<SStringExpElement*> elems;

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
            auto* exp = ParseExp(&curLexer, factory);
            if (!exp)
                return nullptr;

            if (!Accept<RBraceToken>(&curLexer))
                return nullptr;

            elems.push_back(factory.MakeSStringExpElement_Exp(exp));
            continue;
        }

        // aa$b => $b 이야기
        if (auto o_idToken = Accept<IdentifierToken>(&curLexer, curLexer.LexCommandMode()))
        {
            elems.push_back(factory.MakeSStringExpElement_Exp(factory.MakeSExp_Identifier(move(o_idToken->text), std::vector<STypeExp*>{})));
            continue;
        }

        
        if (auto o_textToken = Accept<TextToken>(&curLexer, curLexer.LexCommandMode()))
        {
            elems.push_back(factory.MakeSStringExpElement_Text(move(o_textToken->text)));
            continue;
        }

        return nullptr;
    }

    *lexer = move(curLexer);
    return factory.MakeSExp_String(move(elems));
}

SStmt_Foreach* ParseForeachStmt(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    // foreach
    if (!Accept<ForeachToken>(&curLexer))
        return nullptr;

    // (
    if (!Accept<LParenToken>(&curLexer))
        return nullptr;
    

    // var 
    auto* typeExp = ParseTypeExp(&curLexer, factory);
    if (!typeExp)
        return nullptr;

    // x
    auto o_varNameToken = Accept<IdentifierToken>(&curLexer);
    if (!o_varNameToken)
        return nullptr;

    // in
    if (!Accept<InToken>(&curLexer))
        return nullptr;

    // obj
    auto* obj = ParseExp(&curLexer, factory);
    if (!obj)
        return nullptr;

    // )
    if (!Accept<RParenToken>(&curLexer))
        return nullptr;

    // stmt
    auto* stmt = ParseEmbeddableStmt(&curLexer, factory);
    if (!stmt)
        return nullptr;

    *lexer = move(curLexer);
    return factory.MakeSStmt_Foreach(typeExp, move(o_varNameToken->text), obj, stmt);
}

// 
SStmt_Command* ParseCommandStmt(Lexer* lexer, SFactory& factory)
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
        vector<SExp_String*> cmds;
        while (true)
        {
            if (Accept<RBraceToken>(&curLexer, curLexer.LexCommandMode()))
                break;

            auto* singleCommand = ParseSingleCommand(true, &curLexer, factory);

            if (singleCommand)
            {
                // singleCommand Skip 조건
                size_t elemCount = singleCommand->elements.size();

                if (elemCount == 0)
                    continue;

                if (elemCount == 1)
                {
                    auto* elem = singleCommand->elements[0];
                    if (SStringExpElement_Text* textElem = dynamic_cast<SStringExpElement_Text*>(elem))
                    {
                        if (all_of(textElem->text.begin(), textElem->text.end(), [](char32_t c) { return u_isWhitespace(c); }))
                            continue;
                    }
                }

                cmds.push_back(singleCommand);
                continue;
            }

            return nullptr;
        }

        *lexer = move(curLexer);
        return factory.MakeSStmt_Command(move(cmds));
    }
    else // 싱글 커맨드, 엔터가 나오면 끝난다
    {
        auto* singleCommand = ParseSingleCommand(false, &curLexer, factory);

        if (!singleCommand)
            return nullptr;

        if (singleCommand->elements.empty())
            return nullptr;
        
        *lexer = move(curLexer);

        vector<SExp_String*> strs;
        strs.push_back(singleCommand);

        return factory.MakeSStmt_Command(move(strs));
    }
}

SStmt_Directive* ParseDirectiveStmt(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    // ` <id> ( exp... );
    if (!Accept<BacktickToken>(&curLexer))
        return nullptr;

    auto o_idToken = Accept<IdentifierToken>(&curLexer);
    if (!o_idToken)
        return nullptr;

    if (!Accept<LParenToken>(&curLexer))
        return nullptr;

    vector<SExp*> args;    
    while (!Accept<RParenToken>(&curLexer))
    {
        if (!args.empty())
            if (!Accept<CommaToken>(&curLexer))
                return nullptr;

        auto* arg = ParseExp(&curLexer, factory);
        if (!arg)
            return nullptr;

        args.push_back(arg);
    }

    if (!Accept<SemiColonToken>(&curLexer))
        return nullptr;

    *lexer = move(curLexer);
    return factory.MakeSStmt_Directive(move(o_idToken->text), move(args));
}

// if (...) 'x;' // 단일이냐
// if (...) '{ }' // 묶음이냐
SEmbeddableStmt* ParseEmbeddableStmt(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    // { 가 없다면, Embeddable.Single
    if (!Accept<LBraceToken>(&curLexer))
    {
        auto* stmt = ParseStmt(&curLexer, factory);
        if (!stmt)
            return nullptr;
        
        // block stmt는 제외되서 들어올 것이다
        assert(dynamic_cast<SStmt_Block*>(stmt) == nullptr);

        *lexer = move(curLexer);
        return factory.MakeSEmbeddableStmt_Single(stmt);
    }
    else // 있다면 Embeddable.Multiple
    {
        vector<SStmt*> stmts;

        // } 가 나올때까지
        while (!Accept<RBraceToken>(&curLexer))
        {
            auto* stmt = ParseStmt(&curLexer, factory);

            if (!stmt)
                return nullptr;

            stmts.push_back(stmt);
        }

        *lexer = move(curLexer);
        return factory.MakeSEmbeddableStmt_Block(move(stmts));
    }
}

optional<vector<SStmt*>> ParseBody(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    if (!Accept<LBraceToken>(&curLexer))
        return nullopt;

    vector<SStmt*> stmts;
    while (!Accept<RBraceToken>(&curLexer))
    {
        auto* stmt = ParseStmt(&curLexer, factory);
        if (!stmt)
            return nullopt;

        stmts.push_back(stmt);
    }

    *lexer = move(curLexer);
    return stmts;
}

SStmt* ParseStmt(Lexer* lexer, SFactory& factory)
{
    if (auto* stmt = ParseDirectiveStmt(lexer, factory))
        return stmt;

    if (auto* stmt = ParseBlankStmt(lexer, factory))
        return stmt;

    if (auto* stmt = ParseBlockStmt(lexer, factory))
        return stmt;

    if (auto* stmt = ParseContinueStmt(lexer, factory))
        return stmt;

    if (auto* stmt = ParseBreakStmt(lexer, factory))
        return stmt;

    if (auto* stmt = ParseLeaveStmt(lexer, factory))
        return stmt;

    if (auto* stmt = ParseReturnStmt(lexer, factory))
        return stmt;

    if (auto* stmt = ParseVarDeclStmt(lexer, factory))
        return stmt;

    if (auto* stmt = ParseIfStmt(lexer, factory))
        return stmt;

    if (auto* stmt = ParseForStmt(lexer, factory))
        return stmt;

    if (auto* stmt = ParseExpStmt(lexer, factory))
        return stmt;

    if (auto* stmt = ParseTaskStmt(lexer, factory))
        return stmt;

    if (auto* stmt = ParseAwaitStmt(lexer, factory))
        return stmt;

    if (auto* stmt = ParseAsyncStmt(lexer, factory))
        return stmt;

    if (auto* stmt = ParseForeachStmt(lexer, factory))
        return stmt;

    if (auto* stmt = ParseYieldStmt(lexer, factory))
        return stmt;

    if (auto* stmt = ParseCommandStmt(lexer, factory))
        return stmt;

    return nullptr;
}



}
