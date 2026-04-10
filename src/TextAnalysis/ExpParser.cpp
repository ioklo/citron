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

        if (auto o_lexResult = curLexer.LexNormalMode(true))
        {
            for(auto& info : infos)
            {
                if (info.token == o_lexResult->token)
                {
                    oOpKind = info.kind;
                    curLexer = o_lexResult->lexer;
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

// argModifier는 ref, move, forward, out, params가능
optional<SArgModifier> ParseArgModifier(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    SArgModifier argModifier;
    if (Accept<RefToken>(&curLexer))
    {
        argModifier = SArgModifier::Ref;
    }
    else if (Accept<MoveToken>(&curLexer))
    {
        argModifier = SArgModifier::Move;
    }
    else if (Accept<OutToken>(&curLexer))
    {
        argModifier = SArgModifier::Out;
    }
    else if (Accept<ParamsToken>(&curLexer))
    {
        argModifier = SArgModifier::Params;
    }
    else if (auto o_idToken = Accept<IdentifierToken>(&curLexer))
    {
        if (o_idToken->text == "forward")
        {
            argModifier = SArgModifier::Forward;
        }
        else return nullopt;
    }
    else return nullopt;

    *lexer = move(curLexer);
    return argModifier;
}

SArgument* ParseArgument(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    optional<SArgModifier> argModifier = ParseArgModifier(&curLexer);
    
    auto* exp = ParseExp(&curLexer, factory);

    if (!exp)
        return nullptr;

    *lexer = move(curLexer);
    return factory.MakeSArgument(argModifier, exp);
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

        auto o_lexResult = curLexer.LexNormalMode(true);
        if (!o_lexResult) break;

        // search binary
        for(auto& info : testInfos)
        {
            if (info.token == o_lexResult->token)
            {
                curLexer = move(o_lexResult->lexer);

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

        if (holds_alternative<IsToken>(o_lexResult->token))
        {
            curLexer = o_lexResult->lexer;

            auto* typeExp = ParseTypeExp(&curLexer, factory);

            optional<string> o_bindName;
            if (auto o_idToken = Accept<IdentifierToken>(&curLexer))
                o_bindName = move(o_idToken->text);

            if (!typeExp)
                return nullptr;

            curExp = factory.MakeSExp_Is(curExp, typeExp, move(o_bindName));
            continue;
        }

        if (holds_alternative<AsToken>(o_lexResult->token))
        {
            curLexer = o_lexResult->lexer;

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
        { StarToken(), SUnaryOpKind::Deref },
        { AmpersandToken(), SUnaryOpKind::Ref },
    };

    Lexer curLexer = *lexer;
    optional<SUnaryOpKind> oOpKind;

    auto o_lexResult = curLexer.LexNormalMode(true);
    if (o_lexResult)
    {
        for(auto& info : unaryInfos)
        {
            if (info.token == o_lexResult->token)
            {
                oOpKind = info.kind;
                curLexer = move(o_lexResult->lexer);
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
        auto o_lexResult = curLexer.LexNormalMode(true);
        if (!o_lexResult) break;

        optional<UnaryOpInfo> primaryInfo;

        for (auto& info : primaryInfos)
        {
            if (info.token == o_lexResult->token)
            {
                // TODO: postfix++이 두번 이상 나타나지 않도록 한다
                primaryInfo = info;
                break;
            }
        }

        if (primaryInfo)
        {
            curLexer = o_lexResult->lexer;

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
            auto o_idToken = Accept<IdentifierToken>(&curLexer);

            if (!o_idToken)
                return nullptr;

            // <
            auto o_typeArgs = ParseTypeArgs(&curLexer, factory);

            if (o_typeArgs)
                curExp = factory.MakeSExp_Member(curExp, move(o_idToken->text), move(*o_typeArgs));
            else
                curExp = factory.MakeSExp_Member(curExp, move(o_idToken->text), std::vector<STypeExp*>{});

            continue;
        }

        // exp -> id < > => (*exp).id
        if (Accept<MinusGreaterThanToken>(&curLexer, o_lexResult))
        {
            auto o_idToken = Accept<IdentifierToken>(&curLexer);

            if (!o_idToken)
                return nullptr;

            // <
            auto o_typeArgs = ParseTypeArgs(&curLexer, factory);
            if (o_typeArgs)
            {   
                // (*exp).id로 변경
                auto* deref = factory.MakeSExp_UnaryOp(SUnaryOpKind::Deref, curExp);
                curExp = factory.MakeSExp_Member(deref, move(o_idToken->text), move(*o_typeArgs));
            }
            else
            {   
                // (*exp).id로 변경
                auto* deref = factory.MakeSExp_UnaryOp(SUnaryOpKind::Deref, curExp);
                curExp = factory.MakeSExp_Member(deref, move(o_idToken->text), vector<STypeExp*>{});
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
    if (auto* exp = ParseInlineExp(lexer, factory))
        return exp;

    if (auto* exp = ParseSharedExp(lexer, factory))
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

struct SExpBlock
{
    vector<SStmt*> stmts;
    SExp* o_finalExp;
};

optional<SExpBlock> ParseExpBlock(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer{*lexer};

    if (!Accept<LBraceToken>(&curLexer))
        return nullopt;

    vector<SStmt*> stmts;
    auto* firstStmt = ParseStmt(&curLexer, factory);
    if (!firstStmt) return nullopt; // 하나는 무조건 있어야 함
    stmts.push_back(firstStmt);
    
    while (auto* stmt = ParseStmt(&curLexer, factory))
        stmts.push_back(stmt);

    SExp* o_finalExp = ParseExp(&curLexer, factory); // optional final exp
    if (!Accept<RBraceToken>(&curLexer)) return nullopt;

    *lexer = move(curLexer);
    return SExpBlock{move(stmts), o_finalExp};
}

SExp_Inline* ParseInlineExp(Lexer* lexer, SFactory& factory)
{
    // <INLINE> <LBRACE> <STMT>+ <EXP>? <RBRACE>
    Lexer curLexer = *lexer;
    
    if (!Accept<InlineToken>(&curLexer))
        return nullptr;

    auto o_expBlock = ParseExpBlock(&curLexer, factory);
    if (!o_expBlock) return nullptr;

    *lexer = move(curLexer);
    return factory.Make<SExp_Inline>(move(o_expBlock->stmts), o_expBlock->o_finalExp);
}

SExp_Shared* ParseSharedExp(Lexer* lexer, SFactory& factory)
{
    // <SHARED> <EXP>
    Lexer curLexer = *lexer;
    
    if (!Accept<SharedToken>(&curLexer))
        return nullptr;

    auto* innerExp = ParseExp(&curLexer, factory);
    if (!innerExp)
        return nullptr;

    *lexer = move(curLexer);
    return factory.MakeSExp_Shared(innerExp);
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

            auto o_paramModifier = ParseParamModifier(&curLexer);

            // id id or id
            auto o_FirstIdToken = Accept<IdentifierToken>(&curLexer);
            if (!o_FirstIdToken)
                return nullptr;

            auto o_secondIdToken = Accept<IdentifierToken>(&curLexer);
            if (!o_secondIdToken)
                params.emplace_back(o_paramModifier, nullptr, move(o_FirstIdToken->text));
            else
                params.emplace_back(o_paramModifier, factory.MakeSTypeExp_Id(move(o_FirstIdToken->text), vector<STypeExp*>{}), move(o_secondIdToken->text));
        }
    }
    else
    {   
        auto o_paramModifier = ParseParamModifier(&curLexer);
        
        auto o_idToken = Accept<IdentifierToken>(&curLexer);
        if (!o_idToken)
            return nullptr;

        params.emplace_back(o_paramModifier, nullptr, move(o_idToken->text));
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
        auto o_stmtBody = ParseBody(&curLexer, factory);
        if (!o_stmtBody)
            return nullptr;

        body = factory.MakeSLambdaExpBody_Stmts(move(*o_stmtBody));
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
    auto o_boolToken = Accept<BoolToken>(lexer);

    if (!o_boolToken)
        return nullptr;

    return factory.MakeSExp_BoolLiteral(o_boolToken->value);
}

SExp_IntLiteral* ParseIntLiteralExp(Lexer* lexer, SFactory& factory)
{
    auto o_intToken = Accept<IntToken>(lexer);

    if (!o_intToken)
        return nullptr;

    return factory.MakeSExp_IntLiteral(o_intToken->value);
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
        auto o_lexResult = curLexer.LexStringMode();
        if (Accept<DoubleQuoteToken>(&curLexer, o_lexResult))
            break;

        if (auto o_textToken = Accept<TextToken>(&curLexer, o_lexResult))
        {
            elems.push_back(factory.MakeSStringExpElement_Text(move(o_textToken->text)));
            continue;
        }
        
        if (auto o_idToken = Accept<IdentifierToken>(&curLexer, o_lexResult))
        {
            elems.push_back(factory.MakeSStringExpElement_Exp(factory.MakeSExp_Identifier(move(o_idToken->text), std::vector<STypeExp*>{})));
            continue;
        }

        // ${
        if (Accept<DollarLBraceToken>(&curLexer, o_lexResult))
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

    auto o_idToken = Accept<IdentifierToken>(&curLexer);
    if (!o_idToken) return nullptr;

    // 실패해도 괜찮다
    auto o_typeArgs = ParseTypeArgs(&curLexer, factory);

    if (o_typeArgs)
    {
        *lexer = move(curLexer);
        return factory.MakeSExp_Identifier(move(o_idToken->text), move(*o_typeArgs));
    }
    else
    {
        *lexer = move(curLexer);
        return factory.MakeSExp_Identifier(move(o_idToken->text), std::vector<STypeExp*>{});
    }
}

}