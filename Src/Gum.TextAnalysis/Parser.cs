using Gum.LexicalAnalysis;
using Gum.Syntax;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using static Gum.ParserMisc;

namespace Gum
{
    // 백트래킹을 하는데는 immuatable이 편하기 때문에, Immutable로 간다
    public struct ParserContext
    {
        public LexerContext LexerContext { get; }
        ImmutableHashSet<string> types;

        public static ParserContext Make(LexerContext lexerContext)
        {
            return new ParserContext(lexerContext, ImmutableHashSet<string>.Empty);
        }

        private ParserContext(LexerContext lexerContext, ImmutableHashSet<string> types)
        {
            LexerContext = lexerContext;
            this.types = types;
        }

        public ParserContext Update(LexerContext newContext)
        {
            return new ParserContext(newContext, types);
        }
    }

    public struct ParseResult<TSyntaxElem>
    {
        public static ParseResult<TSyntaxElem> Invalid;
        static ParseResult()
        {
            Invalid = new ParseResult<TSyntaxElem>();
        }

        public bool HasValue { get; }
        public TSyntaxElem Elem { get; }
        public ParserContext Context { get; }
        public ParseResult(TSyntaxElem elem, ParserContext context)
        {
            HasValue = true;
            Elem = elem;
            Context = context;
        }
    }

    public class Parser
    {
        Lexer lexer;
        internal ExpParser expParser;
        internal StmtParser stmtParser;

        public Parser(Lexer lexer)
        {
            this.lexer = lexer;
            expParser = new ExpParser(this, lexer);
            stmtParser = new StmtParser(this, lexer);
        }

        public async ValueTask<Script?> ParseScriptAsync(string input)
        {
            var buffer = new Buffer(new StringReader(input));
            var pos = await buffer.MakePosition().NextAsync();
            var context = ParserContext.Make(LexerContext.Make(pos));

            var scriptResult = await ParseScriptAsync(context);
            return scriptResult.HasValue ? scriptResult.Elem : null;
        }

        public ValueTask<ParseResult<Exp>> ParseExpAsync(ParserContext context)
        {
            return expParser.ParseExpAsync(context);
        }

        public ValueTask<ParseResult<Stmt>> ParseStmtAsync(ParserContext context)
        {
            return stmtParser.ParseStmtAsync(context);
        }       

        public async ValueTask<ParseResult<ImmutableArray<TypeExp>>> ParseTypeArgs(ParserContext context)
        {
            var typeArgsBuilder = ImmutableArray.CreateBuilder<TypeExp>();

            if (!Accept<LessThanToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return ParseResult<ImmutableArray<TypeExp>>.Invalid;

            while (!Accept<GreaterThanToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
            {
                if (0 < typeArgsBuilder.Count)
                    if (!Accept<CommaToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                        throw new ParseFatalException();

                if (!Parse(await ParseTypeExpAsync(context), ref context, out var typeArg))
                    throw new ParseFatalException();

                typeArgsBuilder.Add(typeArg);
            }

            return new ParseResult<ImmutableArray<TypeExp>>(typeArgsBuilder.ToImmutable(), context);
        }

        async ValueTask<ParseResult<TypeExp>> ParseTypeIdExpAsync(ParserContext context)
        {
            if (!Accept<IdentifierToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context, out var idToken))
                return ParseResult<TypeExp>.Invalid;
            
            Parse(await ParseTypeArgs(context), ref context, out var typeArgs);

            return new ParseResult<TypeExp>(new IdTypeExp(idToken.Value, typeArgs), context);
        }

        async ValueTask<ParseResult<TypeExp>> ParsePrimaryTypeExpAsync(ParserContext context)
        {
            if (!Parse(await ParseTypeIdExpAsync(context), ref context, out var typeIdExp))
                return ParseResult<TypeExp>.Invalid;

            TypeExp exp = typeIdExp;
            while(true)
            {
                var lexResult = await lexer.LexNormalModeAsync(context.LexerContext, true);

                // . id <..., ...>
                if (Accept<DotToken>(lexResult, ref context))
                {
                    if (!Accept<IdentifierToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context, out var memberName))
                        return ParseResult<TypeExp>.Invalid;

                    // TODO: typeApp(T.S<>) 처리도 추가
                    exp = new MemberTypeExp(exp, memberName.Value, default);
                    continue;
                }

                break;
            }

            return new ParseResult<TypeExp>(exp, context);
        }

        public async ValueTask<ParseResult<TypeExp>> ParseTypeExpAsync(ParserContext context)
        {
            // ref 
            if (Accept<RefToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
            {
                if (!Parse(await ParsePrimaryTypeExpAsync(context), ref context, out var typeExp))
                    return ParseResult<TypeExp>.Invalid;

                return new ParseResult<TypeExp>(new RefTypeExp(typeExp), context);
            }

            return await ParsePrimaryTypeExpAsync(context);
        }

        // int a, 
        async ValueTask<ParseResult<(TypeAndName FuncDeclParam, bool bVariadic)>> ParseFuncDeclParamAsync(ParserContext context)
        {
            var bVariadic = Accept<ParamsToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context);

            var typeExpResult = await ParseTypeExpAsync(context);
            if (!typeExpResult.HasValue)
                return ParseResult<(TypeAndName, bool)>.Invalid;

            context = typeExpResult.Context;

            if (!Accept<IdentifierToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context, out var name))
                return ParseResult<(TypeAndName, bool)>.Invalid;

            return new ParseResult<(TypeAndName, bool)>((new TypeAndName(typeExpResult.Elem, name.Value), bVariadic), context);
        }

        async ValueTask<ParseResult<FuncParamInfo>> ParseFuncDeclParamsAsync(ParserContext context)
        {
            ParseResult<FuncParamInfo> Invalid() => ParseResult<FuncParamInfo>.Invalid;

            if (!Accept<LParenToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return Invalid();

            var paramsBuilder = ImmutableArray.CreateBuilder<TypeAndName>();
            int? variadicParamIndex = null;
            while (!Accept<RParenToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
            {
                if (paramsBuilder.Count != 0)
                    if (!Accept<CommaToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                        return Invalid();

                var funcDeclParam = await ParseFuncDeclParamAsync(context);
                if (!funcDeclParam.HasValue)
                    return Invalid();

                if (funcDeclParam.Elem.bVariadic)
                    variadicParamIndex = paramsBuilder.Count;

                paramsBuilder.Add(funcDeclParam.Elem.FuncDeclParam);
                context = funcDeclParam.Context;
            }

            return new ParseResult<FuncParamInfo>(new FuncParamInfo(paramsBuilder.ToImmutable(), variadicParamIndex), context);
        }

        internal async ValueTask<ParseResult<GlobalFuncDecl>> ParseGlobalFuncDeclAsync(ParserContext context)
        {
            static ParseResult<GlobalFuncDecl> Invalid() => ParseResult<GlobalFuncDecl>.Invalid;

            // <SEQ> <RetTypeName> <FuncName> <LPAREN> <ARGS> <RPAREN>
            // LBRACE>
            // [Stmt]
            // <RBRACE>            

            // seq
            bool bSequence = Accept<SeqToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context);

            if (!Parse(await ParseTypeExpAsync(context), ref context, out var retType))
                return Invalid();

            if (!Accept<IdentifierToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context, out var funcName))            
                return Invalid();

            if (!Parse(await ParseFuncDeclParamsAsync(context), ref context, out var paramInfo))
                return Invalid();

            if (!Parse(await stmtParser.ParseBlockStmtAsync(context), ref context, out var body))
                return Invalid();

            return new ParseResult<GlobalFuncDecl>(
                new GlobalFuncDecl(
                    bSequence, 
                    retType, 
                    funcName.Value,
                    default,
                    paramInfo,
                    body), 
                context);
        }

        // <T1, T2, ...>
        async ValueTask<ParseResult<ImmutableArray<string>>> ParseTypeParamsAsync(ParserContext context)
        {
            // typeParams
            var typeParamsBuilder = ImmutableArray.CreateBuilder<string>();
            if (Accept<LessThanToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
            {
                while (!Accept<GreaterThanToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                {
                    if (0 < typeParamsBuilder.Count)
                        if (!Accept<CommaToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                            return ParseResult<ImmutableArray<string>>.Invalid;

                    // 변수 이름만 받을 것이므로 TypeExp가 아니라 Identifier여야 한다
                    if (!Accept<IdentifierToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context, out var typeParam))
                        return ParseResult<ImmutableArray<string>>.Invalid;

                    typeParamsBuilder.Add(typeParam.Value);
                }
            }

            return new ParseResult<ImmutableArray<string>>(typeParamsBuilder.ToImmutable(), context);
        }

        internal async ValueTask<ParseResult<TypeDecl>> ParseTypeDeclAsync(ParserContext context)
        {
            if (Parse(await ParseEnumDeclAsync(context), ref context, out var enumDecl))
                return new ParseResult<TypeDecl>(enumDecl, context);

            if (Parse(await ParseStructDeclAsync(context), ref context, out var structDecl))
                return new ParseResult<TypeDecl>(structDecl, context);

            return ParseResult<TypeDecl>.Invalid;
        }

        internal async ValueTask<ParseResult<EnumDecl>> ParseEnumDeclAsync(ParserContext context)
        {
            // enum E<T1, T2> { a , b () } 
            if (!Accept<EnumToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return ParseResult<EnumDecl>.Invalid;

            if (!Accept<IdentifierToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context, out var enumName))
                return ParseResult<EnumDecl>.Invalid;

            if (!Parse(await ParseTypeParamsAsync(context), ref context, out var typeParams))
                return ParseResult<EnumDecl>.Invalid;            
            
            if (!Accept<LBraceToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return ParseResult<EnumDecl>.Invalid;

            var elemsBuilder = ImmutableArray.CreateBuilder<EnumDeclElement>();
            while (!Accept<RBraceToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
            {
                if (0 < elemsBuilder.Count)
                    if (!Accept<CommaToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                        return ParseResult<EnumDecl>.Invalid;

                if (!Accept<IdentifierToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context, out var elemName))
                    return ParseResult<EnumDecl>.Invalid;

                var paramsBuilder = ImmutableArray.CreateBuilder<TypeAndName>();
                if (Accept<LParenToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                {
                    while (!Accept<RParenToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                    {
                        if (!Parse(await ParseTypeExpAsync(context), ref context, out var typeExp))
                            return ParseResult<EnumDecl>.Invalid;

                        if (!Accept<IdentifierToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context, out var paramName))
                            return ParseResult<EnumDecl>.Invalid;

                        paramsBuilder.Add(new TypeAndName(typeExp!, paramName.Value));
                    }
                }

                elemsBuilder.Add(new EnumDeclElement(elemName.Value, paramsBuilder.ToImmutable()));
            }

            return new ParseResult<EnumDecl>(new EnumDecl(enumName.Value, typeParams, elemsBuilder.ToImmutable()), context);
        }

        async ValueTask<ParseResult<AccessModifier>> ParseStructElementAccessModifierAsync(ParserContext context)
        {
            if (Accept<ProtectedToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
            {
                return new ParseResult<AccessModifier>(AccessModifier.Protected, context);
            }
            else if (Accept<PrivateToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
            {
                return new ParseResult<AccessModifier>(AccessModifier.Private, context);
            }
            else if (Accept<PublicToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
            {
                // TODO: public은 기본이므로 쓰지 않아야 합니다 에러메시지
                return ParseResult<AccessModifier>.Invalid;
            }

            // 기본 public
            return new ParseResult<AccessModifier>(AccessModifier.Public, context);
        }

        async ValueTask<ParseResult<TypeStructDeclElement>> ParseStructTypeDeclElementAsync(ParserContext context)
        {
            if (!Parse(await ParseTypeDeclAsync(context), ref context, out var typeDecl))
                return ParseResult<TypeStructDeclElement>.Invalid;

            var typeDeclElem = new TypeStructDeclElement(typeDecl);
            return new ParseResult<TypeStructDeclElement>(typeDeclElem, context);
        }

        async ValueTask<ParseResult<VarStructDeclElement>> ParseStructVarDeclElementAsync(ParserContext context)
        {
            if (!Parse(await ParseStructElementAccessModifierAsync(context), ref context, out var accessModifier))
                return ParseResult<VarStructDeclElement>.Invalid;

            // ex) int
            if (!Parse(await ParseTypeExpAsync(context), ref context, out var varType))
                return ParseResult<VarStructDeclElement>.Invalid;

            // ex) x, y, z
            var varNamesBuilder = ImmutableArray.CreateBuilder<string>();            
            if (!Accept<IdentifierToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context, out var varNameToken0))
                return ParseResult<VarStructDeclElement>.Invalid;

            varNamesBuilder.Add(varNameToken0.Value);

            while (Accept<CommaToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
            {
                if (!Accept<IdentifierToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context, out var varNameToken))
                    return ParseResult<VarStructDeclElement>.Invalid;

                varNamesBuilder.Add(varNameToken.Value);
            }

            // ;
            if (!Accept<SemiColonToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return ParseResult<VarStructDeclElement>.Invalid;

            var varDeclElem = new VarStructDeclElement(accessModifier, varType, varNamesBuilder.ToImmutable());
            return new ParseResult<VarStructDeclElement>(varDeclElem, context);
        }

        async ValueTask<ParseResult<FuncStructDeclElement>> ParseStructFuncDeclElementAsync(ParserContext context)
        {
            ParseResult<FuncStructDeclElement> Invalid() => ParseResult<FuncStructDeclElement>.Invalid;

            if (!Parse(await ParseStructElementAccessModifierAsync(context), ref context, out var accessModifier))
                return Invalid();

            bool bStatic = Accept<StaticToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context);
            bool bSequence = Accept<SeqToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context);

            // ex) void
            if (!Parse(await ParseTypeExpAsync(context), ref context, out var retType))
                return Invalid();

            // ex) F
            if (!Accept<IdentifierToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context, out var funcName))
                return Invalid();

            // ex) <T1, T2>
            if (!Parse(await ParseTypeParamsAsync(context), ref context, out var typeParams))
                return Invalid();

            // ex) (int i, int a)
            if (!Parse(await ParseFuncDeclParamsAsync(context), ref context, out var paramInfo))
                return Invalid();

            // ex) { ... }
            if (!Parse(await stmtParser.ParseBlockStmtAsync(context), ref context, out var body))
                return Invalid();

            var funcDeclElem = new FuncStructDeclElement(new StructFuncDecl(
                accessModifier, bStatic, bSequence, retType, funcName.Value, typeParams, paramInfo, body
            ));

            return new ParseResult<FuncStructDeclElement>(funcDeclElem, context);
        }

        async ValueTask<ParseResult<StructDeclElement>> ParseStructDeclElementAsync(ParserContext context)
        {
            if (Parse(await ParseStructTypeDeclElementAsync(context), ref context, out var typeDeclElem))
                return new ParseResult<StructDeclElement>(typeDeclElem, context);

            if (Parse(await ParseStructVarDeclElementAsync(context), ref context, out var varDeclElem))
                return new ParseResult<StructDeclElement>(varDeclElem, context);

            if (Parse(await ParseStructFuncDeclElementAsync(context), ref context, out var funcDeclElem))
                return new ParseResult<StructDeclElement>(funcDeclElem, context);

            return ParseResult<StructDeclElement>.Invalid;
        }
        
        internal async ValueTask<ParseResult<StructDecl>> ParseStructDeclAsync(ParserContext context)
        {
            // AccessModifier, 기본 public
            AccessModifier accessModifier = AccessModifier.Public;
            if (Accept<PrivateToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
            {
                accessModifier = AccessModifier.Private;
            }
            else if (Accept<PublicToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
            {
                accessModifier = AccessModifier.Public;
            }
            
            if (!Accept<StructToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return ParseResult<StructDecl>.Invalid;

            if (!Accept<IdentifierToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context, out var structName))
                return ParseResult<StructDecl>.Invalid;

            if (!Parse(await ParseTypeParamsAsync(context), ref context, out var typeParams))
                return ParseResult<StructDecl>.Invalid;

            // 상속 부분 : B, I, ...
            var baseTypesBuilder = ImmutableArray.CreateBuilder<TypeExp>();
            if (Accept<ColonToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
            {
                if (!Parse(await ParseTypeExpAsync(context), ref context, out var baseType0))
                    return ParseResult<StructDecl>.Invalid;
                baseTypesBuilder.Add(baseType0);

                while (Accept<CommaToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                {
                    if (!Parse(await ParseTypeExpAsync(context), ref context, out var baseType))
                        return ParseResult<StructDecl>.Invalid;

                    baseTypesBuilder.Add(baseType);
                }
            }

            var elemsBuilder = ImmutableArray.CreateBuilder<StructDeclElement>();

            // {
            if (!Accept<LBraceToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return ParseResult<StructDecl>.Invalid;

            // } 나올때까지
            while(!Accept<RBraceToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
            {
                if (!Parse(await ParseStructDeclElementAsync(context), ref context, out var elem))
                    return ParseResult<StructDecl>.Invalid;

                elemsBuilder.Add(elem);
            }

            return new ParseResult<StructDecl>(new StructDecl(accessModifier, structName.Value, typeParams, baseTypesBuilder.ToImmutable(), elemsBuilder.ToImmutable()), context);
        }

        async ValueTask<ParseResult<ScriptElement>> ParseScriptElementAsync(ParserContext context)
        {
            if (Parse(await ParseTypeDeclAsync(context), ref context, out var typeDecl))
                return new ParseResult<ScriptElement>(new TypeDeclScriptElement(typeDecl), context);
            
            if (Parse(await ParseGlobalFuncDeclAsync(context), ref context, out var funcDecl))
                return new ParseResult<ScriptElement>(new GlobalFuncDeclScriptElement(funcDecl), context);

            if (Parse(await stmtParser.ParseStmtAsync(context), ref context, out var stmt))
                return new ParseResult<ScriptElement>(new StmtScriptElement(stmt), context);

            return ParseResult<ScriptElement>.Invalid;
        }

        public async ValueTask<ParseResult<Script>> ParseScriptAsync(ParserContext context)
        {
            var builder = ImmutableArray.CreateBuilder<ScriptElement>();

            while (!Accept<EndOfFileToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
            {
                var elemResult = await ParseScriptElementAsync(context);
                if (!elemResult.HasValue) return ParseResult<Script>.Invalid;

                builder.Add(elemResult.Elem);
                context = elemResult.Context;
            }

            return new ParseResult<Script>(new Script(builder.ToImmutable()), context);
        }
    }
}