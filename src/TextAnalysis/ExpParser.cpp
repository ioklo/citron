#include "pch.h"

#include <Infra/Ptr.h>

#include <Syntax/Syntax.h>
#include <Syntax/Tokens.h>

#include "Lexer.h"
#include "ExpParser.h"
#include "StmtParser.h"
#include "TypeExpParser.h"

#include "ParserMisc.h"


using namespace std;

namespace {

using namespace Citron;
struct BinaryOpInfo { Token token; SBinaryOpKind kind; };
struct UnaryOpInfo { Token token; SUnaryOpKind kind; };

// utility
template<SExpPtr (*ParseBaseExp)(Lexer* lexer), int N>
SExpPtr ParseLeftAssocBinaryOpExp(Lexer* lexer, BinaryOpInfo (&infos)[N])
{
    Lexer curLexer = *lexer;
    auto operand0 = ParseBaseExp(&curLexer);

    if (!operand0)
        return nullptr;

    SExpPtr curExp = std::move(operand0);

    while (true)
    {
        optional<SBinaryOpKind> oOpKind;

        if (auto oLexResult = curLexer.LexNormalMode(true))
        {
            for(auto& info : infos)
            {
                if (info.token == oLexResult->token)
                {
                    oOpKind = info.kind;
                    curLexer = oLexResult->lexer;
                    break;
                }
            }
        }

        if (!oOpKind)
        {
            // lexer 반영하고
            *lexer = std::move(curLexer);
            return curExp;
        }

        auto operand1 = ParseBaseExp(&curLexer);
        if (!operand1)
            return nullptr;
        
        // Fold
        curExp = MakePtr<SExp_BinaryOp>(*oOpKind, std::move(curExp), std::move(operand1));
    }
}

 // unary -가 int literal과 있을 때는 int literal로 합친다
SExpPtr HandleUnaryMinusWithIntLiteral(SUnaryOpKind kind, SExp* exp)
{
    if (kind == SUnaryOpKind::Minus)
        if (auto* intLiteralExp = dynamic_cast<SExp_IntLiteral*>(exp))
            return MakePtr<SExp_IntLiteral>(-intLiteralExp->value);

    return nullptr;
}

SArgumentPtr ParseArgument(Lexer* lexer)
{
    Lexer curLexer = *lexer;
    auto oOutAndParams = AcceptParseOutAndParams(&curLexer);
    if (!oOutAndParams)
        return nullptr;

    auto exp = ParseExp(&curLexer);

    if (!exp)
        return nullptr;

    *lexer = std::move(curLexer);
    return MakePtr<SArgument>(oOutAndParams->bOut, oOutAndParams->bParams, std::move(exp));
}

}

namespace Citron {

optional<vector<STypeExpPtr>> ParseTypeArgs(Lexer* lexer);


SArgumentsPtr ParseCallArgs(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    if (!Accept<LParenToken>(&curLexer))
        return nullptr;

    vector<SArgumentPtr> arguments;
    while (!Accept<RParenToken>(&curLexer))
    {
        if (!arguments.empty())
            if (!Accept<CommaToken>(&curLexer))
                return nullptr;

        auto arg = ParseArgument(&curLexer);
        if (!arg)
            return nullptr;

        arguments.push_back(std::move(arg));
    }

    *lexer = std::move(curLexer);
    return MakePtr<SArguments>(std::move(arguments)); // 이건 move가 되는데..
}

SExpPtr ParseExp(Lexer* lexer)
{
    return ParseAssignExp(lexer);
}

SExpPtr ParseAssignExp(Lexer* lexer)
{   
    Lexer curLexer = *lexer;

    // base
    auto exp0 = ParseEqualityExp(&curLexer);
    if (!exp0) 
        return nullptr;

    if (!Accept<EqualToken>(&curLexer))
    {
        *lexer = std::move(curLexer);
        return exp0;
    }

    auto exp1 = ParseAssignExp(&curLexer);
    if (!exp1)
        return nullptr;

    *lexer = std::move(curLexer);
    return MakePtr<SExp_BinaryOp>(SBinaryOpKind::Assign, std::move(exp0), std::move(exp1));
}

SExpPtr ParseEqualityExp(Lexer* lexer)
{
    static BinaryOpInfo equalityInfos[] = {
        { EqualEqualToken(), SBinaryOpKind::Equal },
        { ExclEqualToken(), SBinaryOpKind::NotEqual }
    };

    return ParseLeftAssocBinaryOpExp<&ParseTestAndTypeTestExp>(lexer, equalityInfos);
}

SExpPtr ParseTestAndTypeTestExp(Lexer* lexer)
{
    static BinaryOpInfo testInfos[] = {
        { GreaterThanEqualToken(), SBinaryOpKind::GreaterThanOrEqual },
        { LessThanEqualToken(), SBinaryOpKind::LessThanOrEqual },
        { LessThanToken(), SBinaryOpKind::LessThan },
        { GreaterThanToken(), SBinaryOpKind::GreaterThan }
    };

    Lexer curLexer = *lexer;

    // base
    auto operand0 = ParseAdditiveExp(&curLexer);

    if (!operand0)
        return nullptr;

    SExpPtr curExp = std::move(operand0);
    
    while (true)
    {
        bool bHandled = false;

        auto oLexResult = curLexer.LexNormalMode(true);
        if (!oLexResult) break;

        // search binary
        for(auto& info : testInfos)
        {
            if (info.token == oLexResult->token)
            {
                curLexer = std::move(oLexResult->lexer);

                // base
                auto operand1 = ParseAdditiveExp(&curLexer);

                if (!operand1)
                    return nullptr;

                // Fold
                curExp = MakePtr<SExp_BinaryOp>(info.kind, std::move(curExp), std::move(operand1));
                bHandled = true;
                break;
            }
        }

        if (bHandled) continue;

        if (holds_alternative<IsToken>(oLexResult->token))
        {
            curLexer = oLexResult->lexer;

            auto typeExp = ParseTypeExp(&curLexer);

            if (!typeExp)
                return nullptr;

            curExp = MakePtr<SExp_Is>(std::move(curExp), std::move(typeExp));
            continue;
        }

        if (holds_alternative<AsToken>(oLexResult->token))
        {
            curLexer = oLexResult->lexer;

            auto typeExp = ParseTypeExp(&curLexer);
            if (!typeExp) 
                return nullptr;

            curExp = MakePtr<SExp_As>(std::move(curExp), std::move(typeExp));
            continue;
        }

        break;
    }

    *lexer = std::move(curLexer);
    return curExp;
}


Citron::SExpPtr ParseAdditiveExp(Lexer* lexer)
{
    static BinaryOpInfo additiveInfos[] = {
        { PlusToken(), SBinaryOpKind::Add },
        { MinusToken(), SBinaryOpKind::Subtract },
    };

    return ParseLeftAssocBinaryOpExp<&ParseMultiplicativeExp>(lexer, additiveInfos);
}

Citron::SExpPtr ParseMultiplicativeExp(Lexer* lexer)
{   
    static BinaryOpInfo multiplicativeInfos[] = {
        { StarToken(), SBinaryOpKind::Multiply },
        { SlashToken(), SBinaryOpKind::Divide },
        { PercentToken(), SBinaryOpKind::Modulo },
    };

    return ParseLeftAssocBinaryOpExp<&ParseUnaryExp>(lexer, multiplicativeInfos);
}

// base는 PrimaryExp
Citron::SExpPtr ParseUnaryExp(Lexer* lexer)
{
    static UnaryOpInfo unaryInfos[] = {
        { MinusToken(), SUnaryOpKind::Minus},
        { ExclToken(), SUnaryOpKind::LogicalNot },
        { PlusPlusToken(), SUnaryOpKind::PrefixInc },
        { MinusMinusToken(), SUnaryOpKind::PrefixDec },
        { StarToken(), SUnaryOpKind::Deref }
    };

    Lexer curLexer = *lexer;
    optional<SUnaryOpKind> oOpKind;

    auto oLexResult = curLexer.LexNormalMode(true);
    if (oLexResult)
    {
        for(auto& info : unaryInfos)
        {
            if (info.token == oLexResult->token)
            {
                oOpKind = info.kind;
                curLexer = std::move(oLexResult->lexer);
                break;
            }
        }
    }

    if (oOpKind)
    {
        auto exp = ParseUnaryExp(&curLexer);
        if (!exp)
            return nullptr;

        // '-' '3'은 '-3'
        if (auto handledExp = HandleUnaryMinusWithIntLiteral(*oOpKind, exp.get()))
        {
            *lexer = std::move(curLexer);
            return handledExp;
        }

        *lexer = std::move(curLexer);
        return MakePtr<SExp_UnaryOp>(*oOpKind, std::move(exp));
    }
    else
    {
        // base
        return ParsePrimaryExp(lexer);
    }
}

// base ParseSingleExp
Citron::SExpPtr ParsePrimaryExp(Lexer* lexer)
{
    static UnaryOpInfo primaryInfos[] = {
        { PlusPlusToken(), SUnaryOpKind::PostfixInc },
        { MinusMinusToken(), SUnaryOpKind::PostfixDec },
    };

    Lexer curLexer = *lexer;

    auto operand = ParseSingleExp(&curLexer);

    if (!operand)
        return nullptr;

    auto curExp = std::move(operand);

    while (true)
    {
        // Unary일수도 있고, ()일수도 있다
        auto oLexResult = curLexer.LexNormalMode(true);
        if (!oLexResult) break;

        optional<UnaryOpInfo> primaryInfo;

        for (auto& info : primaryInfos)
        {
            if (info.token == oLexResult->token)
            {
                // TODO: postfix++이 두번 이상 나타나지 않도록 한다
                primaryInfo = info;
                break;
            }
        }

        if (primaryInfo)
        {
            curLexer = oLexResult->lexer;

            // Fold
            curExp = MakePtr<SExp_UnaryOp>(primaryInfo->kind, std::move(curExp));
            continue;
        }

        // [ ... ]
        if (Accept<LBracketToken>(&curLexer))
        {
            auto index = ParseExp(&curLexer);
            if (!index)
                return nullptr;

            if (!Accept<RBraceToken>(&curLexer))
                return nullptr;

            curExp = MakePtr<SExp_Indexer>(std::move(curExp), std::move(index));
            continue;
        }

        // . id < >
        if (Accept<DotToken>(&curLexer))
        {
            auto oIdToken = Accept<IdentifierToken>(&curLexer);

            if (!oIdToken)
                return nullptr;

            // <
            auto oTypeArgs = ParseTypeArgs(&curLexer);

            if (oTypeArgs)
                curExp = MakePtr<SExp_Member>(std::move(curExp), std::move(oIdToken->text), std::move(*oTypeArgs));
            else
                curExp = MakePtr<SExp_Member>(std::move(curExp), std::move(oIdToken->text), std::vector<STypeExpPtr>());

            continue;
        }

        // exp -> id < > => (*exp).id
        if (Accept<MinusGreaterThanToken>(&curLexer, oLexResult))
        {
            auto oIdToken = Accept<IdentifierToken>(&curLexer);

            if (!oIdToken)
                return nullptr;

            // <
            auto oTypeArgs = ParseTypeArgs(&curLexer);
            if (oTypeArgs)
            {   
                curExp = MakePtr<SExp_IndirectMember>(std::move(curExp), std::move(oIdToken->text), std::move(*oTypeArgs));
            }
            else
            {   
                curExp = MakePtr<SExp_IndirectMember>(std::move(curExp), std::move(oIdToken->text), vector<STypeExpPtr>());
            }

            continue;
        }

        // (..., ... )             
        auto arguments = ParseCallArgs(&curLexer);
        if (arguments)
        {
            curExp = MakePtr<SExp_Call>(std::move(curExp), std::move(arguments));
            continue;
        }

        break;
    }

    *lexer = curLexer;
    return curExp;
}

SExpPtr ParseSingleExp(Lexer* lexer)
{
    if (auto exp = ParseBoxExp(lexer))
        return exp;
        
    if (auto exp = ParseNewExp(lexer))
        return exp;
        
    if (auto exp = ParseLambdaExp(lexer))
        return exp;
        
    if (auto exp = ParseParenExp(lexer))
        return exp;
        
    if (auto exp = ParseNullLiteralExp(lexer))
        return exp;
        
    if (auto exp = ParseBoolLiteralExp(lexer))
        return exp;
        
    if (auto exp = ParseIntLiteralExp(lexer))
        return exp;
        
    if (auto exp = ParseStringExp(lexer))
        return exp;
        
    if (auto exp = ParseListExp(lexer))
        return exp;
        
    if (auto exp = ParseIdentifierExp(lexer))
        return exp;
        
    return nullptr;
}


shared_ptr<SExp_Box> ParseBoxExp(Lexer* lexer)
{
    // <BOX> <EXP>
    Lexer curLexer = *lexer;

    if (!Accept<BoxToken>(&curLexer))
        return nullptr;

    auto innerExp = ParseExp(&curLexer);
    if (!innerExp)
        return nullptr;

    *lexer = std::move(curLexer);
    return MakePtr<SExp_Box>(std::move(innerExp));
}

shared_ptr<SExp_New> ParseNewExp(Lexer* lexer)
{
    // <NEW> <TYPEEXP> <LPAREN> CallArgs <RPAREN>
    Lexer curLexer = *lexer;
    
    if (!Accept<NewToken>(&curLexer))
        return nullptr;

    auto type = ParseTypeExp(&curLexer);
    if (!type)
        return nullptr;

    auto args = ParseCallArgs(&curLexer);
    
    if (!args)
        return nullptr;

    *lexer = std::move(curLexer);
    return MakePtr<SExp_New>(std::move(type), std::move(args));
}

// LambdaExpression, Right Assoc
shared_ptr<SExp_Lambda> ParseLambdaExp(Lexer* lexer)
{
    Lexer curLexer = *lexer;
    vector<SLambdaExpParam> params;

    // (), (a, b)            
    // (int a)
    // (out int* a, int b) => ...
    // a
    // out a
    // params a
    if (Accept<LParenToken>(&curLexer))
    {
        while (!Accept<RParenToken>(&curLexer))
        {
            if (!params.empty())
                if (!Accept<CommaToken>(&curLexer))
                    return nullptr;

            auto oOutAndParams = AcceptParseOutAndParams(&curLexer);
            if (!oOutAndParams)
                return nullptr;

            // id id or id
            auto oFirstIdToken = Accept<IdentifierToken>(&curLexer);
            if (!oFirstIdToken)
                return nullptr;

            auto oSecondIdToken = Accept<IdentifierToken>(&curLexer);
            if (!oSecondIdToken)
                params.emplace_back(nullptr, std::move(oFirstIdToken->text), oOutAndParams->bOut, oOutAndParams->bParams);
            else
                params.emplace_back(MakePtr<STypeExp_Id>(std::move(oFirstIdToken->text), vector<STypeExpPtr>{}), std::move(oSecondIdToken->text), oOutAndParams->bOut, oOutAndParams->bParams);
        }
    }
    else
    {
        // out과 params는 동시에 쓸 수 없다
        auto oOutAndParams = AcceptParseOutAndParams(&curLexer);
        if (!oOutAndParams)
            return nullptr;
        
        auto oIdToken = Accept<IdentifierToken>(&curLexer);
        if (!oIdToken)
            return nullptr;

        params.emplace_back(nullptr, std::move(oIdToken->text), oOutAndParams->bOut, oOutAndParams->bParams);
    }

    // =>
    if (!Accept<EqualGreaterThanToken>(&curLexer))
        return nullptr;
    

    // exp => return exp;
    // { ... }
    SLambdaExpBodyPtr body;
    if (Peek<LBraceToken>(curLexer))
    {
        // Body 파싱을 그대로 쓴다
        auto oStmtBody = ParseBody(&curLexer);
        if (!oStmtBody)
            return nullptr;

        body = MakePtr<SLambdaExpBody_Stmts>(std::move(*oStmtBody));
    }
    else
    {
        // exp
        auto exp = ParseExp(&curLexer);
        if (!exp)
            return nullptr;

        body = MakePtr<SLambdaExpBody_Exp>(std::move(exp));
    }

    *lexer = std::move(curLexer);
    return MakePtr<SExp_Lambda>(std::move(params), std::move(body));
}

SExpPtr ParseParenExp(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    if (!Accept<LParenToken>(&curLexer))
        return nullptr;
    
    auto oxp = ParseExp(&curLexer);
    if (!oxp)
        return nullptr;
    
    if (!Accept<RParenToken>(&curLexer))
        return nullptr;

    *lexer = std::move(curLexer);
    return oxp;
}

shared_ptr<SExp_NullLiteral> ParseNullLiteralExp(Lexer* lexer)
{
    if (!Accept<NullToken>(lexer))
        return nullptr;

    return MakePtr<SExp_NullLiteral>();
}

shared_ptr<SExp_BoolLiteral> ParseBoolLiteralExp(Lexer* lexer)
{
    auto oBoolToken = Accept<BoolToken>(lexer);

    if (!oBoolToken)
        return nullptr;

    return MakePtr<SExp_BoolLiteral>(oBoolToken->value);
}

shared_ptr<SExp_IntLiteral> ParseIntLiteralExp(Lexer* lexer)
{
    auto oIntToken = Accept<IntToken>(lexer);

    if (!oIntToken)
        return nullptr;

    return MakePtr<SExp_IntLiteral>(oIntToken->value);
}

// 스트링 파싱
shared_ptr<SExp_String> ParseStringExp(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    if (!Accept<DoubleQuoteToken>(&curLexer))
        return nullptr;

    vector<SStringExpElementPtr> elems;
    
    while (true)
    {
        auto oLexResult = curLexer.LexStringMode();
        if (Accept<DoubleQuoteToken>(&curLexer, oLexResult))
            break;

        if (auto oTextToken = Accept<TextToken>(&curLexer, oLexResult))
        {
            elems.push_back(MakePtr<SStringExpElement_Text>(std::move(oTextToken->text)));
            continue;
        }
        
        if (auto oIdToken = Accept<IdentifierToken>(&curLexer, oLexResult))
        {
            elems.push_back(MakePtr<SStringExpElement_Exp>(MakePtr<SExp_Identifier>(std::move(oIdToken->text), std::vector<STypeExpPtr>{})));
            continue;
        }

        // ${
        if (Accept<DollarLBraceToken>(&curLexer, oLexResult))
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

        return nullptr;
    }

    *lexer = std::move(curLexer);
    return MakePtr<SExp_String>(std::move(elems));
}

shared_ptr<SExp_List> ParseListExp(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    if (!Accept<LBracketToken>(&curLexer))
        return nullptr;

    vector<SExpPtr> elems;    
    
    while (!Accept<RBracketToken>(&curLexer))
    {
        if (!elems.empty())
            if (!Accept<CommaToken>(&curLexer))
                return nullptr;

        auto elem = ParseExp(&curLexer);
        if (!elem)
            return nullptr;

        elems.push_back(std::move(elem));
    }

    *lexer = curLexer;
    return MakePtr<SExp_List>(std::move(elems));
}

// lexer를 실패했을때 되돌리는 것은 Parser책임
shared_ptr<SExp_Identifier> ParseIdentifierExp(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    auto oIdToken = Accept<IdentifierToken>(&curLexer);
    if (!oIdToken) return nullptr;

    // 실패해도 괜찮다
    auto oTypeArgs = ParseTypeArgs(&curLexer);

    if (oTypeArgs)
    {
        *lexer = std::move(curLexer);
        return MakePtr<SExp_Identifier>(std::move(oIdToken->text), std::move(*oTypeArgs));
    }
    else
    {
        *lexer = std::move(curLexer);
        return MakePtr<SExp_Identifier>(std::move(oIdToken->text), std::vector<STypeExpPtr>{});
    }
}

}