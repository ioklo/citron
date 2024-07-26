#include "pch.h"
#include "ScriptParser.h"

#include <optional>

#include <Syntax/Syntax.h>

#include "Lexer.h"
#include "ExpParser.h"
#include "StmtParser.h"
#include "TypeExpParser.h"

#include "ParserMisc.h"

using namespace std;

namespace Citron {

unique_ptr<SEnumDecl> ParseEnumDecl(Lexer* lexer);
unique_ptr<SStructDecl> ParseStructDecl(Lexer* lexer);
unique_ptr<SClassDecl> ParseClassDecl(Lexer* lexer);
optional<SAccessModifier> ParseAccessModifier(Lexer* lexer);
unique_ptr<SNamespaceDecl> ParseNamespaceDecl(Lexer* lexer);

// int t
// ref int t
// params T t
optional<SFuncParam> ParseFuncDeclParam(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    auto oOutAndParams = AcceptParseOutAndParams(&curLexer);
    if (!oOutAndParams)
        return nullopt;

    auto typeExp = ParseTypeExp(&curLexer);
    if (!typeExp)
        return nullopt;

    auto oName = Accept<IdentifierToken>(&curLexer);
    if (!oName)
        return nullopt;

    *lexer = std::move(curLexer);
    return SFuncParam(oOutAndParams->bOut, oOutAndParams->bParams, std::move(typeExp), std::move(oName->text));
}

optional<vector<SFuncParam>> ParseFuncDeclParams(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    if (!Accept<LParenToken>(&curLexer))
        return nullopt;

    vector<SFuncParam> params;
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

unique_ptr<SGlobalFuncDecl> ParseGlobalFuncDecl(Lexer* lexer)
{
    // <SEQ> <RetTypeName> <FuncName> <LPAREN> <ARGS> <RPAREN>
    // LBRACE>
    // [Stmt]
    // <RBRACE>   

    Lexer curLexer = *lexer;

    // seq
    auto bSequence = Accept<SeqToken>(&curLexer).has_value();

    auto retType = ParseTypeExp(&curLexer);
    if (!retType)
        return nullptr;

    auto oFuncName = Accept<IdentifierToken>(&curLexer);
    if (!oFuncName)
        return nullptr;

    auto oParameters = ParseFuncDeclParams(&curLexer);
    if (!oParameters)
        return nullptr;

    auto oBody = ParseBody(&curLexer);
    if (!oBody)
        return nullptr;

    *lexer = std::move(curLexer);

    return make_unique<SGlobalFuncDecl>(
        nullopt, // TODO: [7] 일단 null
        bSequence,
        std::move(retType),
        std::move(oFuncName->text),
        std::vector<STypeParam>{},
        std::move(*oParameters),
        std::move(*oBody)
    );
}

// <T1, T2, ...>
optional<vector<STypeParam>> ParseTypeParams(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    // typeParams
    vector<STypeParam> typeParams;
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

            typeParams.push_back(STypeParam{ oTypeParam->text });
        }
    }

    *lexer = std::move(curLexer);
    return typeParams;
}

template<typename TMemberDeclSyntax>
unique_ptr<TMemberDeclSyntax> ParseTypeDecl(Lexer* lexer)
{
    if (auto enumDecl = ParseEnumDecl(lexer))
        return enumDecl;

    if (auto structDecl = ParseStructDecl(lexer))
        return structDecl;

    if (auto classDecl = ParseClassDecl(lexer))
        return classDecl;

    return nullptr;
}

unique_ptr<SEnumDecl> ParseEnumDecl(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    // public enum E<T1, T2> { a , b () } 
    auto oAccessModifier = ParseAccessModifier(&curLexer);

    if (!Accept<EnumToken>(&curLexer))
        return nullptr;
    
    auto oEnumName = Accept<IdentifierToken>(&curLexer);
    if (!oEnumName)
        return nullptr;

    auto oTypeParams = ParseTypeParams(&curLexer);
    if (!oTypeParams)
        return nullptr;

    if (!Accept<LBraceToken>(&curLexer))
        return nullptr;

    vector<SEnumElemDecl> elems;
    while (!Accept<RBraceToken>(&curLexer))
    {
        if (!elems.empty())
            if (!Accept<CommaToken>(&curLexer))
                return nullptr;

        auto oElemName = Accept<IdentifierToken>(&curLexer);
        if (!oElemName)
            return nullptr;

        vector<SEnumElemMemberVarDecl> params;
        
        if (Accept<LParenToken>(&curLexer))
        {
            while (!Accept<RParenToken>(&curLexer))
            {
                if (!params.empty())
                    if (!Accept<CommaToken>(&curLexer))
                        return nullptr;
                
                auto typeExp = ParseTypeExp(&curLexer);
                if (!typeExp)
                    return nullptr;

                auto oParamName = Accept<IdentifierToken>(&curLexer);
                if (!oParamName)
                    return nullptr;

                params.push_back(SEnumElemMemberVarDecl(std::move(typeExp), std::move(oParamName->text)));
            }
        }

        elems.push_back(SEnumElemDecl(std::move(oElemName->text), std::move(params)));
    }

    *lexer = std::move(curLexer);
    return make_unique<SEnumDecl>(oAccessModifier, std::move(oEnumName->text), std::move(*oTypeParams), std::move(elems));    
}

optional<SAccessModifier> ParseAccessModifier(Lexer* lexer)
{
    if (Accept<ProtectedToken>(lexer))
        return SAccessModifier::Protected;

    if (Accept<PrivateToken>(lexer))
        return SAccessModifier::Private;

    if (Accept<PublicToken>(lexer))
        return SAccessModifier::Public;

    return nullopt;
}

unique_ptr<SStructMemberVarDecl> ParseStructMemberVarDecl(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    auto oAccessModifier = ParseAccessModifier(&curLexer);

    // ex) int
    auto varType = ParseTypeExp(&curLexer);
    if (!varType)
        return nullptr;

    // ex) x, y, z
    vector<string> varNames;

    auto oVarNameToken0 = Accept<IdentifierToken>(&curLexer);
    if (!oVarNameToken0)
        return nullptr;

    varNames.push_back(std::move(oVarNameToken0->text));

    while (Accept<CommaToken>(&curLexer))
    {
        auto oVarNameToken = Accept<IdentifierToken>(&curLexer);
        if (!oVarNameToken)
            return nullptr;

        varNames.push_back(std::move(oVarNameToken->text));
    }

    // ;
    if (!Accept<SemiColonToken>(&curLexer))
        return nullptr;

    *lexer = std::move(curLexer);

    return make_unique<SStructMemberVarDecl>(oAccessModifier, std::move(varType), std::move(varNames));
}

unique_ptr<SStructMemberFuncDecl> ParseStructMemberFuncDecl(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    auto oAccessModifier = ParseAccessModifier(&curLexer);

    bool bStatic = Accept<StaticToken>(&curLexer).has_value();
    bool bSequence = Accept<SeqToken>(&curLexer).has_value();

    // ex) void
    auto retType = ParseTypeExp(&curLexer);
    if (!retType)
        return nullptr;

    // ex) F
    auto oFuncName = Accept<IdentifierToken>(&curLexer);
    if (!oFuncName)
        return nullptr;

    // ex) <T1, T2>
    auto oTypeParams = ParseTypeParams(&curLexer);
    if (!oTypeParams)
        return nullptr;

    // ex) (int i, int a)
    auto oParameters = ParseFuncDeclParams(&curLexer);
    if (!oParameters)
        return nullptr;

    // ex) { ... }
    auto oBody = ParseBody(&curLexer);
    if (!oBody)
        return nullptr;

    *lexer = std::move(curLexer);
    return make_unique<SStructMemberFuncDecl>(
        oAccessModifier, bStatic, bSequence, std::move(retType), std::move(oFuncName->text), std::move(*oTypeParams), std::move(*oParameters), std::move(*oBody)
    );
}

unique_ptr<SStructConstructorDecl> ParseStructConstructorDecl(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    auto oAccessModifier = ParseAccessModifier(&curLexer);

    // ex) F
    auto oName = Accept<IdentifierToken>(&curLexer);
    if (!oName)
        return nullptr;

    // ex) (int i, int a)
    auto oParameters = ParseFuncDeclParams(&curLexer);
    if (!oParameters)
        return nullptr;

    // ex) { ... }
    auto oBody = ParseBody(&curLexer);
    if (!oBody)
        return nullptr;

    *lexer = std::move(curLexer);
    return make_unique<SStructConstructorDecl>(oAccessModifier, std::move(oName->text), std::move(*oParameters), std::move(*oBody));
}

unique_ptr<SStructMemberDecl> ParseStructMemberDecl(Lexer* lexer)
{
    if (auto memberDecl = ParseTypeDecl<SStructMemberDecl>(lexer))
        return memberDecl;

    if (auto memberDecl = ParseStructMemberFuncDecl(lexer))
        return memberDecl;

    if (auto memberDecl = ParseStructConstructorDecl(lexer))
        return memberDecl;

    if (auto memberDecl = ParseStructMemberVarDecl(lexer))
        return memberDecl;

    return nullptr;
}

unique_ptr<SStructDecl> ParseStructDecl(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    // AccessModifier, 텍스트에는 없을 수 있다
    auto oAccessModifier = ParseAccessModifier(&curLexer);

    if (!Accept<StructToken>(&curLexer))
        return nullptr;

    auto oStructName = Accept<IdentifierToken>(&curLexer);
    if (!oStructName)
        return nullptr;

    auto oTypeParams = ParseTypeParams(&curLexer);
    if (!oTypeParams)
        return nullptr;

    // 상속 부분 : B, I, ...
    vector<STypeExpPtr> baseTypes;
    if (Accept<ColonToken>(&curLexer))
    {
        auto typeExp = ParseTypeExp(&curLexer);
        if (!typeExp)
            return nullptr;

        baseTypes.push_back(std::move(typeExp));

        while (Accept<CommaToken>(&curLexer))
        {
            auto baseType = ParseTypeExp(&curLexer);
            if (!baseType)
                return nullptr;

            baseTypes.push_back(std::move(baseType));
        }
    }

    vector<SStructMemberDeclPtr> elems;

    // {
    if (!Accept<LBraceToken>(&curLexer))
        return nullptr;

    // } 나올때까지
    while (!Accept<RBraceToken>(&curLexer))
    {
        auto elem = ParseStructMemberDecl(&curLexer);
        if (!elem)
            return nullptr;

        elems.push_back(std::move(elem));
    }
    
    *lexer = std::move(curLexer);
    return make_unique<SStructDecl>(oAccessModifier, std::move(oStructName->text), std::move(*oTypeParams), std::move(baseTypes), std::move(elems));
}

unique_ptr<SClassMemberFuncDecl> ParseClassMemberFuncDecl(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    auto oAccessModifier = ParseAccessModifier(&curLexer);

    bool bStatic = Accept<StaticToken>(&curLexer).has_value();
    bool bSequence = Accept<SeqToken>(&curLexer).has_value();

    // ex) void
    auto retType = ParseTypeExp(&curLexer);
    if (!retType)
        return nullptr;

    // ex) F
    auto oFuncName = Accept<IdentifierToken>(&curLexer);
    if (!oFuncName)
        return nullptr;

    // ex) <T1, T2>
    auto oTypeParams = ParseTypeParams(&curLexer);
    if (!oTypeParams)
        return nullptr;

    // ex) (int i, int a)
    auto oParameters = ParseFuncDeclParams(&curLexer);
    if (!oParameters)
        return nullptr;

    // ex) { ... }
    auto oBody = ParseBody(&curLexer);
    if (!oBody)
        return nullptr;

    *lexer = std::move(curLexer);
    return make_unique<SClassMemberFuncDecl>(
        oAccessModifier,
        bStatic, bSequence,
        std::move(retType),
        std::move(oFuncName->text),
        std::move(*oTypeParams),
        std::move(*oParameters),
        std::move(*oBody));
}

unique_ptr<SClassConstructorDecl> ParseClassConstructorDecl(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    auto oAccessModifier = ParseAccessModifier(&curLexer);

    // ex) F
    auto oName = Accept<IdentifierToken>(&curLexer);
    if (!oName)
        return nullptr;

    // ex) (int i, int a)
    auto oParameters = ParseFuncDeclParams(&curLexer);
    if (!oParameters)
        return nullptr;

    // : base()
    optional<vector<SArgument>> oBaseArgs;
    if (Accept<ColonToken>(&curLexer))
    {
        auto oExpectedToBeBase = Accept<IdentifierToken>(&curLexer);
        if (!oExpectedToBeBase)
            return nullptr;

        // base가 아닌 identifier는 오면 안된다. 다음은 '{' 토큰이다
        if (oExpectedToBeBase->text != "base")
            return nullptr;
            
        auto oArgs = ParseCallArgs(&curLexer);
        if (!oArgs)
            return nullptr;

        oBaseArgs = std::move(oArgs);
    }

    // ex) { ... }
    auto oBody = ParseBody(&curLexer);
    if (!oBody)
        return nullptr;

    *lexer = std::move(curLexer);
    return make_unique<SClassConstructorDecl>(oAccessModifier, std::move(oName->text), std::move(*oParameters), std::move(oBaseArgs), std::move(*oBody));    
}

unique_ptr<SClassMemberVarDecl> ParseClassMemberVarDecl(Lexer* lexer)
{
    Lexer curLexer = *lexer;
    auto oAccessModifier = ParseAccessModifier(&curLexer);

    // ex) int
    auto varType = ParseTypeExp(&curLexer);
    if (!varType)
        return nullptr;

    // ex) x, y, z
    vector<string> varNames;  

    auto oVarNameToken0 = Accept<IdentifierToken>(&curLexer);
    if (!oVarNameToken0)
        return nullptr;

    varNames.push_back(std::move(oVarNameToken0->text));

    while (Accept<CommaToken>(&curLexer))
    {
        auto oVarNameToken = Accept<IdentifierToken>(&curLexer);
        if (!oVarNameToken)
            return nullptr;

        varNames.push_back(std::move(oVarNameToken->text));
    }

    // ;
    if (!Accept<SemiColonToken>(&curLexer))
        return nullptr;

    *lexer = std::move(curLexer);
    return make_unique<SClassMemberVarDecl>(oAccessModifier, std::move(varType), std::move(varNames));
}

unique_ptr<SClassMemberDecl> ParseClassMemberDecl(Lexer* lexer)
{
    if (auto memberDecl = ParseTypeDecl<SClassMemberDecl>(lexer))
        return memberDecl;

    if (auto memberDecl = ParseClassMemberFuncDecl(lexer))
        return memberDecl;

    if (auto memberDecl = ParseClassConstructorDecl(lexer))
        return memberDecl;

    if (auto memberDecl = ParseClassMemberVarDecl(lexer))
        return memberDecl;

    return nullptr;
}

unique_ptr<SClassDecl> ParseClassDecl(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    // AccessModifier, 텍스트에는 없을 수 있다
    auto oAccessModifier = ParseAccessModifier(&curLexer);

    // class
    if (!Accept<ClassToken>(&curLexer))
        return nullptr;

    // C
    auto oClassName = Accept<IdentifierToken>(&curLexer);
    if (!oClassName)
        return nullptr;

    // <T1, T2>
    auto oTypeParams = ParseTypeParams(&curLexer);
    if (!oTypeParams)
        return nullptr;

    // 상속 부분 : B, I, ...
    vector<STypeExpPtr> baseTypes;
    if (Accept<ColonToken>(&curLexer))
    {
        auto baseType0 = ParseTypeExp(&curLexer);
        if (!baseType0)
            return nullptr;

        baseTypes.push_back(std::move(baseType0));

        while (Accept<CommaToken>(&curLexer))
        {
            auto baseType = ParseTypeExp(&curLexer);
            if (!baseType)
                return nullptr;

            baseTypes.push_back(std::move(baseType));
        }
    }

    vector<SClassMemberDeclPtr> members;

    // {
    if (!Accept<LBraceToken>(&curLexer))
        return nullptr;

    // } 나올때까지
    while (!Accept<RBraceToken>(&curLexer))
    {
        auto elem = ParseClassMemberDecl(&curLexer);
        if (!elem)
            return nullptr;

        members.push_back(std::move(elem));
    }

    *lexer = std::move(curLexer);
    return make_unique<SClassDecl>(
        oAccessModifier, 
        std::move(oClassName->text), 
        std::move(*oTypeParams), 
        std::move(baseTypes), 
        std::move(members)
    );
}

SNamespaceDeclElementPtr ParseNamespaceElement(Lexer* lexer)
{
    if (auto decl = ParseNamespaceDecl(lexer))
        return decl;
    
    if (auto decl = ParseTypeDecl<SNamespaceDeclElement>(lexer))
        return decl;

    if (auto decl = ParseGlobalFuncDecl(lexer))
        return decl;

    return nullptr;
}

unique_ptr<SNamespaceDecl> ParseNamespaceDecl(Lexer* lexer)
{
    // <NAMESPACE> <NAME>(.<NAME> ...) <LBRACE>  ... <RBRACE>

    Lexer curLexer = *lexer;

    // namespace
    if (!Accept<NamespaceToken>(&curLexer))
        return nullptr;

    vector<string> nsNames;

    // ex) NS
    auto oNSName = Accept<IdentifierToken>(&curLexer);
    if (!oNSName)
        return nullptr;

    nsNames.push_back(std::move(oNSName->text));

    // . optional
    while (Accept<DotToken>(&curLexer))
    {
        // ex) NS
        oNSName = Accept<IdentifierToken>(&curLexer);
        if (!oNSName)
            return nullptr;

        nsNames.push_back(std::move(oNSName->text));
    }

    // {
    if (!Accept<LBraceToken>(&curLexer))
        return nullptr;

    vector<SNamespaceDeclElementPtr> elems;
    // } 가 나올때까지
    while (!Accept<RBraceToken>(&curLexer))
    {
        auto elem = ParseNamespaceElement(&curLexer);
        if (!elem)
            return nullptr;

        elems.push_back(std::move(elem));
    }

    *lexer = std::move(curLexer);
    return make_unique<SNamespaceDecl>(std::move(nsNames), std::move(elems));
}

SScriptElementPtr ParseScriptElement(Lexer* lexer)
{
    if (auto decl = ParseNamespaceDecl(lexer))
        return decl;

    if (auto decl = ParseTypeDecl<SScriptElement>(lexer))
        return decl;

    if (auto decl = ParseGlobalFuncDecl(lexer))
        return decl;

    return nullptr;
}

optional<SScript> ParseScript(Lexer* lexer)
{
    Lexer curLexer = *lexer;

    vector<SScriptElementPtr> elems;
    while (!Accept<EndOfFileToken>(&curLexer))
    {
        auto scriptElem = ParseScriptElement(&curLexer);

        if (!scriptElem)
            return nullopt;

        elems.push_back(std::move(scriptElem));
    }

    return SScript(std::move(elems));
}

}