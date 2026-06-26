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

#define RETURN_ACCEPT(expr) \
    do { \
        *lexer = move(curLexer); \
        return (expr); \
    } while(false)

using namespace std;

namespace Citron {

SEnumDecl* ParseEnumDecl(Lexer* lexer, SFactory& factory);
SStructDecl* ParseStructDecl(Lexer* lexer, SFactory& factory);
SClassDecl* ParseClassDecl(Lexer* lexer, SFactory& factory);
STraitDecl* ParseTraitDecl(Lexer* lexer, SFactory& factory);
SExtendDecl* ParseExtendDecl(Lexer* lexer, SFactory& factory);
optional<SAccessModifier> ParseAccessModifier(Lexer* lexer);
SNamespaceDecl* ParseNamespaceDecl(Lexer* lexer, SFactory& factory);

optional<SFuncReturn> ParseFuncReturn(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    if (Accept<SomeToken>(&curLexer))
    {
        auto* retType = ParseTypeExp(&curLexer, factory);
        if (!retType)
            return nullopt;

        RETURN_ACCEPT(SFuncReturn_Opaque{retType});
    }
    else
    {
        auto* retType = ParseTypeExp(&curLexer, factory);
        if (!retType)
            return nullopt;

        RETURN_ACCEPT(SFuncReturn_Normal{retType});
    }
}

// int t
// int& t
// [params] T t
optional<SFuncParam> ParseFuncDeclParam(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    auto o_paramModifier = ParseParamModifier(&curLexer);

    auto o_funcParamType = ParseFuncParamTypeExp(&curLexer, factory);
    if (!o_funcParamType)
        return nullopt;

    auto o_name = Accept<IdentifierToken>(&curLexer);
    if (!o_name)
        return nullopt;

    *lexer = move(curLexer);
    return SFuncParam(o_paramModifier, o_funcParamType->bRef, o_funcParamType->typeExp, move(o_name->text));
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

        auto o_param = ParseFuncDeclParam(&curLexer, factory);
        if (!o_param)
            return nullopt;

        params.push_back(move(*o_param));
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

    auto o_funcRet = ParseFuncReturn(&curLexer, factory);
    if (!o_funcRet) return nullptr;

    auto o_funcName = Accept<IdentifierToken>(&curLexer);
    if (!o_funcName) return nullptr;

    auto o_parameters = ParseFuncDeclParams(&curLexer, factory);
    if (!o_parameters) return nullptr;

    auto o_body = ParseBody(&curLexer, factory);
    if (!o_body) return nullptr;

    RETURN_ACCEPT(factory.Make<SGlobalFuncDecl>(
        nullopt, // TODO: [7] GlobalFuncDecl에 AccessModifier 추가
        bSequence,
        move(*o_funcRet),
        move(o_funcName->text),
        std::vector<STypeParam>{},
        move(*o_parameters),
        move(*o_body)
    ));
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
            auto o_typeParam = Accept<IdentifierToken>(&curLexer);
            if (!o_typeParam)
                return nullopt;

            typeParams.push_back(STypeParam{ o_typeParam->text });
        }
    }

    *lexer = move(curLexer);
    return typeParams;
}

template<typename TMemberDeclSyntax>
optional<TMemberDeclSyntax> ParseMemberDecl(Lexer* lexer, SFactory& factory)
{
    if (auto* enumDecl = ParseEnumDecl(lexer, factory))
        return enumDecl;

    if (auto* structDecl = ParseStructDecl(lexer, factory))
        return structDecl;

    if (auto* classDecl = ParseClassDecl(lexer, factory))
        return classDecl;

    if (auto* traitDecl = ParseTraitDecl(lexer, factory))
        return traitDecl;

    if (auto* extendDecl = ParseExtendDecl(lexer, factory))
        return extendDecl;

    return nullopt;
}

SEnumDecl* ParseEnumDecl(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    // public enum E<T1, T2> { a , b () } 
    auto o_accessModifier = ParseAccessModifier(&curLexer);

    if (!Accept<EnumToken>(&curLexer))
        return nullptr;
    
    auto o_enumName = Accept<IdentifierToken>(&curLexer);
    if (!o_enumName)
        return nullptr;

    auto o_typeParams = ParseTypeParams(&curLexer, factory);
    if (!o_typeParams)
        return nullptr;

    if (!Accept<LBraceToken>(&curLexer))
        return nullptr;

    vector<SEnumElemDecl*> elems;
    while (!Accept<RBraceToken>(&curLexer))
    {
        if (!elems.empty())
            if (!Accept<CommaToken>(&curLexer))
                return nullptr;

        auto o_elemName = Accept<IdentifierToken>(&curLexer);
        if (!o_elemName)
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

                auto o_paramName = Accept<IdentifierToken>(&curLexer);
                if (!o_paramName)
                    return nullptr;

                params.push_back(factory.MakeSEnumElemVarDecl(typeExp, move(o_paramName->text)));
            }
        }

        elems.push_back(factory.MakeSEnumElemDecl(move(o_elemName->text), move(params)));
    }

    *lexer = move(curLexer);
    return factory.MakeSEnumDecl(o_accessModifier, move(o_enumName->text), move(*o_typeParams), move(elems));
}

STraitFuncDecl* ParseTraitFuncDecl(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    bool bStatic = Accept<StaticToken>(&curLexer).has_value();

    // return type용 some
    auto o_funcRet = ParseFuncReturn(&curLexer, factory);
    if (!o_funcRet)
        return nullptr;
    
    // ex) F
    auto o_funcName = Accept<IdentifierToken>(&curLexer);
    if (!o_funcName)
        return nullptr;

    // ex) <T1, T2>
    auto o_typeParams = ParseTypeParams(&curLexer, factory);
    if (!o_typeParams)
        return nullptr;

    // ex) (int i, int a)
    auto o_parameters = ParseFuncDeclParams(&curLexer, factory);
    if (!o_parameters)
        return nullptr;

    if (!Accept<SemiColonToken>(&curLexer))
        return nullptr;
    
    RETURN_ACCEPT(factory.Make<STraitFuncDecl>(
        bStatic,
        move(*o_funcRet),
        move(o_funcName->text),
        move(*o_typeParams),
        move(*o_parameters)));
}

optional<STraitMemberDecl> ParseTraitMemberDecl(Lexer* lexer, SFactory& factory)
{
    if (auto* decl = ParseTraitFuncDecl(lexer, factory))
        return decl;

    return nullopt;
}

// trait TraitName<T, U> { ... }
STraitDecl* ParseTraitDecl(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    auto o_accessModifier = ParseAccessModifier(&curLexer);

    if (!Accept<TraitToken>(&curLexer))
        return nullptr;

    auto o_name = Accept<IdentifierToken>(&curLexer);
    if (!o_name)
        return nullptr;

    auto o_typeParams = ParseTypeParams(&curLexer, factory);
    if (!o_typeParams)
        return nullptr;

    if (!Accept<LBraceToken>(&curLexer))
        return nullptr;

    vector<STraitMemberDecl> elems;
    while (!Accept<RBraceToken>(&curLexer))
    {
        auto o_memberDecl = ParseTraitMemberDecl(&curLexer, factory);
        if (!o_memberDecl)
            return nullptr;

        elems.push_back(move(*o_memberDecl));
    }

    RETURN_ACCEPT(factory.Make<STraitDecl>(o_accessModifier, move(o_name->text), move(*o_typeParams), move(elems)));
}

SExtendFuncDecl* ParseExtendFuncDecl(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    bool bStatic = Accept<StaticToken>(&curLexer).has_value();

    // return type용 some
    auto o_funcRet = ParseFuncReturn(&curLexer, factory);
    if (!o_funcRet)
        return nullptr;

    // ex) F
    auto o_funcName = Accept<IdentifierToken>(&curLexer);
    if (!o_funcName)
        return nullptr;

    // ex) <T1, T2>
    auto o_typeParams = ParseTypeParams(&curLexer, factory);
    if (!o_typeParams)
        return nullptr;

    // ex) (int i, int a)
    auto o_parameters = ParseFuncDeclParams(&curLexer, factory);
    if (!o_parameters)
        return nullptr;

    // ex) { ... }
    auto o_body = ParseBody(&curLexer, factory);
    if (!o_body) return nullptr;

    RETURN_ACCEPT(factory.Make<SExtendFuncDecl>(
        bStatic,
        move(*o_funcRet),
        move(o_funcName->text),
        move(*o_typeParams),
        move(*o_parameters),
        move(*o_body)));
}

optional<SExtendMemberDecl> ParseExtendMemberDecl(Lexer* lexer, SFactory& factory)
{
    if (auto* decl = ParseExtendFuncDecl(lexer, factory))
        return decl;

    return nullopt;
}

// extend S : Trait { }
SExtendDecl* ParseExtendDecl(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    // access modifier
    auto o_accessModifier = ParseAccessModifier(&curLexer);

    // extend
    if (!Accept<ExtendToken>(&curLexer))
        return nullptr;

    // name
    auto o_name = Accept<IdentifierToken>(&curLexer);
    if (!o_name) return nullptr;

    // : 
    if (!Accept<ColonToken>(&curLexer)) 
        return nullptr;

    // trait<>
    auto* trait = ParseTypeExp(&curLexer, factory);
    if (!trait) return nullptr;

    // {
    if (!Accept<LBraceToken>(&curLexer))
        return nullptr;

    vector<SExtendMemberDecl> memberDecls;
    while (!Accept<RBraceToken>(&curLexer))
    {
        auto o_memberDecl = ParseExtendMemberDecl(&curLexer, factory);
        if (!o_memberDecl) return nullptr;

        memberDecls.push_back(move(*o_memberDecl));
    }

    RETURN_ACCEPT(factory.Make<SExtendDecl>(o_accessModifier, move(o_name->text), trait, move(memberDecls)));
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

    auto o_accessModifier = ParseAccessModifier(&curLexer);

    // ex) int
    auto* varType = ParseTypeExp(&curLexer, factory);
    if (!varType)
        return nullptr;

    // ex) x, y, z
    vector<string> varNames;

    auto o_varNameToken0 = Accept<IdentifierToken>(&curLexer);
    if (!o_varNameToken0)
        return nullptr;

    varNames.push_back(move(o_varNameToken0->text));

    while (Accept<CommaToken>(&curLexer))
    {
        auto o_varNameToken = Accept<IdentifierToken>(&curLexer);
        if (!o_varNameToken)
            return nullptr;

        varNames.push_back(move(o_varNameToken->text));
    }

    // ;
    if (!Accept<SemiColonToken>(&curLexer))
        return nullptr;

    *lexer = move(curLexer);

    return factory.MakeSStructVarDecl(o_accessModifier, varType, move(varNames));
}

SStructFuncDecl* ParseStructFuncDecl(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    auto o_accessModifier = ParseAccessModifier(&curLexer);

    bool bStatic = Accept<StaticToken>(&curLexer).has_value();
    bool bSequence = Accept<SeqToken>(&curLexer).has_value();

    // ex) void
    auto o_funcRet = ParseFuncReturn(&curLexer, factory);
    if (!o_funcRet) return nullptr;

    // ex) F
    auto o_funcName = Accept<IdentifierToken>(&curLexer);
    if (!o_funcName) return nullptr;

    // ex) <T1, T2>
    auto o_typeParams = ParseTypeParams(&curLexer, factory);
    if (!o_typeParams) return nullptr;

    // ex) (int i, int a)
    auto o_parameters = ParseFuncDeclParams(&curLexer, factory);
    if (!o_parameters) return nullptr;

    // ex) { ... }
    auto o_body = ParseBody(&curLexer, factory);
    if (!o_body) return nullptr;
    
    RETURN_ACCEPT(factory.Make<SStructFuncDecl>(
        o_accessModifier, bStatic, bSequence, move(*o_funcRet), move(o_funcName->text), move(*o_typeParams), move(*o_parameters), move(*o_body)
    ));
}

SStructCtorDecl* ParseStructCtorDecl(const string& structName, Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    auto o_accessModifier = ParseAccessModifier(&curLexer);

    // ex) F
    auto o_name = Accept<IdentifierToken>(&curLexer);
    if (!o_name)
        return nullptr;

    // 이름이 같아야 ctor이다
    if (o_name->text != structName)
        return nullptr;

    // ex) (int i, int a)
    auto o_parameters = ParseFuncDeclParams(&curLexer, factory);
    if (!o_parameters)
        return nullptr;

    // ex) { ... }
    auto o_body = ParseBody(&curLexer, factory);
    if (!o_body)
        return nullptr;

    *lexer = move(curLexer);
    return factory.MakeSStructCtorDecl(o_accessModifier, move(*o_parameters), move(*o_body));
}

// ~S
SStructDtorDecl* ParseStructDtorDecl(const string& structName, Lexer* lexer, SFactory& factory)
{
    Lexer curLexer{*lexer};
    auto o_accessModifier = ParseAccessModifier(&curLexer);

    if (!Accept<TildeToken>(&curLexer)) return nullptr;

    auto o_name = Accept<IdentifierToken>(&curLexer);
    if (!o_name) return nullptr;

    // 이름이 같아야 dtor이다
    if (o_name->text != structName) return nullptr;

    if (!Accept<LParenToken>(&curLexer)) return nullptr;
    if (!Accept<RParenToken>(&curLexer)) return nullptr;

    auto o_body = ParseBody(&curLexer, factory);
    if (!o_body)
        return nullptr;

    *lexer = move(curLexer);
    return factory.MakeSStructDtorDecl(o_accessModifier, move(*o_body));
}

optional<SStructMemberDecl> ParseStructMemberDecl(const string& structName, Lexer* lexer, SFactory& factory)
{
    if (auto o_memberDecl = ParseMemberDecl<SStructMemberDecl>(lexer, factory))
        return *o_memberDecl;

    if (auto* memberDecl = ParseStructFuncDecl(lexer, factory))
        return memberDecl;

    if (auto* memberDecl = ParseStructCtorDecl(structName, lexer, factory))
        return memberDecl;

    if (auto* memberDecl = ParseStructDtorDecl(structName, lexer, factory))
        return memberDecl;

    if (auto* memberDecl = ParseStructVarDecl(lexer, factory))
        return memberDecl;

    return nullopt;
}

SStructDecl* ParseStructDecl(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    // AccessModifier, 텍스트에는 없을 수 있다
    auto o_accessModifier = ParseAccessModifier(&curLexer);

    if (!Accept<StructToken>(&curLexer))
        return nullptr;

    auto o_structName = Accept<IdentifierToken>(&curLexer);
    if (!o_structName)
        return nullptr;

    auto o_typeParams = ParseTypeParams(&curLexer, factory);
    if (!o_typeParams)
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

    vector<SStructMemberDecl> elems;

    // {
    if (!Accept<LBraceToken>(&curLexer))
        return nullptr;

    // } 나올때까지
    while (!Accept<RBraceToken>(&curLexer))
    {
        auto o_elem = ParseStructMemberDecl(o_structName->text, &curLexer, factory);
        if (!o_elem) return nullptr;

        elems.push_back(move(*o_elem));
    }
    
    *lexer = move(curLexer);
    return factory.MakeSStructDecl(o_accessModifier, move(o_structName->text), move(*o_typeParams), move(baseTypes), move(elems));
}

SClassFuncDecl* ParseClassFuncDecl(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    auto o_accessModifier = ParseAccessModifier(&curLexer);

    bool bStatic = Accept<StaticToken>(&curLexer).has_value();
    bool bSequence = Accept<SeqToken>(&curLexer).has_value();

    // ex) void
    auto o_funcRet = ParseFuncReturn(&curLexer, factory);
    if (!o_funcRet) return nullptr;

    // ex) F
    auto o_funcName = Accept<IdentifierToken>(&curLexer);
    if (!o_funcName) return nullptr;

    // ex) <T1, T2>
    auto o_typeParams = ParseTypeParams(&curLexer, factory);
    if (!o_typeParams) return nullptr;

    // ex) (int i, int a)
    auto o_parameters = ParseFuncDeclParams(&curLexer, factory);
    if (!o_parameters) return nullptr;

    // ex) { ... }
    auto o_body = ParseBody(&curLexer, factory);
    if (!o_body) return nullptr;

    RETURN_ACCEPT(factory.Make<SClassFuncDecl>(
        o_accessModifier,
        bStatic, bSequence,
        move(*o_funcRet),
        move(o_funcName->text),
        move(*o_typeParams),
        move(*o_parameters),
        move(*o_body)));
}

SClassCtorDecl* ParseClassCtorDecl(const string& className, Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    auto o_accessModifier = ParseAccessModifier(&curLexer);

    // ex) F
    auto o_name = Accept<IdentifierToken>(&curLexer);
    if (!o_name)
        return nullptr;

    // 이름이 같아야 ctor다
    if (o_name->text != className)
        return nullptr;

    // ex) (int i, int a)
    auto o_parameters = ParseFuncDeclParams(&curLexer, factory);
    if (!o_parameters)
        return nullptr;

    // : base()
    SArguments* baseArgs = nullptr;
    if (Accept<ColonToken>(&curLexer))
    {
        auto o_expectedToBeBase = Accept<IdentifierToken>(&curLexer);
        if (!o_expectedToBeBase)
            return nullptr;

        // base가 아닌 identifier는 오면 안된다. 다음은 '{' 토큰이다
        if (o_expectedToBeBase->text != "base")
            return nullptr;
            
        baseArgs = ParseCallArgs(&curLexer, factory);
        if (!baseArgs)
            return nullptr;
    }

    // ex) { ... }
    auto o_body = ParseBody(&curLexer, factory);
    if (!o_body)
        return nullptr;

    *lexer = move(curLexer);
    return factory.MakeSClassCtorDecl(o_accessModifier, move(*o_parameters), move(baseArgs), move(*o_body));
}

SClassVarDecl* ParseClassVarDecl(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;
    auto o_accessModifier = ParseAccessModifier(&curLexer);

    // ex) int
    auto* varType = ParseTypeExp(&curLexer, factory);
    if (!varType)
        return nullptr;

    // ex) x, y, z
    vector<string> varNames;  

    auto o_varNameToken0 = Accept<IdentifierToken>(&curLexer);
    if (!o_varNameToken0)
        return nullptr;

    varNames.push_back(move(o_varNameToken0->text));

    while (Accept<CommaToken>(&curLexer))
    {
        auto o_varNameToken = Accept<IdentifierToken>(&curLexer);
        if (!o_varNameToken)
            return nullptr;

        varNames.push_back(move(o_varNameToken->text));
    }

    // ;
    if (!Accept<SemiColonToken>(&curLexer))
        return nullptr;

    *lexer = move(curLexer);
    return factory.MakeSClassVarDecl(o_accessModifier, varType, move(varNames));
}

optional<SClassMemberDecl> ParseClassMemberDecl(string& className, Lexer* lexer, SFactory& factory)
{
    if (auto o_memberDecl = ParseMemberDecl<SClassMemberDecl>(lexer, factory))
        return *o_memberDecl;

    if (auto* memberDecl = ParseClassFuncDecl(lexer, factory))
        return memberDecl;

    if (auto* memberDecl = ParseClassCtorDecl(className, lexer, factory))
        return memberDecl;

    if (auto* memberDecl = ParseClassVarDecl(lexer, factory))
        return memberDecl;

    return nullopt;
}

SClassDecl* ParseClassDecl(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    // AccessModifier, 텍스트에는 없을 수 있다
    auto o_accessModifier = ParseAccessModifier(&curLexer);

    // class
    if (!Accept<ClassToken>(&curLexer))
        return nullptr;

    // C
    auto o_className = Accept<IdentifierToken>(&curLexer);
    if (!o_className)
        return nullptr;

    // <T1, T2>
    auto o_typeParams = ParseTypeParams(&curLexer, factory);
    if (!o_typeParams)
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

    vector<SClassMemberDecl> members;

    // {
    if (!Accept<LBraceToken>(&curLexer))
        return nullptr;

    // } 나올때까지
    while (!Accept<RBraceToken>(&curLexer))
    {
        auto o_elem = ParseClassMemberDecl(o_className->text, &curLexer, factory);
        if (!o_elem) return nullptr;

        members.push_back(move(*o_elem));
    }

    *lexer = move(curLexer);
    return factory.MakeSClassDecl(
        o_accessModifier, 
        move(o_className->text), 
        move(*o_typeParams), 
        move(baseTypes), 
        move(members)
    );
}

optional<SNamespaceDeclElement> ParseNamespaceElement(Lexer* lexer, SFactory& factory)
{
    if (auto* decl = ParseNamespaceDecl(lexer, factory))
        return decl;
    
    if (auto o_elem = ParseMemberDecl<SNamespaceDeclElement>(lexer, factory))
        return *o_elem;

    if (auto* decl = ParseGlobalFuncDecl(lexer, factory))
        return decl;

    return nullopt;
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
    auto o_nsName = Accept<IdentifierToken>(&curLexer);
    if (!o_nsName)
        return nullptr;

    nsNames.push_back(move(o_nsName->text));

    // . optional
    while (Accept<DotToken>(&curLexer))
    {
        // ex) NS
        o_nsName = Accept<IdentifierToken>(&curLexer);
        if (!o_nsName)
            return nullptr;

        nsNames.push_back(move(o_nsName->text));
    }

    // {
    if (!Accept<LBraceToken>(&curLexer))
        return nullptr;

    vector<SNamespaceDeclElement> elems;
    // } 가 나올때까지
    while (!Accept<RBraceToken>(&curLexer))
    {
        auto o_elem = ParseNamespaceElement(&curLexer, factory);
        if (!o_elem)
            return nullptr;

        elems.push_back(*o_elem);
    }

    *lexer = move(curLexer);
    return factory.MakeSNamespaceDecl(move(nsNames), move(elems));
}

optional<SScriptElement> ParseScriptElement(Lexer* lexer, SFactory& factory)
{
    if (auto* decl = ParseNamespaceDecl(lexer, factory))
        return decl;

    if (auto o_elem = ParseMemberDecl<SScriptElement>(lexer, factory))
        return *o_elem;

    if (auto* decl = ParseGlobalFuncDecl(lexer, factory))
        return decl;

    return nullopt;
}

SScript* ParseScript(Lexer* lexer, SFactory& factory)
{
    Lexer curLexer = *lexer;

    vector<SScriptElement> elems;
    while (!Accept<EndOfFileToken>(&curLexer))
    {
        auto o_scriptElem = ParseScriptElement(&curLexer, factory);

        if (!o_scriptElem)
            return nullptr;

        elems.push_back(*o_scriptElem);
    }

    return factory.MakeSScript(move(elems));
}

}