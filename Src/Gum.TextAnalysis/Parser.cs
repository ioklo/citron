using Gum.LexicalAnalysis;
using Gum.Syntax;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

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

        #region Utilities
        bool Accept<TToken>(LexResult lexResult, ref ParserContext context)
        {
            if (lexResult.HasValue && lexResult.Token is TToken)
            {
                context = context.Update(lexResult.Context);
                return true;
            }

            return false;
        }

        bool Accept<TToken>(LexResult lexResult, ref ParserContext context, [NotNullWhen(returnValue:true)] out TToken? token) where TToken : Token
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

        bool Peek<TToken>(LexResult lexResult) where TToken : Token
        {
            return lexResult.HasValue && lexResult.Token is TToken;
        }

        bool Parse<TSyntaxElem>(
            ParseResult<TSyntaxElem> parseResult, 
            ref ParserContext context, 
            [NotNullWhen(returnValue: true)] out TSyntaxElem? elem) where TSyntaxElem : class
        {
            if (!parseResult.HasValue)
            {
                elem = null;
                return false;
            }
            else
            {
                elem = parseResult.Elem;
                context = parseResult.Context;
                return true;
            }
        }

        #endregion

        async ValueTask<ParseResult<TypeExp>> ParseTypeIdExpAsync(ParserContext context)
        {
            if (!Accept<IdentifierToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context, out var idToken))
                return ParseResult<TypeExp>.Invalid;

            var typeArgsBuilder = ImmutableArray.CreateBuilder<TypeExp>();
            if (Accept<LessThanToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                while(!Accept<GreaterThanToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                {
                    if (0 < typeArgsBuilder.Count)
                        if (!Accept<CommaToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                            return ParseResult<TypeExp>.Invalid;

                    if (!Parse(await ParseTypeExpAsync(context), ref context, out var typeArg))
                        return ParseResult<TypeExp>.Invalid;

                    typeArgsBuilder.Add(typeArg);
                }

            return new ParseResult<TypeExp>(new IdTypeExp(idToken.Value, typeArgsBuilder.ToImmutable()), context);
        }

        async ValueTask<ParseResult<TypeExp>> ParsePrimaryTypeExpAsync(ParserContext context)
        {
            if (!Parse(await ParseTypeIdExpAsync(context), ref context, out var typeIdExp))
                return ParseResult<TypeExp>.Invalid;

            TypeExp exp = typeIdExp;
            while(true)
            {
                var lexResult = await lexer.LexNormalModeAsync(context.LexerContext, true);

                // . id (..., ...)
                if (Accept<DotToken>(lexResult, ref context))
                {
                    if (!Accept<IdentifierToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context, out var memberName))
                        return ParseResult<TypeExp>.Invalid;

                    // TODO: typeApp(T.S<>) 처리도 추가
                    exp = new MemberTypeExp(exp, memberName.Value, ImmutableArray<TypeExp>.Empty);
                    continue;
                }

                break;
            }

            return new ParseResult<TypeExp>(exp, context);
        }

        public ValueTask<ParseResult<TypeExp>> ParseTypeExpAsync(ParserContext context)
        {
            return ParsePrimaryTypeExpAsync(context);
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

        internal async ValueTask<ParseResult<FuncDecl>> ParseFuncDeclAsync(ParserContext context)
        {
            // <SEQ> <RetTypeName> <FuncName> <LPAREN> <ARGS> <RPAREN>
            // LBRACE>
            // [Stmt]
            // <RBRACE>            

            FuncKind funcKind;
            if (Accept<SeqToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                funcKind = FuncKind.Sequence;
            else
                funcKind = FuncKind.Normal;

            var retTypeResult = await ParseTypeExpAsync(context);
            if (!retTypeResult.HasValue)
                return Invalid();
            context = retTypeResult.Context;

            if (!Accept<IdentifierToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context, out var funcName))            
                return Invalid();

            if (!Accept<LParenToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return Invalid();

            var funcDeclParams = ImmutableArray.CreateBuilder<TypeAndName>();
            int? variadicParamIndex = null;
            while (!Accept<RParenToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
            {
                if (funcDeclParams.Count != 0)
                    if (!Accept<CommaToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                        return Invalid();

                var funcDeclParam = await ParseFuncDeclParamAsync(context);
                if (!funcDeclParam.HasValue)
                    return Invalid();

                if (funcDeclParam.Elem.bVariadic)
                    variadicParamIndex = funcDeclParams.Count;

                funcDeclParams.Add(funcDeclParam.Elem.FuncDeclParam);
                context = funcDeclParam.Context;                
            }

            var blockStmtResult = await stmtParser.ParseBlockStmtAsync(context);
            if (!blockStmtResult.HasValue)
                return Invalid();

            context = blockStmtResult.Context;

            return new ParseResult<FuncDecl>(
                new FuncDecl(
                    funcKind, 
                    retTypeResult.Elem, 
                    funcName.Value,
                    ImmutableArray<string>.Empty,
                    funcDeclParams.ToImmutable(), 
                    variadicParamIndex, 
                    blockStmtResult.Elem), 
                context);

            static ParseResult<FuncDecl> Invalid() => ParseResult<FuncDecl>.Invalid;
        }

        internal async ValueTask<ParseResult<EnumDecl>> ParseEnumDeclAsync(ParserContext context)
        {
            // enum E<T1, T2> { a , b () } 
            if (!Accept<EnumToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return ParseResult<EnumDecl>.Invalid;

            if (!Accept<IdentifierToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context, out var enumName))
                return ParseResult<EnumDecl>.Invalid;
            
            // typeParams
            var typeParamsBuilder = ImmutableArray.CreateBuilder<string>();
            if (Accept<LessThanToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
            {
                while(!Accept<GreaterThanToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                {
                    if (0 < typeParamsBuilder.Count)
                        if (!Accept<CommaToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                            return ParseResult<EnumDecl>.Invalid;                    

                    // 변수 이름만 받을 것이므로 TypeExp가 아니라 Identifier여야 한다
                    if (!Accept<IdentifierToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context, out var typeParam))
                        return ParseResult<EnumDecl>.Invalid;

                    typeParamsBuilder.Add(typeParam.Value);
                }
            }

            if (!Accept<LBraceToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return ParseResult<EnumDecl>.Invalid;

            var elements = ImmutableArray.CreateBuilder<EnumDeclElement>();
            while (!Accept<RBraceToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
            {
                if (0 < elements.Count)
                    if (!Accept<CommaToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                        return ParseResult<EnumDecl>.Invalid;

                if (!Accept<IdentifierToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context, out var elemName))
                    return ParseResult<EnumDecl>.Invalid;

                var parameters = ImmutableArray.CreateBuilder<TypeAndName>();
                if (Accept<LParenToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                {
                    while (!Accept<RParenToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                    {
                        if (!Parse(await ParseTypeExpAsync(context), ref context, out var typeExp))
                            return ParseResult<EnumDecl>.Invalid;

                        if (!Accept<IdentifierToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context, out var paramName))
                            return ParseResult<EnumDecl>.Invalid;

                        parameters.Add(new TypeAndName(typeExp!, paramName.Value));
                    }
                }

                elements.Add(new EnumDeclElement(elemName.Value, parameters.ToImmutable()));
            }

            return new ParseResult<EnumDecl>(new EnumDecl(enumName.Value, typeParamsBuilder.ToImmutable(), elements.ToImmutable()), context);
        }

        async ValueTask<ParseResult<ScriptElement>> ParseScriptElementAsync(ParserContext context)
        {
            if (Parse(await ParseEnumDeclAsync(context), ref context, out var enumDecl))
                return new ParseResult<ScriptElement>(new EnumDeclScriptElement(enumDecl!), context);

            if (Parse(await ParseFuncDeclAsync(context), ref context, out var funcDecl))
                return new ParseResult<ScriptElement>(new FuncDeclScriptElement(funcDecl!), context);

            if (Parse(await stmtParser.ParseStmtAsync(context), ref context, out var stmt))
                return new ParseResult<ScriptElement>(new StmtScriptElement(stmt!), context);

            return ParseResult<ScriptElement>.Invalid;
        }

        public async ValueTask<ParseResult<Script>> ParseScriptAsync(ParserContext context)
        {
            var elems = ImmutableArray.CreateBuilder<ScriptElement>();

            while (!Accept<EndOfFileToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
            {
                var elemResult = await ParseScriptElementAsync(context);
                if (!elemResult.HasValue) return ParseResult<Script>.Invalid;

                elems.Add(elemResult.Elem);
                context = elemResult.Context;
            }

            return new ParseResult<Script>(new Script(elems.ToImmutable()), context);
        }
    }
}