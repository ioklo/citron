#include "ExpParser.h"

#include "Infra/Ptr.h"
#include "Syntax/Syntax.h"
#include "Syntax/Tokens.h"

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
template<SExp* (*ParseBaseExp)(Lexer* lexer, SFactory& factory), int N>
SExp* ParseLeftAssocBinaryOpExp(Lexer* lexer, BinaryOpInfo (&infos)[N], SFactory& factory)
{
    Lexer curLexer = *lexer;
    auto* operand0 = ParseBaseExp(&curLexer, factory);

    if (!operand0)
        return nullptr;

    SExp* curExp = operand0;

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
            *lexer = move(curLexer);
            return curExp;
        }

        auto* operand1 = ParseBaseExp(&curLexer, factory);
        if (!operand1)
            return nullptr;
        
        // Fold
        curExp = factory.MakeSExp_BinaryOp(*oOpKind, curExp, operand1);
    }
}

 // unary -가 int literal과 있을 때는 int literal로 합친다
SExp* HandleUnaryMinusWithIntLiteral(SUnaryOpKind kind, SExp* exp, SFactory& factory)
{
    if (kind == SUnaryOpKind::Minus)
        if (auto* intLiteralExp = dynamic_cast<SExp_IntLiteral*>(exp))
            return factory.MakeSExp_IntLiteral(-intLiteralExp->value);

    return nullptr;
}

SArgument* ParseArgument(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;
    auto oOutAndParams = AcceptParseOutAndParams(&curLexer);
    if (!oOutAndParams)
        return nullptr;

    auto* exp = ParseExp(&curLexer, factory);

    if (!exp)
        return nullptr;

    *lexer = move(curLexer);
    return factory.MakeSArgument(oOutAndParams->bOut, oOutAndParams->bParams, exp);
}

}

namespace Citron {

optional<vector<STypeExp*>> ParseTypeArgs(Lexer* lexer, SFactory& factory);

SArguments* ParseCallArgs(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    if (!Accept<LParenToken>(&curLexer))
        return nullptr;

    vector<SArgument*> arguments;
    while (!Accept<RParenToken>(&curLexer))
    {
        if (!arguments.empty())
            if (!Accept<CommaToken>(&curLexer))
                return nullptr;

        auto* arg = ParseArgument(&curLexer, factory);
        if (!arg)
            return nullptr;

        arguments.push_back(arg);
    }

    *lexer = move(curLexer);
    return factory.MakeSArguments(move(arguments));
}

SExp* ParseExp(Lexer* lexer, SFactory& factory)
{
    return ParseAssignExp(lexer, factory);
}

SExp* ParseAssignExp(Lexer* lexer, SFactory& factory)
{   
    Lexer curLexer = *lexer;

    // base
    auto* exp0 = ParseEqualityExp(&curLexer, factory);
    if (!exp0) 
        return nullptr;

    if (!Accept<EqualToken>(&curLexer))
    {
        *lexer = move(curLexer);
        return exp0;
    }

    auto* exp1 = ParseAssignExp(&curLexer, factory);
    if (!exp1)
        return nullptr;

    *lexer = move(curLexer);
    return factory.MakeSExp_BinaryOp(SBinaryOpKind::Assign, exp0, exp1);
}

SExp* ParseEqualityExp(Lexer* lexer, SFactory& factory)
{
    static BinaryOpInfo equalityInfos[] = {
        { EqualEqualToken(), SBinaryOpKind::Equal },
        { ExclEqualToken(), SBinaryOpKind::NotEqual }
    };

    return ParseLeftAssocBinaryOpExp<&ParseTestAndTypeTestExp>(lexer, equalityInfos, factory);
}

SExp* ParseTestAndTypeTestExp(Lexer* lexer, SFactory& factory)
{
    static BinaryOpInfo testInfos[] = {
        { GreaterThanEqualToken(), SBinaryOpKind::GreaterThanOrEqual },
        { LessThanEqualToken(), SBinaryOpKind::LessThanOrEqual },
        { LessThanToken(), SBinaryOpKind::LessThan },
        { GreaterThanToken(), SBinaryOpKind::GreaterThan }
    };

    Lexer curLexer = *lexer;

    // base
    auto* operand0 = ParseAdditiveExp(&curLexer, factory);

    if (!operand0)
        return nullptr;

    SExp* curExp = operand0;
    
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
                curLexer = move(oLexResult->lexer);

                // base
                auto* operand1 = ParseAdditiveExp(&curLexer, factory);

                if (!operand1)
                    return nullptr;

                // Fold
                curExp = factory.MakeSExp_BinaryOp(info.kind, curExp, operand1);
                bHandled = true;
                break;
            }
        }

        if (bHandled) continue;

        if (holds_alternative<IsToken>(oLexResult->token))
        {
            curLexer = oLexResult->lexer;

            auto* typeExp = ParseTypeExp(&curLexer, factory);

            if (!typeExp)
                return nullptr;

            curExp = factory.MakeSExp_Is(curExp, typeExp);
            continue;
        }

        if (holds_alternative<AsToken>(oLexResult->token))
        {
            curLexer = oLexResult->lexer;

            auto* typeExp = ParseTypeExp(&curLexer, factory);
            if (!typeExp) 
                return nullptr;

            curExp = factory.MakeSExp_As(curExp, typeExp);
            continue;
        }

        break;
    }

    *lexer = move(curLexer);
    return curExp;
}


Citron::SExp* ParseAdditiveExp(Lexer* lexer, SFactory& factory)
{
    static BinaryOpInfo additiveInfos[] = {
        { PlusToken(), SBinaryOpKind::Add },
        { MinusToken(), SBinaryOpKind::Subtract },
    };

    return ParseLeftAssocBinaryOpExp<&ParseMultiplicativeExp>(lexer, additiveInfos, factory);
}

Citron::SExp* ParseMultiplicativeExp(Lexer* lexer, SFactory& factory)
{   
    static BinaryOpInfo multiplicativeInfos[] = {
        { StarToken(), SBinaryOpKind::Multiply },
        { SlashToken(), SBinaryOpKind::Divide },
        { PercentToken(), SBinaryOpKind::Modulo },
    };

    return ParseLeftAssocBinaryOpExp<&ParseUnaryExp>(lexer, multiplicativeInfos, factory);
}

// base는 PrimaryExp
Citron::SExp* ParseUnaryExp(Lexer* lexer, SFactory& factory)
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
                curLexer = move(oLexResult->lexer);
                break;
            }
        }
    }

    if (oOpKind)
    {
        auto* exp = ParseUnaryExp(&curLexer, factory);
        if (!exp)
            return nullptr;

        // '-' '3'은 '-3'
        if (auto* handledExp = HandleUnaryMinusWithIntLiteral(*oOpKind, exp, factory))
        {
            *lexer = move(curLexer);
            return handledExp;
        }

        *lexer = move(curLexer);
        return factory.MakeSExp_UnaryOp(*oOpKind, exp);
    }
    else
    {
        // base
        return ParsePrimaryExp(lexer, factory);
    }
}

// base ParseSingleExp
Citron::SExp* ParsePrimaryExp(Lexer* lexer, SFactory& factory)
{
    static UnaryOpInfo primaryInfos[] = {
        { PlusPlusToken(), SUnaryOpKind::PostfixInc },
        { MinusMinusToken(), SUnaryOpKind::PostfixDec },
    };

    Lexer curLexer = *lexer;

    auto* operand = ParseSingleExp(&curLexer, factory);

    if (!operand)
        return nullptr;

    auto* curExp = operand;

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
            curExp = factory.MakeSExp_UnaryOp(primaryInfo->kind, curExp);
            continue;
        }

        // [ ... ]
        if (Accept<LBracketToken>(&curLexer))
        {
            auto* index = ParseExp(&curLexer, factory);
            if (!index)
                return nullptr;

            if (!Accept<RBraceToken>(&curLexer))
                return nullptr;

            curExp = factory.MakeSExp_Indexer(curExp, index);
            continue;
        }

        // . id < >
        if (Accept<DotToken>(&curLexer))
        {
            auto oIdToken = Accept<IdentifierToken>(&curLexer);

            if (!oIdToken)
                return nullptr;

            // <
            auto oTypeArgs = ParseTypeArgs(&curLexer, factory);

            if (oTypeArgs)
                curExp = factory.MakeSExp_Member(curExp, move(oIdToken->text), move(*oTypeArgs));
            else
                curExp = factory.MakeSExp_Member(curExp, move(oIdToken->text), std::vector<STypeExp*>{});

            continue;
        }

        // exp -> id < > => (*exp).id
        if (Accept<MinusGreaterThanToken>(&curLexer, oLexResult))
        {
            auto oIdToken = Accept<IdentifierToken>(&curLexer);

            if (!oIdToken)
                return nullptr;

            // <
            auto oTypeArgs = ParseTypeArgs(&curLexer, factory);
            if (oTypeArgs)
            {   
                curExp = factory.MakeSExp_IndirectMember(curExp, move(oIdToken->text), move(*oTypeArgs));
            }
            else
            {   
                curExp = factory.MakeSExp_IndirectMember(curExp, move(oIdToken->text), vector<STypeExp*>{});
            }

            continue;
        }

        // (..., ... )             
        auto* arguments = ParseCallArgs(&curLexer, factory);
        if (arguments)
        {
            curExp = factory.MakeSExp_Call(curExp, arguments);
            continue;
        }

        break;
    }

    *lexer = curLexer;
    return curExp;
}

SExp* ParseSingleExp(Lexer* lexer, SFactory& factory)
{
    if (auto* exp = ParseBoxExp(lexer, factory))
        return exp;
        
    if (auto* exp = ParseNewExp(lexer, factory))
        return exp;
        
    if (auto* exp = ParseLambdaExp(lexer, factory))
        return exp;
        
    if (auto* exp = ParseParenExp(lexer, factory))
        return exp;
        
    if (auto* exp = ParseNullLiteralExp(lexer, factory))
        return exp;
        
    if (auto* exp = ParseBoolLiteralExp(lexer, factory))
        return exp;
        
    if (auto* exp = ParseIntLiteralExp(lexer, factory))
        return exp;
        
    if (auto* exp = ParseStringExp(lexer, factory))
        return exp;
        
    if (auto* exp = ParseListExp(lexer, factory))
        return exp;
        
    if (auto* exp = ParseIdentifierExp(lexer, factory))
        return exp;
        
    return nullptr;
}


SExp_Box* ParseBoxExp(Lexer* lexer, SFactory& factory)
{
    // <BOX> <EXP>
    Lexer curLexer = *lexer;

    if (!Accept<BoxToken>(&curLexer))
        return nullptr;

    auto* innerExp = ParseExp(&curLexer, factory);
    if (!innerExp)
        return nullptr;

    *lexer = move(curLexer);
    return factory.MakeSExp_Box(innerExp);
}

SExp_New* ParseNewExp(Lexer* lexer, SFactory& factory)
{
    // <NEW> <TYPEEXP> <LPAREN> CallArgs <RPAREN>
    Lexer curLexer = *lexer;
    
    if (!Accept<NewToken>(&curLexer))
        return nullptr;

    auto* type = ParseTypeExp(&curLexer, factory);
    if (!type)
        return nullptr;

    auto* args = ParseCallArgs(&curLexer, factory);
    
    if (!args)
        return nullptr;

    *lexer = move(curLexer);
    return factory.MakeSExp_New(type, args);
}

// LambdaExpression, Right Assoc
SExp_Lambda* ParseLambdaExp(Lexer* lexer, SFactory& factory)
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
                params.emplace_back(nullptr, move(oFirstIdToken->text), oOutAndParams->bOut, oOutAndParams->bParams);
            else
                params.emplace_back(factory.MakeSTypeExp_Id(move(oFirstIdToken->text), vector<STypeExp*>{}), move(oSecondIdToken->text), oOutAndParams->bOut, oOutAndParams->bParams);
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

        params.emplace_back(nullptr, move(oIdToken->text), oOutAndParams->bOut, oOutAndParams->bParams);
    }

    // =>
    if (!Accept<EqualGreaterThanToken>(&curLexer))
        return nullptr;
    

    // exp => return exp;
    // { ... }
    SLambdaExpBody* body;
    if (Peek<LBraceToken>(curLexer))
    {
        // Body 파싱을 그대로 쓴다
        auto oStmtBody = ParseBody(&curLexer, factory);
        if (!oStmtBody)
            return nullptr;

        body = factory.MakeSLambdaExpBody_Stmts(move(*oStmtBody));
    }
    else
    {
        // exp
        auto* exp = ParseExp(&curLexer, factory);
        if (!exp)
            return nullptr;

        body = factory.MakeSLambdaExpBody_Exp(exp);
    }

    *lexer = move(curLexer);
    return factory.MakeSExp_Lambda(move(params), body);
}

SExp* ParseParenExp(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    if (!Accept<LParenToken>(&curLexer))
        return nullptr;
    
    auto* oxp = ParseExp(&curLexer, factory);
    if (!oxp)
        return nullptr;
    
    if (!Accept<RParenToken>(&curLexer))
        return nullptr;

    *lexer = move(curLexer);
    return oxp;
}

SExp_NullLiteral* ParseNullLiteralExp(Lexer* lexer, SFactory& factory)
{
    if (!Accept<NullToken>(lexer))
        return nullptr;

    return factory.MakeSExp_NullLiteral();
}

SExp_BoolLiteral* ParseBoolLiteralExp(Lexer* lexer, SFactory& factory)
{
    auto oBoolToken = Accept<BoolToken>(lexer);

    if (!oBoolToken)
        return nullptr;

    return factory.MakeSExp_BoolLiteral(oBoolToken->value);
}

SExp_IntLiteral* ParseIntLiteralExp(Lexer* lexer, SFactory& factory)
{
    auto oIntToken = Accept<IntToken>(lexer);

    if (!oIntToken)
        return nullptr;

    return factory.MakeSExp_IntLiteral(oIntToken->value);
}

// 스트링 파싱
SExp_String* ParseStringExp(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    if (!Accept<DoubleQuoteToken>(&curLexer))
        return nullptr;

    vector<SStringExpElement*> elems;
    
    while (true)
    {
        auto oLexResult = curLexer.LexStringMode();
        if (Accept<DoubleQuoteToken>(&curLexer, oLexResult))
            break;

        if (auto oTextToken = Accept<TextToken>(&curLexer, oLexResult))
        {
            elems.push_back(factory.MakeSStringExpElement_Text(move(oTextToken->text)));
            continue;
        }
        
        if (auto oIdToken = Accept<IdentifierToken>(&curLexer, oLexResult))
        {
            elems.push_back(factory.MakeSStringExpElement_Exp(factory.MakeSExp_Identifier(move(oIdToken->text), std::vector<STypeExp*>{})));
            continue;
        }

        // ${
        if (Accept<DollarLBraceToken>(&curLexer, oLexResult))
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

        return nullptr;
    }

    *lexer = move(curLexer);
    return factory.MakeSExp_String(move(elems));
}

SExp_List* ParseListExp(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    if (!Accept<LBracketToken>(&curLexer))
        return nullptr;

    vector<SExp*> elems;    
    
    while (!Accept<RBracketToken>(&curLexer))
    {
        if (!elems.empty())
            if (!Accept<CommaToken>(&curLexer))
                return nullptr;

        auto* elem = ParseExp(&curLexer, factory);
        if (!elem)
            return nullptr;

        elems.push_back(elem);
    }

    *lexer = curLexer;
    return factory.MakeSExp_List(move(elems));
}

// lexer를 실패했을때 되돌리는 것은 Parser책임
SExp_Identifier* ParseIdentifierExp(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    auto oIdToken = Accept<IdentifierToken>(&curLexer);
    if (!oIdToken) return nullptr;

    // 실패해도 괜찮다
    auto oTypeArgs = ParseTypeArgs(&curLexer, factory);

    if (oTypeArgs)
    {
        *lexer = move(curLexer);
        return factory.MakeSExp_Identifier(move(oIdToken->text), move(*oTypeArgs));
    }
    else
    {
        *lexer = move(curLexer);
        return factory.MakeSExp_Identifier(move(oIdToken->text), std::vector<STypeExp*>{});
    }
}

}