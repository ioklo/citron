using Citron.LexicalAnalysis;
using Citron.Syntax;
using System;
using System.Collections.Generic;
using Citron.Collections;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Microsoft.VisualBasic;

namespace Citron
{
    // 백트래킹을 하는데는 immuatable이 편하기 때문에, Immutable로 간다
    public struct ParserContext
    {
        public LexerContext LexerContext { get; }

        public static ParserContext Make(LexerContext lexerContext)
        {
            return new ParserContext(lexerContext);
        }

        private ParserContext(LexerContext lexerContext)
        {
            LexerContext = lexerContext;
        }

        public ParserContext Update(LexerContext newContext)
        {
            return new ParserContext(newContext);
        }
    }
    
    public partial struct ScriptParser
    {
        Lexer lexer;
        ParserContext context;

        public static bool Parse(Lexer lexer, ref ParserContext context, [NotNullWhen(returnValue: true)] out Script? outScript)
        {
            //var buffer = new Buffer(new StringReader(input));
            //var pos = buffer.MakePosition().Next();
            //var context = ParserContext.Make(LexerContext.Make(pos));

            var parser = new ScriptParser() { lexer = lexer, context = context };
            if (!parser.ParseScript(out outScript))
                return false;

            context = parser.context;
            return true;
        }

        bool Accept(SingleToken token)
        {
            var lexResult = lexer.LexNormalMode(context.LexerContext, true);
            return Accept(token, lexResult);
        }

        bool Accept(SingleToken token, LexResult lexResult)
        {
            if (lexResult.HasValue && lexResult.Token == token)
            {
                context = context.Update(lexResult.Context);
                return true;
            }

            return false;
        }

        bool Accept<TToken>([NotNullWhen(returnValue: true)] out TToken? token) where TToken : Token
        {
            var lexResult = lexer.LexNormalMode(context.LexerContext, true);
            return Accept(lexResult, out token);
        }

        bool Accept<TToken>(LexResult lexResult, [NotNullWhen(returnValue: true)] out TToken? token) where TToken : Token
        {
            if (lexResult.HasValue && lexResult.Token is TToken resultToken)
            {   
                context = context.Update(lexResult.Context);
                token = resultToken;
                return true;
            }

            token = null;
            return false;
        }

        // int t
        // ref int t
        // params T t
        bool InternalParseFuncDeclParam([NotNullWhen(returnValue: true)] out FuncParam? outFuncParam)
        {
            // params
            bool hasParams = Accept(Tokens.Params);

            if (!TypeExpParser.Parse(lexer, ref context, out var typeExp))
            { 
                outFuncParam = null;
                return false;
            }

            if (!Accept<IdentifierToken>(out var name))
            {
                outFuncParam = null;
                return false;
            }

            outFuncParam = new FuncParam(HasParams: hasParams, typeExp, name.Value);
            return true;
        }
        bool InternalParseFuncDeclParams([NotNullWhen(returnValue: true)] out ImmutableArray<FuncParam>? outFuncParams)
        {
            if (!Accept(Tokens.LParen))
            {
                outFuncParams = null;
                return false;
            }

            var paramsBuilder = ImmutableArray.CreateBuilder<FuncParam>();
            while (!Accept(Tokens.RParen))
            {
                if (paramsBuilder.Count != 0)
                    if (!Accept(Tokens.Comma))
                    {
                        outFuncParams = null;
                        return false;
                    }

                if (!ParseFuncDeclParam(out var funcDeclParam))
                { 
                    outFuncParams = null;
                    return false;
                }
                
                paramsBuilder.Add(funcDeclParam.Value);
            }

            outFuncParams = paramsBuilder.ToImmutable();
            return true;
        }
        
        bool InternalParseGlobalFuncDecl([NotNullWhen(returnValue: true)] out GlobalFuncDecl? outDecl)
        {
            // <SEQ> <RetTypeName> <FuncName> <LPAREN> <ARGS> <RPAREN>
            // LBRACE>
            // [Stmt]
            // <RBRACE>            

            // seq
            bool bSequence = Accept(Tokens.Seq);

            if (!TypeExpParser.Parse(lexer, ref context, out var retType))
            {
                outDecl = null;
                return false;
            }

            if (!Accept<IdentifierToken>(out var funcName))
            {
                outDecl = null;
                return false;
            }

            if (!ParseFuncDeclParams(out var parameters))
            {
                outDecl = null;
                return false;
            }

            if (!StmtParser.ParseBody(lexer, ref context, out var body))
            {
                outDecl = null;
                return false;
            }

            outDecl = new GlobalFuncDecl(
                accessModifier: null, // TODO: [7] 일단 null
                bSequence,
                retType,
                funcName.Value,
                default,
                parameters.Value,
                body.Value
            );

            return true;
        }

        // <T1, T2, ...>
        bool InternalParseTypeParams([NotNullWhen(returnValue: true)] out ImmutableArray<TypeParam>? outTypeParams)
        {
            // typeParams
            var typeParamsBuilder = ImmutableArray.CreateBuilder<TypeParam>();
            if (Accept(Tokens.LessThan))
            {
                while (!Accept(Tokens.GreaterThan))
                {
                    if (0 < typeParamsBuilder.Count)
                        if (!Accept(Tokens.Comma))
                        {
                            outTypeParams = null;
                            return false;
                        }

                    // 변수 이름만 받을 것이므로 TypeExp가 아니라 Identifier여야 한다
                    if (!Accept<IdentifierToken>(out var typeParam))
                    {
                        outTypeParams = null;
                        return false;
                    }

                    typeParamsBuilder.Add(new TypeParam(typeParam.Value));
                }
            }

            outTypeParams = typeParamsBuilder.ToImmutable();
            return true;
        }

        bool InternalParseTypeDecl([NotNullWhen(returnValue: true)] out TypeDecl? outTypeDecl)
        {
            if (ParseEnumDecl(out var enumDecl))
            {
                outTypeDecl = enumDecl;
                return true;
            }

            if (ParseStructDecl(out var structDecl))
            {
                outTypeDecl = structDecl;
                return true;
            }

            if (ParseClassDecl(out var classDecl))
            {
                outTypeDecl = classDecl;
                return true;
            }

            outTypeDecl = null;
            return false;
        }

        bool InternalParseEnumDecl([NotNullWhen(returnValue: true)] out EnumDecl? outDecl)
        {
            // public enum E<T1, T2> { a , b () } 
            ParseAccessModifier(out var accessModifier);

            if (!Accept(Tokens.Enum))
            {
                outDecl = null;
                return false;
            }

            if (!Accept<IdentifierToken>(out var enumName))
            {
                outDecl = null;
                return false;
            }

            if (!ParseTypeParams(out var typeParams))
            {
                outDecl = null;
                return false;
            }

            if (!Accept(Tokens.LBrace))
            {
                outDecl = null;
                return false;
            }

            var elemsBuilder = ImmutableArray.CreateBuilder<EnumElemDecl>();
            while (!Accept(Tokens.RBrace))
            {
                if (0 < elemsBuilder.Count)
                    if (!Accept(Tokens.Comma))
                    {
                        outDecl = null;
                        return false;
                    }

                if (!Accept<IdentifierToken>(out var elemName))
                {
                    outDecl = null;
                    return false;
                }

                var paramsBuilder = ImmutableArray.CreateBuilder<EnumElemMemberVarDecl>();
                if (Accept(Tokens.LParen))
                {
                    while (!Accept(Tokens.RParen))
                    {
                        if (0 < paramsBuilder.Count)
                            if (!Accept(Tokens.Comma))
                            {
                                outDecl = null;
                                return false;
                            }

                        if (!TypeExpParser.Parse(lexer, ref context, out var typeExp))
                        {
                            outDecl = null;
                            return false;
                        }

                        if (!Accept<IdentifierToken>(out var paramName))
                        {
                            outDecl = null;
                            return false;
                        }

                        paramsBuilder.Add(new EnumElemMemberVarDecl(typeExp!, paramName.Value));
                    }
                }

                elemsBuilder.Add(new EnumElemDecl(elemName.Value, paramsBuilder.ToImmutable()));
            }

            outDecl = new EnumDecl(accessModifier, enumName.Value, typeParams.Value, elemsBuilder.ToImmutable());
            return true;
        }

        bool InternalParseAccessModifier([NotNullWhen(returnValue: true)] out AccessModifier? outModifier)
        {
            if (Accept(Tokens.Protected))
            {
                outModifier = AccessModifier.Protected;
                return true;
            }

            if (Accept(Tokens.Private))
            {
                outModifier = AccessModifier.Private;
                return true;
            }

            if (Accept(Tokens.Public))
            {
                outModifier = AccessModifier.Public;
                return true;
            }

            outModifier = null;
            return false;
        }

        bool InternalParseStructMemberTypeDecl([NotNullWhen(returnValue: true)] out StructMemberTypeDecl? outDecl)
        {
            if (!ParseTypeDecl(out var typeDecl))
            {
                outDecl = null;
                return false;
            }

            outDecl = new StructMemberTypeDecl(typeDecl);
            return true;
        }

        bool InternalParseStructMemberVarDecl([NotNullWhen(returnValue: true)] out StructMemberVarDecl? outDecl)
        {
            ParseAccessModifier(out var accessModifier);

            // ex) int
            if (!TypeExpParser.Parse(lexer, ref context, out var varType))
            {
                outDecl = null;
                return false;
            }

            // ex) x, y, z
            var varNamesBuilder = ImmutableArray.CreateBuilder<string>();
            if (!Accept<IdentifierToken>(out var varNameToken0))
            {
                outDecl = null;
                return false;
            }

            varNamesBuilder.Add(varNameToken0.Value);

            while (Accept(Tokens.Comma))
            {
                if (!Accept<IdentifierToken>(out var varNameToken))
                {
                    outDecl = null;
                    return false;
                }

                varNamesBuilder.Add(varNameToken.Value);
            }

            // ;
            if (!Accept(Tokens.SemiColon))
            {
                outDecl = null;
                return false;
            }

            outDecl = new StructMemberVarDecl(accessModifier, varType, varNamesBuilder.ToImmutable());
            return true;
        }

        bool InternalParseStructMemberFuncDecl([NotNullWhen(returnValue: true)] out StructMemberFuncDecl? outDecl)
        {
            ParseAccessModifier(out var accessModifier);

            bool bStatic = Accept(Tokens.Static);
            bool bSequence = Accept(Tokens.Seq);

            // ex) void
            if (!TypeExpParser.Parse(lexer, ref context, out var retType))
            {
                outDecl = null;
                return false;
            }

            // ex) F
            if (!Accept<IdentifierToken>(out var funcName))
            {
                outDecl = null;
                return false;
            }

            // ex) <T1, T2>
            if (!ParseTypeParams(out var typeParams))
            {
                outDecl = null;
                return false;
            }

            // ex) (int i, int a)
            if (!ParseFuncDeclParams(out var parameters))
            {
                outDecl = null;
                return false;
            }

            // ex) { ... }
            if (!StmtParser.ParseBody(lexer, ref context, out var body))
            {
                outDecl = null;
                return false;
            }

            outDecl = new StructMemberFuncDecl(
                accessModifier, bStatic, bSequence, retType, funcName.Value, typeParams.Value, parameters.Value, body.Value
            );

            return true;
        }

        bool InternalParseStructConstructorDecl([NotNullWhen(returnValue: true)] out StructConstructorDecl? outDecl)
        {
            ParseAccessModifier(out var accessModifier);
            
            // ex) F
            if (!Accept<IdentifierToken>(out var name))
            {
                outDecl = null;
                return false;
            }

            // ex) (int i, int a)
            if (!ParseFuncDeclParams(out var parameters))
            {
                outDecl = null;
                return false;
            }

            // ex) { ... }
            if (!StmtParser.ParseBody(lexer, ref context, out var body))
            {
                outDecl = null;
                return false;
            }

            outDecl = new StructConstructorDecl(accessModifier, name.Value, parameters.Value, body.Value);
            return true;
        }

        bool InternalParseStructMemberDecl([NotNullWhen(returnValue: true)] out StructMemberDecl? outDecl)
        {
            if (ParseStructMemberTypeDecl(out var typeDecl))
            {
                outDecl = typeDecl;
                return true;
            }

            if (ParseStructMemberFuncDecl(out var funcDecl))
            {
                outDecl = funcDecl;
                return true;
            }

            if (ParseStructConstructorDecl(out var constructorDecl))
            {
                outDecl = constructorDecl;
                return true;
            }

            if (ParseStructMemberVarDecl(out var varDecl))
            {
                outDecl = varDecl;
                return true;
            }


            outDecl = null;
            return false;
        }
        
        bool InternalParseStructDecl([NotNullWhen(returnValue: true)] out StructDecl? outDecl)
        {
            // AccessModifier, 텍스트에는 없을 수 있다
            ParseAccessModifier(out var accessModifier);            

            if (!Accept(Tokens.Struct))
            {
                outDecl = null;
                return false;
            }

            if (!Accept<IdentifierToken>(out var structName))
            {
                outDecl = null;
                return false;
            }

            if (!ParseTypeParams(out var typeParams))
            {
                outDecl = null;
                return false;
            }

            // 상속 부분 : B, I, ...
            var baseTypesBuilder = ImmutableArray.CreateBuilder<TypeExp>();
            if (Accept(Tokens.Colon))
            {
                if (!TypeExpParser.Parse(lexer, ref context, out var baseType0))
                {
                    outDecl = null;
                    return false;
                }

                baseTypesBuilder.Add(baseType0);

                while (Accept(Tokens.Comma))
                {
                    if (!TypeExpParser.Parse(lexer, ref context, out var baseType))
                    {
                        outDecl = null;
                        return false;
                    }

                    baseTypesBuilder.Add(baseType);
                }
            }

            var elemsBuilder = ImmutableArray.CreateBuilder<StructMemberDecl>();

            // {
            if (!Accept(Tokens.LBrace))
            {
                outDecl = null;
                return false;
            }

            // } 나올때까지
            while(!Accept(Tokens.RBrace))
            {
                if (!ParseStructMemberDecl(out var elem))
                {
                    outDecl = null;
                    return false;
                }

                elemsBuilder.Add(elem);
            }

            outDecl = new StructDecl(accessModifier, structName.Value, typeParams.Value, baseTypesBuilder.ToImmutable(), elemsBuilder.ToImmutable());
            return true;
        }

        bool InternalParseClassMemberTypeDecl([NotNullWhen(returnValue: true)] out ClassMemberTypeDecl? outDecl)
        {
            if (!ParseTypeDecl(out var typeDecl))
            {
                outDecl = null;
                return false;
            }

            outDecl = new ClassMemberTypeDecl(typeDecl);
            return true;
        }

        bool InternalParseClassMemberFuncDecl([NotNullWhen(returnValue: true)] out ClassMemberFuncDecl? outDecl)
        {
            ParseAccessModifier(out var accessModifier);
            
            bool bStatic = Accept(Tokens.Static);
            bool bSequence = Accept(Tokens.Seq);

            // ex) void
            if (!TypeExpParser.Parse(lexer, ref context, out var retType))
            {
                outDecl = null;
                return false;
            }

            // ex) F
            if (!Accept<IdentifierToken>(out var funcName))
            {
                outDecl = null;
                return false;
            }

            // ex) <T1, T2>
            if (!ParseTypeParams(out var typeParams))
            {
                outDecl = null;
                return false;
            }

            // ex) (int i, int a)
            if (!ParseFuncDeclParams(out var parameters))
            {
                outDecl = null;
                return false;
            }

            // ex) { ... }
            if (!StmtParser.ParseBody(lexer, ref context, out var body))
            {
                outDecl = null;
                return false;
            }

            outDecl = new ClassMemberFuncDecl(accessModifier, bStatic, bSequence, retType, funcName.Value, typeParams.Value, parameters.Value, body.Value);
            return true;
        }

        bool InternalParseClassConstructorDecl([NotNullWhen(returnValue: true)] out ClassConstructorDecl? outDecl)
        {
            ParseAccessModifier(out var accessModifier);
            
            // ex) F
            if (!Accept<IdentifierToken>(out var name))
            {
                outDecl = null;
                return false;
            }

            // ex) (int i, int a)
            if (!ParseFuncDeclParams(out var parameters))
            {
                outDecl = null;
                return false;
            }

            // : base()
            ImmutableArray<Argument>? baseArgs = null;
            if (Accept(Tokens.Colon))
            {
                if (Accept<IdentifierToken>(out var expectedToBeBase))
                {
                    if (expectedToBeBase.Value == "base")
                    {
                        if (!ExpParser.ParseCallArgs(lexer, ref context, out var args))
                        {
                            outDecl = null;
                            return false;
                        }

                        baseArgs = args;
                    }
                    else // base가 아닌 identifier는 오면 안된다. 다음은 '{' 토큰이다
                    {
                        outDecl = null;
                        return false;
                    }
                }
                else
                {
                    outDecl = null;
                    return false;
                }
            }

            // ex) { ... }
            if (!StmtParser.ParseBody(lexer, ref context, out var body))
            {
                outDecl = null;
                return false;
            }

            outDecl = new ClassConstructorDecl(accessModifier, name.Value, parameters.Value, baseArgs, body.Value);
            return true;
        }

        bool InternalParseClassMemberVarDecl([NotNullWhen(returnValue: true)] out ClassMemberVarDecl? outDecl)
        {
            ParseAccessModifier(out var accessModifier);
            
            // ex) int
            if (!TypeExpParser.Parse(lexer, ref context, out var varType))
            {
                outDecl = null;
                return false;
            }

            // ex) x, y, z
            var varNamesBuilder = ImmutableArray.CreateBuilder<string>();
            if (!Accept<IdentifierToken>(out var varNameToken0))
            {
                outDecl = null;
                return false;
            }

            varNamesBuilder.Add(varNameToken0.Value);

            while (Accept(Tokens.Comma))
            {
                if (!Accept<IdentifierToken>(out var varNameToken))
                {
                    outDecl = null;
                    return false;
                }

                varNamesBuilder.Add(varNameToken.Value);
            }

            // ;
            if (!Accept(Tokens.SemiColon))
            {
                outDecl = null;
                return false;
            }

            outDecl = new ClassMemberVarDecl(accessModifier, varType, varNamesBuilder.ToImmutable());
            return true;
        }

        bool InternalParseClassMemberDecl([NotNullWhen(returnValue: true)] out ClassMemberDecl? outDecl)
        {
            if (ParseClassMemberTypeDecl(out var typeDecl))
            {
                outDecl = typeDecl;
                return true;
            }

            if (ParseClassMemberFuncDecl(out var funcDecl))
            {
                outDecl = funcDecl;
                return true;
            }

            if (ParseClassConstructorDecl(out var constructorDecl))
            {
                outDecl = constructorDecl;
                return true;
            }

            if (ParseClassMemberVarDecl(out var varDecl))
            {
                outDecl = varDecl;
                return true;
            }

            outDecl = null;
            return false;
        }

        bool InternalParseClassDecl([NotNullWhen(returnValue: true)] out ClassDecl? outDecl)
        {
            // AccessModifier, 텍스트에는 없을 수 있다
            ParseAccessModifier(out var accessModifier);
            
            // class
            if (!Accept(Tokens.Class))
            {
                outDecl = null;
                return false;
            }

            // C
            if (!Accept<IdentifierToken>(out var className))
            {
                outDecl = null;
                return false;
            }

            // <T1, T2>
            if (!ParseTypeParams(out var typeParams))
            {
                outDecl = null;
                return false;
            }

            // 상속 부분 : B, I, ...
            var baseTypesBuilder = ImmutableArray.CreateBuilder<TypeExp>();
            if (Accept(Tokens.Colon))
            {
                if (!TypeExpParser.Parse(lexer, ref context, out var baseType0))
                {
                    outDecl = null;
                    return false;
                }

                baseTypesBuilder.Add(baseType0);

                while (Accept(Tokens.Comma))
                {
                    if (!TypeExpParser.Parse(lexer, ref context, out var baseType))
                    {
                        outDecl = null;
                        return false;
                    }

                    baseTypesBuilder.Add(baseType);
                }
            }

            var membersBuilder = ImmutableArray.CreateBuilder<ClassMemberDecl>();

            // {
            if (!Accept(Tokens.LBrace))
            {
                outDecl = null;
                return false;
            }

            // } 나올때까지
            while (!Accept(Tokens.RBrace))
            {
                if (!ParseClassMemberDecl(out var elem))
                {
                    outDecl = null;
                    return false;
                }

                membersBuilder.Add(elem);
            }

            outDecl = new ClassDecl(accessModifier, className.Value, typeParams.Value, baseTypesBuilder.ToImmutable(), membersBuilder.ToImmutable());
            return true;
        }

        bool InternalParseNamespaceElement([NotNullWhen(returnValue: true)] out NamespaceElement? outElem)
        {
            if (ParseNamespaceDecl(out var namespaceDecl))
            {
                outElem = new NamespaceDeclNamespaceElement(namespaceDecl);
                return true;
            }

            if (ParseTypeDecl(out var typeDecl))
            {
                outElem = new TypeDeclNamespaceElement(typeDecl);
                return true;
            }

            if (ParseGlobalFuncDecl(out var funcDecl))
            {
                outElem = new GlobalFuncDeclNamespaceElement(funcDecl);
                return true;
            }

            outElem = null;
            return false;
        }

        bool InternalParseNamespaceDecl([NotNullWhen(returnValue: true)] out NamespaceDecl? outDecl)
        {
            // <NAMESPACE> <NAME>(.<NAME> ...) <LBRACE>  ... <RBRACE>

            // namespace
            if (!Accept(Tokens.Namespace))
            {
                outDecl = null;
                return false;
            }

            var nsNamesBuilder = ImmutableArray.CreateBuilder<string>();
            IdentifierToken? nsName;

            // ex) NS
            if (!Accept(out nsName))
            {
                outDecl = null;
                return false;
            }

            nsNamesBuilder.Add(nsName.Value);
            
            // . optional
            while (Accept(Tokens.Dot))
            {
                // ex) NS
                if (!Accept(out nsName))
                {
                    outDecl = null;
                    return false;
                }

                nsNamesBuilder.Add(nsName.Value);
            }

            // {
            if (!Accept(Tokens.LBrace))
            {
                outDecl = null;
                return false;
            }

            var elemsBuilder = ImmutableArray.CreateBuilder<NamespaceElement>();
            // } 가 나올때까지
            while (!Accept(Tokens.RBrace))
            {
                if (!ParseNamespaceElement(out var elem))
                {
                    outDecl = null;
                    return false;
                }

                elemsBuilder.Add(elem);
            }

            outDecl = new NamespaceDecl(nsNamesBuilder.ToImmutable(), elemsBuilder.ToImmutable());
            return true;
        }

        bool InternalParseScriptElement([NotNullWhen(returnValue: true)] out ScriptElement? outElem)
        {
            if (ParseNamespaceDecl(out var namespaceDecl))
            {
                outElem = new NamespaceDeclScriptElement(namespaceDecl);
                return true;
            }

            if (ParseTypeDecl(out var typeDecl))
            {
                outElem = new TypeDeclScriptElement(typeDecl);
                return true;
            }

            if (ParseGlobalFuncDecl(out var funcDecl))
            {
                outElem = new GlobalFuncDeclScriptElement(funcDecl);
                return true;
            }

            outElem = null;
            return false;
        }        

        bool InternalParseScript([NotNullWhen(returnValue: true)] out Script? outScript)
        {
            var builder = ImmutableArray.CreateBuilder<ScriptElement>();

            while (!Accept(Tokens.EndOfFile))
            {
                if (!ParseScriptElement(out var scriptElem))
                {
                    outScript = null;
                    return false;
                }

                builder.Add(scriptElem);
            }

            outScript = new Script(builder.ToImmutable());
            return true;
        }
    }
}