#include "pch.h"
#include <TextAnalysis/ScriptParser.h>

#include <optional>

#include <Syntax/FuncParamSyntax.h>
#include <Syntax/GlobalFuncDeclSyntax.h>
#include <Syntax/TypeDeclSyntax.h>
#include <Syntax/NamespaceDeclSyntaxElements.h>
#include <Syntax/ScriptSyntax.h>

#include <TextAnalysis/Lexer.h>
#include <TextAnalysis/ExpParser.h>

#include "TypeExpParser.h"
#include "ParserMisc.h"
#include "StmtParser.h"


using namespace std;

namespace Citron {

optional<EnumDeclSyntax> ParseEnumDecl(Lexer* lexer);
optional<StructDeclSyntax> ParseStructDecl(Lexer* lexer);
optional<ClassDeclSyntax> ParseClassDecl(Lexer* lexer);
optional<AccessModifierSyntax> ParseAccessModifier(Lexer* lexer);
optional<NamespaceDeclSyntax> ParseNamespaceDecl(Lexer* lexer);

// int t
// ref int t
// params T t
optional<FuncParamSyntax> ParseFuncDeclParam(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    auto oOutAndParams = AcceptParseOutAndParams(&curLexer);
    if (!oOutAndParams)
        return nullopt;

    auto oTypeExp = ParseTypeExp(&curLexer);
    if (!oTypeExp)
        return nullopt;

    auto oName = Accept<IdentifierToken>(&curLexer);
    if (!oName)
        return nullopt;

    *lexer = std::move(curLexer);
    return FuncParamSyntax{ oOutAndParams->bOut, oOutAndParams->bParams, std::move(*oTypeExp), std::move(oName->text) };
}

optional<vector<FuncParamSyntax>> ParseFuncDeclParams(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    if (!Accept<LParenToken>(&curLexer))
        return nullopt;

    vector<FuncParamSyntax> params;
    while (!Accept<RParenToken>(&curLexer))
    {
        if (!params.empty())
            if (!Accept<CommaToken>(&curLexer))
                return nullopt;

        auto oParam = ParseFuncDeclParam(&curLexer);
        if (!oParam)
            return nullopt;

        params.push_back(std::move(*oParam));
    }

    *lexer = std::move(curLexer);
    return params;
}

optional<GlobalFuncDeclSyntax> ParseGlobalFuncDecl(Lexer* lexer)
{
    // <SEQ> <RetTypeName> <FuncName> <LPAREN> <ARGS> <RPAREN>
    // LBRACE>
    // [Stmt]
    // <RBRACE>   

    Lexer curLexer = *lexer;

    // seq
    auto bSequence = Accept<SeqToken>(&curLexer).has_value();

    auto oRetType = ParseTypeExp(&curLexer);
    if (!oRetType)
        return nullopt;

    auto oFuncName = Accept<IdentifierToken>(&curLexer);
    if (!oFuncName)
        return nullopt;

    auto oParameters = ParseFuncDeclParams(&curLexer);
    if (!oParameters)
        return nullopt;

    auto oBody = ParseBody(&curLexer);
    if (!oBody)
        return nullopt;

    *lexer = std::move(curLexer);

    return GlobalFuncDeclSyntax(
        nullopt, // TODO: [7] 일단 null
        bSequence,
        std::move(*oRetType),
        std::move(oFuncName->text),
        {},
        std::move(*oParameters),
        std::move(*oBody)
    );
}

// <T1, T2, ...>
optional<vector<TypeParamSyntax>> ParseTypeParams(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    // typeParams
    vector<TypeParamSyntax> typeParams;
    if (Accept<LessThanToken>(&curLexer))
    {
        while (!Accept<GreaterThanToken>(&curLexer))
        {
            if (!typeParams.empty())
                if (!Accept<CommaToken>(&curLexer))
                    return nullopt;

            // 변수 이름만 받을 것이므로 TypeExp가 아니라 Identifier여야 한다
            auto oTypeParam = Accept<IdentifierToken>(&curLexer);
            if (!oTypeParam)
                return nullopt;

            typeParams.push_back(TypeParamSyntax{ oTypeParam->text });
        }
    }

    *lexer = std::move(curLexer);
    return typeParams;
}

optional<TypeDeclSyntax> ParseTypeDecl(Lexer* lexer)
{
    if (auto oTypeDecl = ParseEnumDecl(lexer))
        return oTypeDecl;

    if (auto oTypeDecl = ParseStructDecl(lexer))
        return oTypeDecl;

    if (auto oTypeDecl = ParseClassDecl(lexer))
        return oTypeDecl;

    return nullopt;
}

optional<EnumDeclSyntax> ParseEnumDecl(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    // public enum E<T1, T2> { a , b () } 
    auto oAccessModifier = ParseAccessModifier(&curLexer);

    if (!Accept<EnumToken>(&curLexer))
        return nullopt;
    
    auto oEnumName = Accept<IdentifierToken>(&curLexer);
    if (!oEnumName)
        return nullopt;

    auto oTypeParams = ParseTypeParams(&curLexer);
    if (!oTypeParams)
        return nullopt;

    if (!Accept<LBraceToken>(&curLexer))
        return nullopt;

    vector<EnumElemDeclSyntax> elems;
    while (!Accept<RBraceToken>(&curLexer))
    {
        if (!elems.empty())
            if (!Accept<CommaToken>(&curLexer))
                return nullopt;

        auto oElemName = Accept<IdentifierToken>(&curLexer);
        if (!oElemName)
            return nullopt;

        vector<EnumElemMemberVarDeclSyntax> params;
        
        if (Accept<LParenToken>(&curLexer))
        {
            while (!Accept<RParenToken>(&curLexer))
            {
                if (!params.empty())
                    if (!Accept<CommaToken>(&curLexer))
                        return nullopt;
                
                auto oTypeExp = ParseTypeExp(&curLexer);
                if (!oTypeExp)
                    return nullopt;

                auto oParamName = Accept<IdentifierToken>(&curLexer);
                if (!oParamName)
                    return nullopt;

                params.push_back(EnumElemMemberVarDeclSyntax(std::move(*oTypeExp), std::move(oParamName->text)));
            }
        }

        elems.push_back(EnumElemDeclSyntax(std::move(oElemName->text), std::move(params)));
    }

    *lexer = std::move(curLexer);
    return EnumDeclSyntax(oAccessModifier, std::move(oEnumName->text), std::move(*oTypeParams), std::move(elems));    
}

optional<AccessModifierSyntax> ParseAccessModifier(Lexer* lexer)
{
    if (Accept<ProtectedToken>(lexer))
        return AccessModifierSyntax::Protected;

    if (Accept<PrivateToken>(lexer))
        return AccessModifierSyntax::Private;

    if (Accept<PublicToken>(lexer))
        return AccessModifierSyntax::Public;

    return nullopt;
}

optional<StructMemberTypeDeclSyntax> ParseStructMemberTypeDecl(Lexer* lexer)
{
    if (auto oTypeDecl = ParseTypeDecl(lexer))
        return StructMemberTypeDeclSyntax(*oTypeDecl);

    return nullopt;
}

optional<StructMemberVarDeclSyntax> ParseStructMemberVarDecl(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    auto oAccessModifier = ParseAccessModifier(&curLexer);

    // ex) int
    auto oVarType = ParseTypeExp(&curLexer);
    if (!oVarType)
        return nullopt;

    // ex) x, y, z
    vector<u32string> varNames;

    auto oVarNameToken0 = Accept<IdentifierToken>(&curLexer);
    if (!oVarNameToken0)
        return nullopt;

    varNames.push_back(std::move(oVarNameToken0->text));

    while (Accept<CommaToken>(&curLexer))
    {
        auto oVarNameToken = Accept<IdentifierToken>(&curLexer);
        if (!oVarNameToken)
            return nullopt;

        varNames.push_back(std::move(oVarNameToken->text));
    }

    // ;
    if (!Accept<SemiColonToken>(&curLexer))
        return nullopt;

    *lexer = std::move(curLexer);

    return StructMemberVarDeclSyntax(oAccessModifier, std::move(*oVarType), std::move(varNames));
}

optional<StructMemberFuncDeclSyntax> ParseStructMemberFuncDecl(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    auto oAccessModifier = ParseAccessModifier(&curLexer);

    bool bStatic = Accept<StaticToken>(&curLexer).has_value();
    bool bSequence = Accept<SeqToken>(&curLexer).has_value();

    // ex) void
    auto oRetType = ParseTypeExp(&curLexer);
    if (!oRetType)
        return nullopt;

    // ex) F
    auto oFuncName = Accept<IdentifierToken>(&curLexer);
    if (!oFuncName)
        return nullopt;

    // ex) <T1, T2>
    auto oTypeParams = ParseTypeParams(&curLexer);
    if (!oTypeParams)
        return nullopt;

    // ex) (int i, int a)
    auto oParameters = ParseFuncDeclParams(&curLexer);
    if (!oParameters)
        return nullopt;

    // ex) { ... }
    auto oBody = ParseBody(&curLexer);
    if (!oBody)
        return nullopt;

    *lexer = std::move(curLexer);
    return StructMemberFuncDeclSyntax(
        oAccessModifier, bStatic, bSequence, std::move(*oRetType), std::move(oFuncName->text), std::move(*oTypeParams), std::move(*oParameters), std::move(*oBody)
    );
}

optional<StructConstructorDeclSyntax> ParseStructConstructorDecl(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    auto oAccessModifier = ParseAccessModifier(&curLexer);

    // ex) F
    auto oName = Accept<IdentifierToken>(&curLexer);
    if (!oName)
        return nullopt;

    // ex) (int i, int a)
    auto oParameters = ParseFuncDeclParams(&curLexer);
    if (!oParameters)
        return nullopt;

    // ex) { ... }
    auto oBody = ParseBody(&curLexer);
    if (!oBody)
        return nullopt;

    *lexer = std::move(curLexer);
    return StructConstructorDeclSyntax(oAccessModifier, std::move(oName->text), std::move(*oParameters), std::move(*oBody));
}

optional<StructMemberDeclSyntax> ParseStructMemberDecl(Lexer* lexer)
{
    if (auto oMemberDecl = ParseStructMemberTypeDecl(lexer))
        return oMemberDecl;

    if (auto oMemberDecl = ParseStructMemberFuncDecl(lexer))
        return oMemberDecl;

    if (auto oMemberDecl = ParseStructConstructorDecl(lexer))
        return oMemberDecl;

    if (auto oMemberDecl = ParseStructMemberVarDecl(lexer))
        return oMemberDecl;

    return nullopt;
}

optional<StructDeclSyntax> ParseStructDecl(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    // AccessModifier, 텍스트에는 없을 수 있다
    auto oAccessModifier = ParseAccessModifier(&curLexer);

    if (!Accept<StructToken>(&curLexer))
        return nullopt;

    auto oStructName = Accept<IdentifierToken>(&curLexer);
    if (!oStructName)
        return nullopt;

    auto oTypeParams = ParseTypeParams(&curLexer);
    if (!oTypeParams)
        return nullopt;

    // 상속 부분 : B, I, ...
    vector<TypeExpSyntax> baseTypes;    
    if (Accept<ColonToken>(&curLexer))
    {
        auto oBaseType0 = ParseTypeExp(&curLexer);
        if (!oBaseType0)
            return nullopt;

        baseTypes.push_back(std::move(*oBaseType0));

        while (Accept<CommaToken>(&curLexer))
        {
            auto oBaseType = ParseTypeExp(&curLexer);
            if (!oBaseType)
                return nullopt;

            baseTypes.push_back(std::move(*oBaseType));
        }
    }

    vector<StructMemberDeclSyntax> elems;

    // {
    if (!Accept<LBraceToken>(&curLexer))
        return nullopt;

    // } 나올때까지
    while (!Accept<RBraceToken>(&curLexer))
    {
        auto oElem = ParseStructMemberDecl(&curLexer);
        if (!oElem)
            return nullopt;

        elems.push_back(std::move(*oElem));
    }
    
    *lexer = std::move(curLexer);
    return StructDeclSyntax(oAccessModifier, std::move(oStructName->text), std::move(*oTypeParams), std::move(baseTypes), std::move(elems));
}

optional<ClassMemberTypeDeclSyntax> ParseClassMemberTypeDecl(Lexer* lexer)
{
    if (auto oTypeDecl = ParseTypeDecl(lexer))
        return ClassMemberTypeDeclSyntax(std::move(*oTypeDecl));

    return nullopt;
}

optional<ClassMemberFuncDeclSyntax> ParseClassMemberFuncDecl(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    auto oAccessModifier = ParseAccessModifier(&curLexer);

    bool bStatic = Accept<StaticToken>(&curLexer).has_value();
    bool bSequence = Accept<SeqToken>(&curLexer).has_value();

    // ex) void
    auto oRetType = ParseTypeExp(&curLexer);
    if (!oRetType)
        return nullopt;

    // ex) F
    auto oFuncName = Accept<IdentifierToken>(&curLexer);
    if (!oFuncName)
        return nullopt;

    // ex) <T1, T2>
    auto oTypeParams = ParseTypeParams(&curLexer);
    if (!oTypeParams)
        return nullopt;

    // ex) (int i, int a)
    auto oParameters = ParseFuncDeclParams(&curLexer);
    if (!oParameters)
        return nullopt;

    // ex) { ... }
    auto oBody = ParseBody(&curLexer);
    if (!oBody)
        return nullopt;

    *lexer = std::move(curLexer);
    return ClassMemberFuncDeclSyntax(
        oAccessModifier,
        bStatic, bSequence,
        std::move(*oRetType),
        std::move(oFuncName->text),
        std::move(*oTypeParams),
        std::move(*oParameters),
        std::move(*oBody));
}

optional<ClassConstructorDeclSyntax> ParseClassConstructorDecl(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    auto oAccessModifier = ParseAccessModifier(&curLexer);

    // ex) F
    auto oName = Accept<IdentifierToken>(&curLexer);
    if (!oName)
        return nullopt;

    // ex) (int i, int a)
    auto oParameters = ParseFuncDeclParams(&curLexer);
    if (!oParameters)
        return nullopt;

    // : base()
    optional<vector<ArgumentSyntax>> oBaseArgs;
    if (Accept<ColonToken>(&curLexer))
    {
        auto oExpectedToBeBase = Accept<IdentifierToken>(&curLexer);
        if (!oExpectedToBeBase)
            return nullopt;

        // base가 아닌 identifier는 오면 안된다. 다음은 '{' 토큰이다
        if (oExpectedToBeBase->text != U"base")
            return nullopt;
            
        auto oArgs = ParseCallArgs(&curLexer);
        if (!oArgs)
            return nullopt;

        oBaseArgs = std::move(oArgs);
    }

    // ex) { ... }
    auto oBody = ParseBody(&curLexer);
    if (!oBody)
        return nullopt;

    *lexer = std::move(curLexer);
    return ClassConstructorDeclSyntax(oAccessModifier, std::move(oName->text), std::move(*oParameters), std::move(oBaseArgs), std::move(*oBody));    
}

optional<ClassMemberVarDeclSyntax> ParseClassMemberVarDecl(Lexer* lexer)
{
    Lexer curLexer = *lexer;
    auto oAccessModifier = ParseAccessModifier(&curLexer);

    // ex) int
    auto oVarType = ParseTypeExp(&curLexer);
    if (!oVarType)
        return nullopt;

    // ex) x, y, z
    vector<u32string> varNames;  

    auto oVarNameToken0 = Accept<IdentifierToken>(&curLexer);
    if (!oVarNameToken0)
        return nullopt;

    varNames.push_back(std::move(oVarNameToken0->text));

    while (Accept<CommaToken>(&curLexer))
    {
        auto oVarNameToken = Accept<IdentifierToken>(&curLexer);
        if (!oVarNameToken)
            return nullopt;

        varNames.push_back(std::move(oVarNameToken->text));
    }

    // ;
    if (!Accept<SemiColonToken>(&curLexer))
        return nullopt;    

    *lexer = std::move(curLexer);
    return ClassMemberVarDeclSyntax(oAccessModifier, std::move(*oVarType), std::move(varNames));
}

optional<ClassMemberDeclSyntax> ParseClassMemberDecl(Lexer* lexer)
{
    if (auto oMemberDecl = ParseClassMemberTypeDecl(lexer))
        return oMemberDecl;

    if (auto oMemberDecl = ParseClassMemberFuncDecl(lexer))
        return oMemberDecl;

    if (auto oMemberDecl = ParseClassConstructorDecl(lexer))
        return oMemberDecl;

    if (auto oMemberDecl = ParseClassMemberVarDecl(lexer))
        return oMemberDecl;

    return nullopt;
}

optional<ClassDeclSyntax> ParseClassDecl(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    // AccessModifier, 텍스트에는 없을 수 있다
    auto oAccessModifier = ParseAccessModifier(&curLexer);

    // class
    if (!Accept<ClassToken>(&curLexer))
        return nullopt;

    // C
    auto oClassName = Accept<IdentifierToken>(&curLexer);
    if (!oClassName)
        return nullopt;

    // <T1, T2>
    auto oTypeParams = ParseTypeParams(&curLexer);
    if (!oTypeParams)
        return nullopt;

    // 상속 부분 : B, I, ...
    vector<TypeExpSyntax> baseTypes;
    if (Accept<ColonToken>(&curLexer))
    {
        auto oBaseType0 = ParseTypeExp(&curLexer);
        if (!oBaseType0)
            return nullopt;

        baseTypes.push_back(std::move(*oBaseType0));

        while (Accept<CommaToken>(&curLexer))
        {
            auto oBaseType = ParseTypeExp(&curLexer);
            if (!oBaseType)
                return nullopt;

            baseTypes.push_back(std::move(*oBaseType));
        }
    }

    vector<ClassMemberDeclSyntax> members;

    // {
    if (!Accept<LBraceToken>(&curLexer))
        return nullopt;

    // } 나올때까지
    while (!Accept<RBraceToken>(&curLexer))
    {
        auto oElem = ParseClassMemberDecl(&curLexer);
        if (!oElem)
            return nullopt;

        members.push_back(std::move(*oElem));
    }

    *lexer = std::move(curLexer);
    return ClassDeclSyntax(
        oAccessModifier, 
        std::move(oClassName->text), 
        std::move(*oTypeParams), 
        std::move(baseTypes), 
        std::move(members)
    );
}

optional<NamespaceDeclSyntaxElement> ParseNamespaceElement(Lexer* lexer)
{
    if (auto oDecl = ParseNamespaceDecl(lexer))
        return NamespaceDeclNamespaceDeclSyntaxElement(*oDecl);
    
    if (auto oDecl = ParseTypeDecl(lexer))
        return TypeDeclNamespaceDeclSyntaxElement(*oDecl);

    if (auto oDecl = ParseGlobalFuncDecl(lexer))
        return GlobalFuncDeclNamespaceDeclSyntaxElement(*oDecl);

    return nullopt;
}

optional<NamespaceDeclSyntax> ParseNamespaceDecl(Lexer* lexer)
{
    // <NAMESPACE> <NAME>(.<NAME> ...) <LBRACE>  ... <RBRACE>

    Lexer curLexer = *lexer;

    // namespace
    if (!Accept<NamespaceToken>(&curLexer))
        return nullopt;

    vector<u32string> nsNames;

    // ex) NS
    auto oNSName = Accept<IdentifierToken>(&curLexer);
    if (!oNSName)
        return nullopt;

    nsNames.push_back(std::move(oNSName->text));

    // . optional
    while (Accept<DotToken>(&curLexer))
    {
        // ex) NS
        oNSName = Accept<IdentifierToken>(&curLexer);
        if (!oNSName)
            return nullopt;

        nsNames.push_back(std::move(oNSName->text));
    }

    // {
    if (!Accept<LBraceToken>(&curLexer))
        return nullopt;

    vector<NamespaceDeclSyntaxElement> elems;
    // } 가 나올때까지
    while (!Accept<RBraceToken>(&curLexer))
    {
        auto oElem = ParseNamespaceElement(&curLexer);
        if (!oElem)
            return nullopt;

        elems.push_back(std::move(*oElem));
    }

    *lexer = std::move(curLexer);
    return NamespaceDeclSyntax(std::move(nsNames), std::move(elems));
}

optional<ScriptSyntaxElement> ParseScriptElement(Lexer* lexer)
{
    if (auto oDecl = ParseNamespaceDecl(lexer))
        return NamespaceDeclScriptSyntaxElement(std::move(*oDecl));

    if (auto oDecl = ParseTypeDecl(lexer))
        return TypeDeclScriptSyntaxElement(std::move(*oDecl));

    if (auto oDecl = ParseGlobalFuncDecl(lexer))
        return GlobalFuncDeclScriptSyntaxElement(std::move(*oDecl));

    return nullopt;
}

optional<ScriptSyntax> ParseScript(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    vector<ScriptSyntaxElement> elems;
    while (!Accept<EndOfFileToken>(&curLexer))
    {
        auto oScriptElem = ParseScriptElement(&curLexer);

        if (!oScriptElem)
            return nullopt;

        elems.push_back(std::move(*oScriptElem));
    }

    return ScriptSyntax(std::move(elems));
}

}