#include "TypeExpParser.h"

#include <optional>
#include <vector>
#include <memory>

#include "Infra/Ptr.h"
#include "Syntax/Syntax.h"
#include "Syntax/Tokens.h"
#include "Lexer.h"
#include "ParserMisc.h"

using namespace std;

namespace Citron {

// nullable, shared, ptr,
// T?**** 섞는거 불가능
// nullable<T> = T?, T?????불가능, 안될 이유는 없을거 같지만 일단 문법차원에서 막는다
// ptr<T> = T*, T***** 가능
// shared<T> = shared T
// local<T> = local T
// shared T* 불가능
// ref는 타입에 없다

// Formal =  // 오로지 id와 키워드만 있는 버전
//     | Id
//     | 'nullable' '<' TypeExp '>'
//     | 'shared' '<' TypeExp '>'
//     | 'local' '<' TypeExp '>'
//     ... 

// Postfix = '*'+ | '?'

// TypeExp = // prefix, postfix 가능한 버전
//     | Id Postfix
//     | 'nullable' '<' TypeExp '>' Postfix
//     | 'shared' '<' TypeExp '>' Postfix
//     | 'shared' Formal 
//     | 'local' '<' TypeExp '>' Postfix
//     ...

STypeExp* ParseIdChainTypeExp(Lexer* lexer, SFactory& factory);

// SFuncTypeExp* ParseFuncTypeExp(Lexer* lexer, SFactory& factory);
// STupleTypeExp* ParseTupleTypeExp(Lexer* lexer, SFactory& factory);

// postfix 처리를 하지 않는 버전
STypeExp* ParseFormalTypeExp_Keywords(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer{*lexer};

    auto oIdToken = Accept<IdentifierToken>(&curLexer);
    if (!oIdToken) return nullptr;

    if (oIdToken->text == "nullable")
    {
        if (!Accept<LessThanToken>(&curLexer)) return nullptr;
        auto oInnerTypeExp = ParseTypeExp(&curLexer, factory);
        if (!oInnerTypeExp) return nullptr;

        if (!Accept<GreaterThanToken>(&curLexer)) return nullptr;

        *lexer = move(curLexer);
        return factory.MakeSTypeExp_Nullable(move(oInnerTypeExp));
    }
    else if (oIdToken->text == "ptr")
    {
        if (!Accept<LessThanToken>(&curLexer)) return nullptr;
        auto oInnerTypeExp = ParseTypeExp(&curLexer, factory);
        if (!oInnerTypeExp) return nullptr;

        if (!Accept<GreaterThanToken>(&curLexer)) return nullptr;

        *lexer = move(curLexer);
        return factory.MakeSTypeExp_Ptr(move(oInnerTypeExp));
    }
    else if (oIdToken->text == "shared")
    {
        if (!Accept<LessThanToken>(&curLexer)) return nullptr;
        auto oInnerTypeExp = ParseTypeExp(&curLexer, factory);
        if (!oInnerTypeExp) return nullptr;

        if (!Accept<GreaterThanToken>(&curLexer)) return nullptr;

        *lexer = move(curLexer);
        return factory.MakeSTypeExp_Shared(move(oInnerTypeExp));
    }
    else if (oIdToken->text == "local")
    {   
        if (!Accept<LessThanToken>(&curLexer)) return nullptr;
        auto oInnerTypeExp = ParseTypeExp(&curLexer, factory);
        if (!oInnerTypeExp) return nullptr;

        if (!Accept<GreaterThanToken>(&curLexer)) return nullptr;

        *lexer = move(curLexer);
        return factory.MakeSTypeExp_Local(move(oInnerTypeExp));
    }

    return nullptr;
}

STypeExp* ParseFormalTypeExp(Lexer* lexer, SFactory& factory)
{
    if (STypeExp* typeExp = ParseFormalTypeExp_Keywords(lexer, factory))
        return typeExp;

    if (STypeExp* typeExp = ParseIdChainTypeExp(lexer, factory))
        return typeExp;

    return nullptr;
}

STypeExp* ParseTypeExp_Postfix(STypeExp* typeExp, Lexer* lexer, SFactory& factory)
{
    Lexer curLexer{*lexer};

    if (Accept<StarToken>(&curLexer))
    {
        typeExp = factory.MakeSTypeExp_Ptr(move(typeExp));

        while (Accept<StarToken>(&curLexer))
            typeExp = factory.MakeSTypeExp_Ptr(move(typeExp));

        *lexer = move(curLexer);
        return typeExp;
    }
    else if (Accept<QuestionToken>(&curLexer))
    {
        typeExp = factory.MakeSTypeExp_Nullable(move(typeExp));
        *lexer = move(curLexer);
        return typeExp;
    }
    else return typeExp;
}

template<typename TResultType, TResultType* (*PostfixFunc)(STypeExp*, Lexer*, SFactory&), TResultType* (*Wrapper)(STypeExp*, SFactory&)>
TResultType* TParseTypeExpKeywords(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer{*lexer};

    auto oIdToken = Accept<IdentifierToken>(&curLexer);
    if (!oIdToken) return nullptr;
    
    if (oIdToken->text == "nullable")
    {
        if (!Accept<LessThanToken>(&curLexer)) return nullptr;
        auto oInnerTypeExp = ParseTypeExp(&curLexer, factory);
        if (!oInnerTypeExp) return nullptr;

        if (!Accept<GreaterThanToken>(&curLexer)) return nullptr;        

        *lexer = move(curLexer);
        auto* typeExp = factory.MakeSTypeExp_Nullable(move(oInnerTypeExp));
        return PostfixFunc(typeExp, lexer, factory);
    }
    else if (oIdToken->text == "ptr")
    {
        if (!Accept<LessThanToken>(&curLexer)) return nullptr;
        auto oInnerTypeExp = ParseTypeExp(&curLexer, factory);
        if (!oInnerTypeExp) return nullptr;

        if (!Accept<GreaterThanToken>(&curLexer)) return nullptr;

        *lexer = move(curLexer);
        auto* typeExp = factory.MakeSTypeExp_Ptr(move(oInnerTypeExp));
        return PostfixFunc(typeExp, lexer, factory);
    }
    else if (oIdToken->text == "shared") 
    {
        // <가 있느냐 여부
        if (Accept<LessThanToken>(&curLexer))
        {
            auto oInnerTypeExp = ParseTypeExp(&curLexer, factory);
            if (!oInnerTypeExp) return nullptr;

            if (!Accept<GreaterThanToken>(&curLexer)) return nullptr;

            *lexer = move(curLexer);
            auto* typeExp = factory.MakeSTypeExp_Shared(move(oInnerTypeExp));
            return PostfixFunc(typeExp, lexer, factory);
        }
        else
        {
            // shared T 꼴
            auto* innerTypeExp = ParseFormalTypeExp(&curLexer, factory);
            if (!innerTypeExp) return nullptr;

            *lexer = move(curLexer);
            return Wrapper(factory.MakeSTypeExp_Shared(innerTypeExp), factory);
        }
    }
    else if (oIdToken->text == "local")
    {
        // <가 있느냐 여부
        if (Accept<LessThanToken>(&curLexer))
        {
            auto oInnerTypeExp = ParseTypeExp(&curLexer, factory);
            if (!oInnerTypeExp) return nullptr;

            if (!Accept<GreaterThanToken>(&curLexer)) return nullptr;

            *lexer = move(curLexer);
            auto* typeExp = factory.MakeSTypeExp_Local(move(oInnerTypeExp));
            return PostfixFunc(typeExp, lexer, factory);
        }
        else
        {
            // local T 꼴
            auto* innerTypeExp = ParseFormalTypeExp(&curLexer, factory);
            if (!innerTypeExp) return nullptr;

            *lexer = move(curLexer);
            return Wrapper(factory.MakeSTypeExp_Local(innerTypeExp), factory);
        }
    }

    return nullptr;
}

STypeExp* Identity(STypeExp* typeExp, SFactory& factory) { return typeExp; }

// postfix 처리를 하는 버전
STypeExp* ParseTypeExp(Lexer* lexer, SFactory& factory)
{
    // 항상 nullable, ptr, shared, local 먼저
    if (STypeExp* typeExp = TParseTypeExpKeywords<STypeExp, ParseTypeExp_Postfix, Identity> (lexer, factory))
        return typeExp;

    if (STypeExp* typeExp = ParseIdChainTypeExp(lexer, factory))
        return ParseTypeExp_Postfix(typeExp, lexer, factory);

    return nullptr;
}

SVarDeclType* ParseVarDeclTypeExp_Postfix(STypeExp* typeExp, Lexer* lexer, SFactory& factory)
{
    Lexer curLexer{*lexer};

    if (Accept<StarToken>(&curLexer))
    {
        typeExp = factory.MakeSTypeExp_Ptr(typeExp);

        while (Accept<StarToken>(&curLexer))
            typeExp = factory.MakeSTypeExp_Ptr(typeExp);

        *lexer = move(curLexer);
        return factory.MakeSVarDeclType_Normal(typeExp);
    }
    else if (Accept<QuestionToken>(&curLexer))
    {
        typeExp = factory.MakeSTypeExp_Nullable(typeExp);
        *lexer = move(curLexer);
        return factory.MakeSVarDeclType_Normal(typeExp);
    }
    else if (Accept<AmpersandToken>(&curLexer))
    {
        *lexer = move(curLexer);
        return factory.MakeSVarDeclType_Ref(typeExp);
    }
    else return factory.MakeSVarDeclType_Normal(typeExp);
}

SVarDeclType* ParseVarDeclTypeExp_Var(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer{*lexer};
    auto oVarToken = Accept<IdentifierToken>(&curLexer);
    if (!oVarToken) return nullptr;

    // shared var 처리
    if (oVarToken->text == "shared")
    {
        auto oNextVarToken = Accept<IdentifierToken>(&curLexer);
        if (!oNextVarToken) return nullptr;
        if (oNextVarToken->text != "var") return nullptr;

        *lexer = move(curLexer);
        return factory.MakeSVarDeclType_Var(SVarDeclType_VarKind::Shared);
    }

    if (oVarToken->text != "var") return nullptr;

    if (Accept<StarToken>(&curLexer))
    {
        *lexer = move(curLexer);
        return factory.MakeSVarDeclType_Var(SVarDeclType_VarKind::Ptr);
    }
    else if (Accept<QuestionToken>(&curLexer))
    {
        *lexer = move(curLexer);
        return factory.MakeSVarDeclType_Var(SVarDeclType_VarKind::Nullable);
    }
    else if (Accept<AmpersandToken>(&curLexer))
    {
        *lexer = move(curLexer);
        return factory.MakeSVarDeclType_VarRef();
    }
    else
    {
        *lexer = move(curLexer);
        return factory.MakeSVarDeclType_Var(SVarDeclType_VarKind::Normal);
    }
}

SVarDeclType* ParseVarDeclTypeExp_Wrapper(STypeExp* typeExp, SFactory& factory)
{
    return factory.MakeSVarDeclType_Normal(typeExp);
}

// var와 &를 더 처리 하는 버전
SVarDeclType* ParseVarDeclTypeExp(Lexer* lexer, SFactory& factory)
{
    // var, var*, var?, var& 처리
    if (auto* varType = ParseVarDeclTypeExp_Var(lexer, factory))
        return varType;

    // nullable, ptr, shared, local 먼저
    if (SVarDeclType* typeExp = TParseTypeExpKeywords<SVarDeclType, ParseVarDeclTypeExp_Postfix, ParseVarDeclTypeExp_Wrapper>(lexer, factory))
        return typeExp;

    if (STypeExp* typeExp = ParseIdChainTypeExp(lexer, factory))
        return ParseVarDeclTypeExp_Postfix(typeExp, lexer, factory);

    return nullptr;
}

optional<vector<STypeExp*>> ParseTypeArgs(Lexer* lexer, SFactory& factory)
{
    vector<STypeExp*> typeArgs;

    Lexer curLexer = *lexer;

    if (!Accept<LessThanToken>(&curLexer))
        return nullopt;

    while (!Accept<GreaterThanToken>(&curLexer))
    {
        if (!typeArgs.empty())
            if (!Accept<CommaToken>(&curLexer))
                return nullopt;

        auto oTypeArg = ParseTypeExp(&curLexer, factory);
        if (!oTypeArg)
            return nullopt;

        typeArgs.push_back(move(oTypeArg));
    }

    *lexer = move(curLexer);
    return typeArgs;
}

STypeExp_Id* ParseIdTypeExp(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    auto oIdToken = Accept<IdentifierToken>(&curLexer);
    if (!oIdToken)
        return nullptr;

    if (oIdToken->text == "nullable" || oIdToken->text == "ptr" || oIdToken->text == "shared" || oIdToken->text == "local")
        return nullptr;

    if (auto oTypeArgs = ParseTypeArgs(&curLexer, factory))
    {
        *lexer = move(curLexer);
        return factory.MakeSTypeExp_Id(oIdToken->text, move(*oTypeArgs));
    }
    else
    {
        *lexer = move(curLexer);
        return factory.MakeSTypeExp_Id(oIdToken->text, vector<STypeExp*>());
    }
}

// ID...
STypeExp* ParseIdChainTypeExp(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    auto* idTypeExp = ParseIdTypeExp(&curLexer, factory);
    if (!idTypeExp)
        return nullptr;

    STypeExp* curTypeExp = idTypeExp;

    // .
    while (Accept<DotToken>(&curLexer))
    {
        // ID
        auto oIdToken = Accept<IdentifierToken>(&curLexer);
        if (!oIdToken)
            return nullptr;

        auto oTypeArgs = ParseTypeArgs(&curLexer, factory);
        if (oTypeArgs)
            curTypeExp = factory.MakeSTypeExp_Member(curTypeExp, move(oIdToken->text), move(*oTypeArgs));
        else 
            curTypeExp = factory.MakeSTypeExp_Member(curTypeExp, move(oIdToken->text), std::vector<STypeExp*>{});
    }

    *lexer = move(curLexer);
    return curTypeExp;
}

// func<>
// std::optional<SFuncTypeExp> ParseFuncTypeExp(Lexer* lexer);

// swift some
// some<>

// tuple
// std::optional<STupleTypeExp> ParseTupleTypeExp(Lexer* lexer);

}