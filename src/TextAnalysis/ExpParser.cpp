#include "pch.h"

#include <Syntax/ExpSyntaxes.h>
#include <Syntax/StmtSyntaxes.h>
#include <Syntax/Tokens.h>
#include <Syntax/ArgumentSyntax.h>
#include <Syntax/StringExpSyntaxElements.h>

#include <TextAnalysis/Lexer.h>
#include <TextAnalysis/ExpParser.h>
#include <TextAnalysis/StmtParser.h>
#include <TextAnalysis/TypeExpParser.h>

#include "ParserMisc.h"


using namespace std;

namespace {

using namespace Citron;
struct BinaryOpInfo { Token token; BinaryOpSyntaxKind kind; };
struct UnaryOpInfo { Token token; UnaryOpSyntaxKind kind; };

// utility
template<optional<ExpSyntax> (*ParseBaseExp)(Lexer* lexer), int N>
optional<ExpSyntax> ParseLeftAssocBinaryOpExp(Lexer* lexer, BinaryOpInfo (&infos)[N])
{
    Lexer curLexer = *lexer;
    auto oOperand0 = ParseBaseExp(&curLexer);

    if (!oOperand0)
        return nullopt;

    ExpSyntax curExp = *oOperand0;

    while (true)
    {
        optional<BinaryOpSyntaxKind> oOpKind;

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

        auto oOperand1 = ParseBaseExp(&curLexer);
        if (!oOperand1)
            return nullopt;
        
        // Fold
        curExp = BinaryOpExpSyntax(*oOpKind, std::move(curExp), std::move(*oOperand1));
    }
}

 // unary -가 int literal과 있을 때는 int literal로 합친다
optional<ExpSyntax> HandleUnaryMinusWithIntLiteral(UnaryOpSyntaxKind kind, ExpSyntax& exp)
{
    if (kind == UnaryOpSyntaxKind::Minus)
        if (auto* intLiteralExp = get_if<IntLiteralExpSyntax>(&exp))
            return IntLiteralExpSyntax(-intLiteralExp->GetValue());

    return nullopt;
}

optional<ArgumentSyntax> ParseArgument(Lexer* lexer)
{
    Lexer curLexer = *lexer;
    auto oOutAndParams = AcceptParseOutAndParams(&curLexer);
    if (!oOutAndParams)
        return nullopt;

    auto oExp = ParseExp(&curLexer);

    if (!oExp)
        return nullopt;

    *lexer = std::move(curLexer);
    return ArgumentSyntax{ oOutAndParams->bOut, oOutAndParams->bParams, std::move(*oExp) };
}

}

namespace Citron {

optional<vector<TypeExpSyntax>> ParseTypeArgs(Lexer* lexer);


optional<vector<ArgumentSyntax>> ParseCallArgs(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    if (!Accept<LParenToken>(&curLexer))
        return nullopt;

    vector<ArgumentSyntax> arguments;
    while (!Accept<RParenToken>(&curLexer))
    {
        if (!arguments.empty())
            if (!Accept<CommaToken>(&curLexer))
                return nullopt;

        auto oArg = ParseArgument(&curLexer);
        if (!oArg)
            return nullopt;

        arguments.push_back(*oArg);
    }

    *lexer = std::move(curLexer);
    return arguments; // 이건 move가 되는데..
}

optional<ExpSyntax> ParseExp(Lexer* lexer)
{
    return ParseAssignExp(lexer);
}

optional<ExpSyntax> ParseAssignExp(Lexer* lexer)
{   
    Lexer curLexer = *lexer;

    // base
    auto oExp0 = ParseEqualityExp(&curLexer);
    if (!oExp0) 
        return nullopt;

    if (!Accept<EqualToken>(&curLexer))
    {
        *lexer = std::move(curLexer);
        return oExp0;
    }

    auto oExp1 = ParseAssignExp(&curLexer);
    if (!oExp1)
        return nullopt;

    *lexer = std::move(curLexer);
    return BinaryOpExpSyntax(BinaryOpSyntaxKind::Assign, std::move(*oExp0), std::move(*oExp1));
}

optional<ExpSyntax> ParseEqualityExp(Lexer* lexer)
{
    static BinaryOpInfo equalityInfos[] = {
        { EqualEqualToken(), BinaryOpSyntaxKind::Equal },
        { ExclEqualToken(), BinaryOpSyntaxKind::NotEqual }
    };

    return ParseLeftAssocBinaryOpExp<&ParseTestAndTypeTestExp>(lexer, equalityInfos);
}

optional<ExpSyntax> ParseTestAndTypeTestExp(Lexer* lexer)
{
    static BinaryOpInfo testInfos[] = {
        { GreaterThanEqualToken(), BinaryOpSyntaxKind::GreaterThanOrEqual },
        { LessThanEqualToken(), BinaryOpSyntaxKind::LessThanOrEqual },
        { LessThanToken(), BinaryOpSyntaxKind::LessThan },
        { GreaterThanToken(), BinaryOpSyntaxKind::GreaterThan }
    };

    Lexer curLexer = *lexer;

    // base
    auto oOperand0 = ParseAdditiveExp(&curLexer);

    if (!oOperand0)
        return nullopt;

    ExpSyntax curExp = *oOperand0;
    
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
                auto oOperand1 = ParseAdditiveExp(&curLexer);

                if (!oOperand1)
                    return nullopt;

                // Fold
                curExp = BinaryOpExpSyntax(info.kind, std::move(curExp), std::move(*oOperand1));
                bHandled = true;
                break;
            }
        }

        if (bHandled) continue;

        if (holds_alternative<IsToken>(oLexResult->token))
        {
            curLexer = oLexResult->lexer;

            auto oTypeExp = ParseTypeExp(&curLexer);

            if (!oTypeExp)
                return nullopt;

            curExp = IsExpSyntax(std::move(curExp), std::move(*oTypeExp));
            continue;
        }

        if (holds_alternative<AsToken>(oLexResult->token))
        {
            curLexer = oLexResult->lexer;

            auto oTypeExp = ParseTypeExp(&curLexer);
            if (!oTypeExp) 
                return nullopt;

            curExp = AsExpSyntax(std::move(curExp), std::move(*oTypeExp));
            continue;
        }

        break;
    }

    *lexer = std::move(curLexer);
    return curExp;
}


std::optional<Citron::ExpSyntax> ParseAdditiveExp(Lexer* lexer)
{
    static BinaryOpInfo additiveInfos[] = {
        { PlusToken(), BinaryOpSyntaxKind::Add },
        { MinusToken(), BinaryOpSyntaxKind::Subtract },
    };

    return ParseLeftAssocBinaryOpExp<&ParseMultiplicativeExp>(lexer, additiveInfos);
}

std::optional<Citron::ExpSyntax> ParseMultiplicativeExp(Lexer* lexer)
{   
    static BinaryOpInfo multiplicativeInfos[] = {
        { StarToken(), BinaryOpSyntaxKind::Multiply },
        { SlashToken(), BinaryOpSyntaxKind::Divide },
        { PercentToken(), BinaryOpSyntaxKind::Modulo },
    };

    return ParseLeftAssocBinaryOpExp<&ParseUnaryExp>(lexer, multiplicativeInfos);
}

// base는 PrimaryExp
std::optional<Citron::ExpSyntax> ParseUnaryExp(Lexer* lexer)
{
    static UnaryOpInfo unaryInfos[] = {
        { MinusToken(), UnaryOpSyntaxKind::Minus},
        { ExclToken(), UnaryOpSyntaxKind::LogicalNot },
        { PlusPlusToken(), UnaryOpSyntaxKind::PrefixInc },
        { MinusMinusToken(), UnaryOpSyntaxKind::PrefixDec },
        { StarToken(), UnaryOpSyntaxKind::Deref }
    };

    Lexer curLexer = *lexer;
    optional<UnaryOpSyntaxKind> oOpKind;

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
        auto oExp = ParseUnaryExp(&curLexer);
        if (!oExp)
            return nullopt;

        // '-' '3'은 '-3'
        if (auto oHandledExp = HandleUnaryMinusWithIntLiteral(*oOpKind, *oExp))
        {
            *lexer = std::move(curLexer);
            return oHandledExp;
        }

        *lexer = std::move(curLexer);
        return UnaryOpExpSyntax(*oOpKind, std::move(*oExp));
    }
    else
    {
        // base
        return ParsePrimaryExp(lexer);
    }
}

// base ParseSingleExp
std::optional<Citron::ExpSyntax> ParsePrimaryExp(Lexer* lexer)
{
    static UnaryOpInfo primaryInfos[] = {
        { PlusPlusToken(), UnaryOpSyntaxKind::PostfixInc },
        { MinusMinusToken(), UnaryOpSyntaxKind::PostfixDec },
    };

    Lexer curLexer = *lexer;

    auto oOperand = ParseSingleExp(&curLexer);

    if (!oOperand)
        return nullopt;

    auto curExp = std::move(*oOperand);

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
            curExp = UnaryOpExpSyntax(primaryInfo->kind, std::move(curExp));
            continue;
        }

        // [ ... ]
        if (Accept<LBracketToken>(&curLexer))
        {
            auto oIndex = ParseExp(&curLexer);
            if (!oIndex)
                return nullopt;

            if (!Accept<RBraceToken>(&curLexer))
                return nullopt;

            curExp = IndexerExpSyntax(curExp, std::move(*oIndex));
            continue;
        }

        // . id < >
        if (Accept<DotToken>(&curLexer))
        {
            auto oIdToken = Accept<IdentifierToken>(&curLexer);

            if (!oIdToken)
                return nullopt;

            // <
            auto oTypeArgs = ParseTypeArgs(&curLexer);

            if (oTypeArgs)
                curExp = MemberExpSyntax(std::move(curExp), std::move(oIdToken->text), *oTypeArgs);
            else
                curExp = MemberExpSyntax(std::move(curExp), std::move(oIdToken->text), {});

            continue;
        }

        // exp -> id < > => (*exp).id
        if (Accept<MinusGreaterThanToken>(&curLexer, oLexResult))
        {
            auto oIdToken = Accept<IdentifierToken>(&curLexer);

            if (!oIdToken)
                return nullopt;

            // <
            auto oTypeArgs = ParseTypeArgs(&curLexer);
            if (oTypeArgs)
            {   
                curExp = MemberExpSyntax(UnaryOpExpSyntax(UnaryOpSyntaxKind::Deref, std::move(curExp)), std::move(oIdToken->text), *oTypeArgs);
            }
            else
            {   
                curExp = MemberExpSyntax(UnaryOpExpSyntax(UnaryOpSyntaxKind::Deref, std::move(curExp)), std::move(oIdToken->text), { });
            }

            continue;
        }

        // (..., ... )             
        auto oArguments = ParseCallArgs(&curLexer);
        if (oArguments)
        {
            curExp = CallExpSyntax(std::move(curExp), std::move(*oArguments));
            continue;
        }

        break;
    }

    *lexer = curLexer;
    return curExp;
}

optional<ExpSyntax> ParseSingleExp(Lexer* lexer)
{
    if (auto oExp = ParseBoxExp(lexer))
        return oExp;
        
    if (auto oExp = ParseNewExp(lexer))
        return oExp;
        
    if (auto oExp = ParseLambdaExp(lexer))
        return oExp;
        
    if (auto oExp = ParseParenExp(lexer))
        return oExp;
        
    if (auto oExp = ParseNullLiteralExp(lexer))
        return oExp;
        
    if (auto oExp = ParseBoolLiteralExp(lexer))
        return oExp;
        
    if (auto oExp = ParseIntLiteralExp(lexer))
        return oExp;
        
    if (auto oExp = ParseStringExp(lexer))
        return oExp;
        
    if (auto oExp = ParseListExp(lexer))
        return oExp;
        
    if (auto oExp = ParseIdentifierExp(lexer))
        return oExp;
        
    return nullopt;
}


optional<BoxExpSyntax> ParseBoxExp(Lexer* lexer)
{
    // <BOX> <EXP>
    Lexer curLexer = *lexer;

    if (!Accept<BoxToken>(&curLexer))
        return nullopt;

    auto oInnerExp = ParseExp(&curLexer);
    if (!oInnerExp)
        return nullopt;

    *lexer = std::move(curLexer);
    return BoxExpSyntax(std::move(*oInnerExp));
}

optional<NewExpSyntax> ParseNewExp(Lexer* lexer)
{
    // <NEW> <TYPEEXP> <LPAREN> CallArgs <RPAREN>
    Lexer curLexer = *lexer;
    
    if (!Accept<NewToken>(&curLexer))
        return nullopt;

    auto oType = ParseTypeExp(&curLexer);
    if (!oType)
        return nullopt;

    auto oArgs = ParseCallArgs(&curLexer);
    
    if (!oArgs)
        return nullopt;

    *lexer = std::move(curLexer);
    return NewExpSyntax(std::move(*oType), std::move(*oArgs));
}

// LambdaExpression, Right Assoc
optional<LambdaExpSyntax> ParseLambdaExp(Lexer* lexer)
{
    Lexer curLexer = *lexer;
    vector<LambdaExpParamSyntax> params;

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
                    return nullopt;

            auto oOutAndParams = AcceptParseOutAndParams(&curLexer);
            if (!oOutAndParams)
                return nullopt;

            // id id or id
            auto oFirstIdToken = Accept<IdentifierToken>(&curLexer);
            if (!oFirstIdToken)
                return nullopt;

            auto oSecondIdToken = Accept<IdentifierToken>(&curLexer);
            if (!oSecondIdToken)
                params.emplace_back(LambdaExpParamSyntax{ nullopt, std::move(oFirstIdToken->text), oOutAndParams->bOut, oOutAndParams->bParams });
            else
                params.emplace_back(LambdaExpParamSyntax{ IdTypeExpSyntax(std::move(oFirstIdToken->text), {}), std::move(oSecondIdToken->text), oOutAndParams->bOut, oOutAndParams->bParams });
        }
    }
    else
    {
        // out과 params는 동시에 쓸 수 없다
        auto oOutAndParams = AcceptParseOutAndParams(&curLexer);
        if (!oOutAndParams)
            return nullopt;
        
        auto oIdToken = Accept<IdentifierToken>(&curLexer);
        if (!oIdToken)
            return nullopt;

        params.emplace_back(LambdaExpParamSyntax{ nullopt, std::move(oIdToken->text), oOutAndParams->bOut, oOutAndParams->bParams });
    }

    // =>
    if (!Accept<EqualGreaterThanToken>(&curLexer))
        return nullopt;
    

    // exp => return exp;
    // { ... }
    vector<StmtSyntax> body;
    if (Peek<LBraceToken>(curLexer))
    {
        // Body 파싱을 그대로 쓴다
        auto oStmtBody = ParseBody(&curLexer);
        if (!oStmtBody)
            return nullopt;

        body = std::move(*oStmtBody);
    }
    else
    {
        // exp
        auto oExpBody = ParseExp(&curLexer);
        if (!oExpBody)
            return nullopt;

        body = { ReturnStmtSyntax(ReturnValueSyntaxInfo { std::move(*oExpBody) }) };
    }

    *lexer = std::move(curLexer);
    return LambdaExpSyntax(std::move(params), std::move(body));
}

optional<ExpSyntax> ParseParenExp(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    if (!Accept<LParenToken>(&curLexer))
        return nullopt;
    
    auto oExp = ParseExp(&curLexer);
    if (!oExp)
        return nullopt;
    
    if (!Accept<RParenToken>(&curLexer))
        return nullopt;

    *lexer = std::move(curLexer);
    return oExp;
}

optional<NullLiteralExpSyntax> ParseNullLiteralExp(Lexer* lexer)
{
    if (!Accept<NullToken>(lexer))
        return nullopt;

    return NullLiteralExpSyntax();
}

optional<BoolLiteralExpSyntax> ParseBoolLiteralExp(Lexer* lexer)
{
    auto oBoolToken = Accept<BoolToken>(lexer);

    if (!oBoolToken)
        return nullopt;

    return BoolLiteralExpSyntax(oBoolToken->value);
}

optional<IntLiteralExpSyntax> ParseIntLiteralExp(Lexer* lexer)
{
    auto oIntToken = Accept<IntToken>(lexer);

    if (!oIntToken)
        return nullopt;

    return IntLiteralExpSyntax(oIntToken->value);
}

// 스트링 파싱
optional<StringExpSyntax> ParseStringExp(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    if (!Accept<DoubleQuoteToken>(&curLexer))
        return nullopt;

    vector<StringExpSyntaxElement> elems;
    
    while (true)
    {
        auto oLexResult = curLexer.LexStringMode();
        if (Accept<DoubleQuoteToken>(&curLexer, oLexResult))
            break;

        if (auto oTextToken = Accept<TextToken>(&curLexer, oLexResult))
        {
            elems.push_back(TextStringExpSyntaxElement{ std::move(oTextToken->text) });
            continue;
        }
        
        if (auto oIdToken = Accept<IdentifierToken>(&curLexer, oLexResult))
        {
            elems.push_back(ExpStringExpSyntaxElement{ IdentifierExpSyntax(std::move(oIdToken->text), {}) });
            continue;
        }

        // ${
        if (Accept<DollarLBraceToken>(&curLexer, oLexResult))
        {
            // TODO: EndInnerExpToken 일때 빠져나와야 한다는 표시를 해줘야 한다
            auto oExp = ParseExp(&curLexer);
            if (!oExp)
                return nullopt;
            
            if (!Accept<RBraceToken>(&curLexer))
                return nullopt;

            elems.push_back(ExpStringExpSyntaxElement{ std::move(*oExp) });
            continue;
        }

        return nullopt;
    }

    *lexer = std::move(curLexer);
    return StringExpSyntax(std::move(elems));    
}

optional<ListExpSyntax> ParseListExp(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    if (!Accept<LBracketToken>(&curLexer))
        return nullopt;

    vector<ExpSyntax> elems;    
    
    while (!Accept<RBracketToken>(&curLexer))
    {
        if (!elems.empty())
            if (!Accept<CommaToken>(&curLexer))
                return nullopt;

        auto oElem = ParseExp(&curLexer);
        if (!oElem)
            return nullopt;

        elems.push_back(std::move(*oElem));
    }

    *lexer = curLexer;
    return ListExpSyntax(std::move(elems));
}

// lexer를 실패했을때 되돌리는 것은 Parser책임
optional<IdentifierExpSyntax> ParseIdentifierExp(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    auto oIdToken = Accept<IdentifierToken>(&curLexer);
    if (!oIdToken) return nullopt;

    // 실패해도 괜찮다
    auto oTypeArgs = ParseTypeArgs(&curLexer);

    if (oTypeArgs)
    {
        *lexer = std::move(curLexer);
        return IdentifierExpSyntax(std::move(oIdToken->text), std::move(*oTypeArgs));
    }
    else
    {
        *lexer = std::move(curLexer);
        return IdentifierExpSyntax(std::move(oIdToken->text), { });
    }
}

}