#include "ScriptParser.h"

#include <optional>
#include <memory>

#include "Infra/Ptr.h"
#include "Syntax/Syntax.h"
#include "Syntax/Tokens.h"

#include "Lexer.h"
#include "ExpParser.h"
#include "StmtParser.h"
#include "TypeExpParser.h"
#include "ParserMisc.h"

using namespace std;

namespace Citron {

SEnumDecl* ParseEnumDecl(Lexer* lexer, SFactory& factory);
SStructDecl* ParseStructDecl(Lexer* lexer, SFactory& factory);
SClassDecl* ParseClassDecl(Lexer* lexer, SFactory& factory);
optional<SAccessModifier> ParseAccessModifier(Lexer* lexer);
SNamespaceDecl* ParseNamespaceDecl(Lexer* lexer, SFactory& factory);

// int t
// ref int t
// params T t
optional<SFuncParam> ParseFuncDeclParam(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    auto oOutAndParams = AcceptParseOutAndParams(&curLexer);
    if (!oOutAndParams)
        return nullopt;

    auto* typeExp = ParseTypeExp(&curLexer, factory);
    if (!typeExp)
        return nullopt;

    auto oName = Accept<IdentifierToken>(&curLexer);
    if (!oName)
        return nullopt;

    *lexer = move(curLexer);
    return SFuncParam(oOutAndParams->bOut, oOutAndParams->bParams, typeExp, move(oName->text));
}

optional<vector<SFuncParam>> ParseFuncDeclParams(Lexer* lexer, SFactory& factory)
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

        auto oParam = ParseFuncDeclParam(&curLexer, factory);
        if (!oParam)
            return nullopt;

        params.push_back(move(*oParam));
    }

    *lexer = move(curLexer);
    return params;
}

SGlobalFuncDecl* ParseGlobalFuncDecl(Lexer* lexer, SFactory& factory)
{
    // <SEQ> <RetTypeName> <FuncName> <LPAREN> <ARGS> <RPAREN>
    // LBRACE>
    // [Stmt]
    // <RBRACE>   

    Lexer curLexer = *lexer;

    // seq
    auto bSequence = Accept<SeqToken>(&curLexer).has_value();

    auto* retType = ParseTypeExp(&curLexer, factory);
    if (!retType)
        return nullptr;

    auto oFuncName = Accept<IdentifierToken>(&curLexer);
    if (!oFuncName)
        return nullptr;

    auto oParameters = ParseFuncDeclParams(&curLexer, factory);
    if (!oParameters)
        return nullptr;

    auto oBody = ParseBody(&curLexer, factory);
    if (!oBody)
        return nullptr;

    *lexer = move(curLexer);

    return factory.MakeSGlobalFuncDecl(
        nullopt, // TODO: [7] 일단 null
        bSequence,
        retType,
        move(oFuncName->text),
        std::vector<STypeParam>{},
        move(*oParameters),
        move(*oBody)
    );
}

// <T1, T2, ...>
optional<vector<STypeParam>> ParseTypeParams(Lexer* lexer, SFactory& factory)
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

    *lexer = move(curLexer);
    return typeParams;
}

template<typename TMemberDeclSyntax>
TMemberDeclSyntax* ParseTypeDecl(Lexer* lexer, SFactory& factory)
{
    if (auto* enumDecl = ParseEnumDecl(lexer, factory))
        return enumDecl;

    if (auto* structDecl = ParseStructDecl(lexer, factory))
        return structDecl;

    if (auto* classDecl = ParseClassDecl(lexer, factory))
        return classDecl;

    return nullptr;
}

SEnumDecl* ParseEnumDecl(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    // public enum E<T1, T2> { a , b () } 
    auto oAccessModifier = ParseAccessModifier(&curLexer);

    if (!Accept<EnumToken>(&curLexer))
        return nullptr;
    
    auto oEnumName = Accept<IdentifierToken>(&curLexer);
    if (!oEnumName)
        return nullptr;

    auto oTypeParams = ParseTypeParams(&curLexer, factory);
    if (!oTypeParams)
        return nullptr;

    if (!Accept<LBraceToken>(&curLexer))
        return nullptr;

    vector<SEnumElemDecl*> elems;
    while (!Accept<RBraceToken>(&curLexer))
    {
        if (!elems.empty())
            if (!Accept<CommaToken>(&curLexer))
                return nullptr;

        auto oElemName = Accept<IdentifierToken>(&curLexer);
        if (!oElemName)
            return nullptr;

        vector<SEnumElemVarDecl*> params;
        
        if (Accept<LParenToken>(&curLexer))
        {
            while (!Accept<RParenToken>(&curLexer))
            {
                if (!params.empty())
                    if (!Accept<CommaToken>(&curLexer))
                        return nullptr;
                
                auto* typeExp = ParseTypeExp(&curLexer, factory);
                if (!typeExp)
                    return nullptr;

                auto oParamName = Accept<IdentifierToken>(&curLexer);
                if (!oParamName)
                    return nullptr;

                params.push_back(factory.MakeSEnumElemVarDecl(typeExp, move(oParamName->text)));
            }
        }

        elems.push_back(factory.MakeSEnumElemDecl(move(oElemName->text), move(params)));
    }

    *lexer = move(curLexer);
    return factory.MakeSEnumDecl(oAccessModifier, move(oEnumName->text), move(*oTypeParams), move(elems));
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

SStructVarDecl* ParseStructVarDecl(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    auto oAccessModifier = ParseAccessModifier(&curLexer);

    // ex) int
    auto* varType = ParseTypeExp(&curLexer, factory);
    if (!varType)
        return nullptr;

    // ex) x, y, z
    vector<string> varNames;

    auto oVarNameToken0 = Accept<IdentifierToken>(&curLexer);
    if (!oVarNameToken0)
        return nullptr;

    varNames.push_back(move(oVarNameToken0->text));

    while (Accept<CommaToken>(&curLexer))
    {
        auto oVarNameToken = Accept<IdentifierToken>(&curLexer);
        if (!oVarNameToken)
            return nullptr;

        varNames.push_back(move(oVarNameToken->text));
    }

    // ;
    if (!Accept<SemiColonToken>(&curLexer))
        return nullptr;

    *lexer = move(curLexer);

    return factory.MakeSStructVarDecl(oAccessModifier, varType, move(varNames));
}

SStructFuncDecl* ParseStructFuncDecl(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    auto oAccessModifier = ParseAccessModifier(&curLexer);

    bool bStatic = Accept<StaticToken>(&curLexer).has_value();
    bool bSequence = Accept<SeqToken>(&curLexer).has_value();

    // ex) void
    auto* retType = ParseTypeExp(&curLexer, factory);
    if (!retType)
        return nullptr;

    // ex) F
    auto oFuncName = Accept<IdentifierToken>(&curLexer);
    if (!oFuncName)
        return nullptr;

    // ex) <T1, T2>
    auto oTypeParams = ParseTypeParams(&curLexer, factory);
    if (!oTypeParams)
        return nullptr;

    // ex) (int i, int a)
    auto oParameters = ParseFuncDeclParams(&curLexer, factory);
    if (!oParameters)
        return nullptr;

    // ex) { ... }
    auto oBody = ParseBody(&curLexer, factory);
    if (!oBody)
        return nullptr;

    *lexer = move(curLexer);
    return factory.MakeSStructFuncDecl(
        oAccessModifier, bStatic, bSequence, retType, move(oFuncName->text), move(*oTypeParams), move(*oParameters), move(*oBody)
    );
}

SStructCtorDecl* ParseStructCtorDecl(const string& structName, Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    auto oAccessModifier = ParseAccessModifier(&curLexer);

    // ex) F
    auto oName = Accept<IdentifierToken>(&curLexer);
    if (!oName)
        return nullptr;

    // 이름이 같아야 ctor이다
    if (oName->text != structName)
        return nullptr;

    // ex) (int i, int a)
    auto oParameters = ParseFuncDeclParams(&curLexer, factory);
    if (!oParameters)
        return nullptr;

    // ex) { ... }
    auto oBody = ParseBody(&curLexer, factory);
    if (!oBody)
        return nullptr;

    *lexer = move(curLexer);
    return factory.MakeSStructCtorDecl(oAccessModifier, move(*oParameters), move(*oBody));
}

SStructMemberDecl* ParseStructMemberDecl(const string& structName, Lexer* lexer, SFactory& factory)
{
    if (auto* memberDecl = ParseTypeDecl<SStructMemberDecl>(lexer, factory))
        return memberDecl;

    if (auto* memberDecl = ParseStructFuncDecl(lexer, factory))
        return memberDecl;

    if (auto* memberDecl = ParseStructCtorDecl(structName, lexer, factory))
        return memberDecl;

    if (auto* memberDecl = ParseStructVarDecl(lexer, factory))
        return memberDecl;

    return nullptr;
}

SStructDecl* ParseStructDecl(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    // AccessModifier, 텍스트에는 없을 수 있다
    auto oAccessModifier = ParseAccessModifier(&curLexer);

    if (!Accept<StructToken>(&curLexer))
        return nullptr;

    auto oStructName = Accept<IdentifierToken>(&curLexer);
    if (!oStructName)
        return nullptr;

    auto oTypeParams = ParseTypeParams(&curLexer, factory);
    if (!oTypeParams)
        return nullptr;

    // 상속 부분 : B, I, ...
    vector<STypeExp*> baseTypes;
    if (Accept<ColonToken>(&curLexer))
    {
        auto* typeExp = ParseTypeExp(&curLexer, factory);
        if (!typeExp)
            return nullptr;

        baseTypes.push_back(typeExp);

        while (Accept<CommaToken>(&curLexer))
        {
            auto* baseType = ParseTypeExp(&curLexer, factory);
            if (!baseType)
                return nullptr;

            baseTypes.push_back(baseType);
        }
    }

    vector<SStructMemberDecl*> elems;

    // {
    if (!Accept<LBraceToken>(&curLexer))
        return nullptr;

    // } 나올때까지
    while (!Accept<RBraceToken>(&curLexer))
    {
        auto* elem = ParseStructMemberDecl(oStructName->text, &curLexer, factory);
        if (!elem)
            return nullptr;

        elems.push_back(elem);
    }
    
    *lexer = move(curLexer);
    return factory.MakeSStructDecl(oAccessModifier, move(oStructName->text), move(*oTypeParams), move(baseTypes), move(elems));
}

SClassFuncDecl* ParseClassFuncDecl(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    auto oAccessModifier = ParseAccessModifier(&curLexer);

    bool bStatic = Accept<StaticToken>(&curLexer).has_value();
    bool bSequence = Accept<SeqToken>(&curLexer).has_value();

    // ex) void
    auto* retType = ParseTypeExp(&curLexer, factory);
    if (!retType)
        return nullptr;

    // ex) F
    auto oFuncName = Accept<IdentifierToken>(&curLexer);
    if (!oFuncName)
        return nullptr;

    // ex) <T1, T2>
    auto oTypeParams = ParseTypeParams(&curLexer, factory);
    if (!oTypeParams)
        return nullptr;

    // ex) (int i, int a)
    auto oParameters = ParseFuncDeclParams(&curLexer, factory);
    if (!oParameters)
        return nullptr;

    // ex) { ... }
    auto oBody = ParseBody(&curLexer, factory);
    if (!oBody)
        return nullptr;

    *lexer = move(curLexer);
    return factory.MakeSClassFuncDecl(
        oAccessModifier,
        bStatic, bSequence,
        retType,
        move(oFuncName->text),
        move(*oTypeParams),
        move(*oParameters),
        move(*oBody));
}

SClassCtorDecl* ParseClassCtorDecl(const string& className, Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    auto oAccessModifier = ParseAccessModifier(&curLexer);

    // ex) F
    auto oName = Accept<IdentifierToken>(&curLexer);
    if (!oName)
        return nullptr;

    // 이름이 같아야 ctor다
    if (oName->text != className)
        return nullptr;

    // ex) (int i, int a)
    auto oParameters = ParseFuncDeclParams(&curLexer, factory);
    if (!oParameters)
        return nullptr;

    // : base()
    SArguments* baseArgs;
    if (Accept<ColonToken>(&curLexer))
    {
        auto oExpectedToBeBase = Accept<IdentifierToken>(&curLexer);
        if (!oExpectedToBeBase)
            return nullptr;

        // base가 아닌 identifier는 오면 안된다. 다음은 '{' 토큰이다
        if (oExpectedToBeBase->text != "base")
            return nullptr;
            
        baseArgs = ParseCallArgs(&curLexer, factory);
        if (!baseArgs)
            return nullptr;
    }

    // ex) { ... }
    auto oBody = ParseBody(&curLexer, factory);
    if (!oBody)
        return nullptr;

    *lexer = move(curLexer);
    return factory.MakeSClassCtorDecl(oAccessModifier, move(*oParameters), move(baseArgs), move(*oBody));
}

SClassVarDecl* ParseClassVarDecl(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;
    auto oAccessModifier = ParseAccessModifier(&curLexer);

    // ex) int
    auto* varType = ParseTypeExp(&curLexer, factory);
    if (!varType)
        return nullptr;

    // ex) x, y, z
    vector<string> varNames;  

    auto oVarNameToken0 = Accept<IdentifierToken>(&curLexer);
    if (!oVarNameToken0)
        return nullptr;

    varNames.push_back(move(oVarNameToken0->text));

    while (Accept<CommaToken>(&curLexer))
    {
        auto oVarNameToken = Accept<IdentifierToken>(&curLexer);
        if (!oVarNameToken)
            return nullptr;

        varNames.push_back(move(oVarNameToken->text));
    }

    // ;
    if (!Accept<SemiColonToken>(&curLexer))
        return nullptr;

    *lexer = move(curLexer);
    return factory.MakeSClassVarDecl(oAccessModifier, varType, move(varNames));
}

SClassMemberDecl* ParseClassMemberDecl(string& className, Lexer* lexer, SFactory& factory)
{
    if (auto* memberDecl = ParseTypeDecl<SClassMemberDecl>(lexer, factory))
        return memberDecl;

    if (auto* memberDecl = ParseClassFuncDecl(lexer, factory))
        return memberDecl;

    if (auto* memberDecl = ParseClassCtorDecl(className, lexer, factory))
        return memberDecl;

    if (auto* memberDecl = ParseClassVarDecl(lexer, factory))
        return memberDecl;

    return nullptr;
}

SClassDecl* ParseClassDecl(Lexer* lexer, SFactory& factory)
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
    auto oTypeParams = ParseTypeParams(&curLexer, factory);
    if (!oTypeParams)
        return nullptr;

    // 상속 부분 : B, I, ...
    vector<STypeExp*> baseTypes;
    if (Accept<ColonToken>(&curLexer))
    {
        auto* baseType0 = ParseTypeExp(&curLexer, factory);
        if (!baseType0)
            return nullptr;

        baseTypes.push_back(baseType0);

        while (Accept<CommaToken>(&curLexer))
        {
            auto* baseType = ParseTypeExp(&curLexer, factory);
            if (!baseType)
                return nullptr;

            baseTypes.push_back(baseType);
        }
    }

    vector<SClassMemberDecl*> members;

    // {
    if (!Accept<LBraceToken>(&curLexer))
        return nullptr;

    // } 나올때까지
    while (!Accept<RBraceToken>(&curLexer))
    {
        auto* elem = ParseClassMemberDecl(oClassName->text, &curLexer, factory);
        if (!elem)
            return nullptr;

        members.push_back(elem);
    }

    *lexer = move(curLexer);
    return factory.MakeSClassDecl(
        oAccessModifier, 
        move(oClassName->text), 
        move(*oTypeParams), 
        move(baseTypes), 
        move(members)
    );
}

SNamespaceDeclElement* ParseNamespaceElement(Lexer* lexer, SFactory& factory)
{
    if (auto* decl = ParseNamespaceDecl(lexer, factory))
        return decl;
    
    if (auto* decl = ParseTypeDecl<SNamespaceDeclElement>(lexer, factory))
        return decl;

    if (auto* decl = ParseGlobalFuncDecl(lexer, factory))
        return decl;

    return nullptr;
}

SNamespaceDecl* ParseNamespaceDecl(Lexer* lexer, SFactory& factory)
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

    nsNames.push_back(move(oNSName->text));

    // . optional
    while (Accept<DotToken>(&curLexer))
    {
        // ex) NS
        oNSName = Accept<IdentifierToken>(&curLexer);
        if (!oNSName)
            return nullptr;

        nsNames.push_back(move(oNSName->text));
    }

    // {
    if (!Accept<LBraceToken>(&curLexer))
        return nullptr;

    vector<SNamespaceDeclElement*> elems;
    // } 가 나올때까지
    while (!Accept<RBraceToken>(&curLexer))
    {
        auto* elem = ParseNamespaceElement(&curLexer, factory);
        if (!elem)
            return nullptr;

        elems.push_back(elem);
    }

    *lexer = move(curLexer);
    return factory.MakeSNamespaceDecl(move(nsNames), move(elems));
}

SScriptElement* ParseScriptElement(Lexer* lexer, SFactory& factory)
{
    if (auto* decl = ParseNamespaceDecl(lexer, factory))
        return decl;

    if (auto* decl = ParseTypeDecl<SScriptElement>(lexer, factory))
        return decl;

    if (auto* decl = ParseGlobalFuncDecl(lexer, factory))
        return decl;

    return nullptr;
}

SScript* ParseScript(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    vector<SScriptElement*> elems;
    while (!Accept<EndOfFileToken>(&curLexer))
    {
        auto* scriptElem = ParseScriptElement(&curLexer, factory);

        if (!scriptElem)
            return nullptr;

        elems.push_back(scriptElem);
    }

    return factory.MakeSScript(move(elems));
}

}