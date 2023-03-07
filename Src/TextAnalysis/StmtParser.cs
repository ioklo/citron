using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Citron.LexicalAnalysis;
using Citron.Syntax;
using static Citron.ParserMisc;
using System.Collections.Generic;
using Citron.Collections;
using System.ComponentModel;

namespace Citron
{
    public class StmtParser
    {
        Parser parser;
        Lexer lexer;
        
        public StmtParser(Parser parser, Lexer lexer)
        {
            this.parser = parser;
            this.lexer = lexer;
        }
        
        internal async ValueTask<ParseResult<Stmt>> ParseIfStmtAsync(ParserContext context)
        {
            // if (exp) stmt => If(exp, stmt, null)
            // if (exp) stmt0 else stmt1 => If(exp, stmt0, stmt1)
            // if (exp0) if (exp1) stmt1 else stmt2 => If(exp0, If(exp1, stmt1, stmt2))
            // if (exp is typeExp) => IfTestStmt

            if (!Accept<IfToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return Invalid();

            if (!Accept<LParenToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return Invalid();

            if (!Parse(await parser.ParseExpAsync(context), ref context, out var cond))            
                return Invalid();           
            
            if (Accept<IsToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
            {
                if (!Parse(await parser.ParseTypeExpAsync(context), ref context, out var testType))
                    return Invalid();

                // optional                 
                Accept<IdentifierToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context, out var varName);

                if (!Accept<RParenToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                    return Invalid();

                // right assoc, conflict는 별다른 처리를 하지 않고 지나가면 될 것 같다
                if (!Parse(await ParseEmbeddableStmtAsync(context), ref context, out var body))
                    return Invalid();

                EmbeddableStmt? elseBody = null;
                if (Accept<ElseToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                {
                    if (!Parse(await ParseEmbeddableStmtAsync(context), ref context, out elseBody))
                        return Invalid();
                }

                return new ParseResult<Stmt>(new IfTestStmt(cond, testType, varName?.Value, body, elseBody), context);
            }
            else
            {
                if (!Accept<RParenToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                    return Invalid();

                // right assoc, conflict는 별다른 처리를 하지 않고 지나가면 될 것 같다
                if (!Parse(await ParseEmbeddableStmtAsync(context), ref context, out var body))
                    return Invalid();

                EmbeddableStmt? elseBody = null;
                if (Accept<ElseToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                {
                    if (!Parse(await ParseEmbeddableStmtAsync(context), ref context, out elseBody))
                        return Invalid();
                }

                return new ParseResult<Stmt>(new IfStmt(cond!, body, elseBody), context);
            }

            static ParseResult<Stmt> Invalid() => ParseResult<Stmt>.Invalid;
        }

        internal async ValueTask<ParseResult<VarDecl>> ParseVarDeclAsync(ParserContext context)
        {
            // optional ref
            bool bRef = Accept<RefToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context);

            if (!Parse(await parser.ParseTypeExpAsync(context), ref context, out var varType))
                return Invalid();

            var elemsBuilder = ImmutableArray.CreateBuilder<VarDeclElement>();
            do
            {
                if (!Accept<IdentifierToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context, out var varIdResult))
                    return Invalid();

                VarDeclElemInitializer? initializer = null;
                if (Accept<EqualToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                {
                    // ref가 나올 수 있다
                    bool bRefExp = Accept<RefToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context);                    

                    // TODO: ;나 ,가 나올때까지라는걸 명시해주면 좋겠다
                    if (!Parse(await parser.ParseExpAsync(context), ref context, out var initExp))
                        return Invalid();

                    initializer = new VarDeclElemInitializer(bRefExp, initExp);
                }

                elemsBuilder.Add(new VarDeclElement(varIdResult!.Value, initializer));

            } while (Accept<CommaToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context)); // ,가 나오면 계속한다

            return new ParseResult<VarDecl>(new VarDecl(bRef, varType!, elemsBuilder.ToImmutable()), context);

            static ParseResult<VarDecl> Invalid() => ParseResult<VarDecl>.Invalid;
        }

        // int x = 0;
        internal async ValueTask<ParseResult<VarDeclStmt>> ParseVarDeclStmtAsync(ParserContext context)
        {
            if (!Parse(await ParseVarDeclAsync(context), ref context, out var varDecl))
                return ParseResult<VarDeclStmt>.Invalid;

            if (!context.LexerContext.Pos.IsReachEnd() &&
                !Accept<SemiColonToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context)) // ;으로 마무리
                return ParseResult<VarDeclStmt>.Invalid;

            return new ParseResult<VarDeclStmt>(new VarDeclStmt(varDecl!), context);
        }

        async ValueTask<ParseResult<ForStmtInitializer>> ParseForStmtInitializerAsync(ParserContext context)
        {
            if (Parse(await ParseVarDeclAsync(context), ref context, out var varDecl))            
                return new ParseResult<ForStmtInitializer>(new VarDeclForStmtInitializer(varDecl!), context);

            if (Parse(await parser.ParseExpAsync(context), ref context, out var exp))
                return new ParseResult<ForStmtInitializer>(new ExpForStmtInitializer(exp!), context);

            return ParseResult<ForStmtInitializer>.Invalid;
        }

        internal async ValueTask<ParseResult<ForStmt>> ParseForStmtAsync(ParserContext context)
        {
            if (!Accept<ForToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return Invalid();

            if (!Accept<LParenToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return Fatal();

            // TODO: 이 Initializer의 끝은 ';' 이다
            Parse(await ParseForStmtInitializerAsync(context), ref context, out var initializer);

            if (!Accept<SemiColonToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return Fatal();

            // TODO: 이 CondExp의 끝은 ';' 이다            
            Parse(await parser.ParseExpAsync(context), ref context, out var cond);

            if (!Accept<SemiColonToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return Fatal();

            // TODO: 이 CondExp의 끝은 ')' 이다            
            Parse(await parser.ParseExpAsync(context), ref context, out var cont);
            
            if (!Accept<RParenToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return Fatal();

            if (!Parse(await ParseEmbeddableStmtAsync(context), ref context, out var bodyStmt))
                return Fatal();

            return new ParseResult<ForStmt>(new ForStmt(initializer, cond, cont, bodyStmt!), context);

            static ParseResult<ForStmt> Invalid() => ParseResult<ForStmt>.Invalid;
            static ParseResult<ForStmt> Fatal() => throw new ParseFatalException();
        }

        internal async ValueTask<ParseResult<ContinueStmt>> ParseContinueStmtAsync(ParserContext context)
        {
            if (!Accept<ContinueToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return ParseResult<ContinueStmt>.Invalid;

            if (!Accept<SemiColonToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return ParseResult<ContinueStmt>.Invalid;

            return new ParseResult<ContinueStmt>(new ContinueStmt(), context);
        }

        internal async ValueTask<ParseResult<BreakStmt>> ParseBreakStmtAsync(ParserContext context)
        {
            if (!Accept<BreakToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return ParseResult<BreakStmt>.Invalid;

            if (!Accept<SemiColonToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return ParseResult<BreakStmt>.Invalid;

            return new ParseResult<BreakStmt>(new BreakStmt(), context);
        }

        internal async ValueTask<ParseResult<ReturnStmt>> ParseReturnStmtAsync(ParserContext context)
        {
            if (!Accept<ReturnToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return ParseResult<ReturnStmt>.Invalid;

            ReturnValueInfo? returnValue = null;            
            if (Accept<RefToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
            {
                if (!Parse(await parser.ParseExpAsync(context), ref context, out var refReturnExp))
                    return ParseResult<ReturnStmt>.Invalid;

                returnValue = new ReturnValueInfo(true, refReturnExp);
            }
            else if (Parse(await parser.ParseExpAsync(context), ref context, out var returnExp))
            {
                returnValue = new ReturnValueInfo(false, returnExp);
            }
            else
            {
                returnValue = null;
            }
            
            if (!Accept<SemiColonToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return ParseResult<ReturnStmt>.Invalid;

            return new ParseResult<ReturnStmt>(new ReturnStmt(returnValue), context);
        }

        internal async ValueTask<ParseResult<BlockStmt>> ParseBlockStmtAsync(ParserContext context)
        {
            if (!Accept<LBraceToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return ParseResult<BlockStmt>.Invalid;

            var stmtsBuilder = ImmutableArray.CreateBuilder<Stmt>();
            while (!Accept<RBraceToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
            {
                if (Parse(await ParseStmtAsync(context), ref context, out var stmt))
                {
                    stmtsBuilder.Add(stmt!);
                    continue;
                }

                return ParseResult<BlockStmt>.Invalid;
            }

            return new ParseResult<BlockStmt>(new BlockStmt(stmtsBuilder.ToImmutable()), context);
        }

        internal async ValueTask<ParseResult<BlankStmt>> ParseBlankStmtAsync(ParserContext context)
        {
            if (!Accept<SemiColonToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return ParseResult<BlankStmt>.Invalid;

            return new ParseResult<BlankStmt>(new BlankStmt(), context);
        }

        // TODO: Assign, Call만 가능하게 해야 한다
        internal async ValueTask<ParseResult<ExpStmt>> ParseExpStmtAsync(ParserContext context)
        {
            if (!Parse(await parser.ParseExpAsync(context), ref context, out var exp))
                return ParseResult<ExpStmt>.Invalid;

            if (!Accept<SemiColonToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return ParseResult<ExpStmt>.Invalid;

            return new ParseResult<ExpStmt>(new ExpStmt(exp!), context);
        }

        async ValueTask<ParseResult<TaskStmt>> ParseTaskStmtAsync(ParserContext context)
        {
            if (!Accept<TaskToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return ParseResult<TaskStmt>.Invalid;
            
            if (!Parse(await parser.ParseBodyAsync(context), ref context, out var body))
                return ParseResult<TaskStmt>.Invalid; 

            return new ParseResult<TaskStmt>(new TaskStmt(body), context);
        }

        async ValueTask<ParseResult<AwaitStmt>> ParseAwaitStmtAsync(ParserContext context)
        {
            if (!Accept<AwaitToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return ParseResult<AwaitStmt>.Invalid;

            if (!Parse(await parser.ParseBodyAsync(context), ref context, out var body))
                return ParseResult<AwaitStmt>.Invalid;

            return new ParseResult<AwaitStmt>(new AwaitStmt(body), context);
        }

        async ValueTask<ParseResult<AsyncStmt>> ParseAsyncStmtAsync(ParserContext context)
        {
            if (!Accept<AsyncToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return ParseResult<AsyncStmt>.Invalid;

            if (!Parse(await parser.ParseBodyAsync(context), ref context, out var body))
                return ParseResult<AsyncStmt>.Invalid;

            return new ParseResult<AsyncStmt>(new AsyncStmt(body), context);
        }

        async ValueTask<ParseResult<YieldStmt>> ParseYieldStmtAsync(ParserContext context)
        {
            if (!Accept<YieldToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return ParseResult<YieldStmt>.Invalid;

            if (!Parse(await parser.ParseExpAsync(context), ref context, out var yieldValue))
                return ParseResult<YieldStmt>.Invalid;

            if (!Accept<SemiColonToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return ParseResult<YieldStmt>.Invalid;

            return new ParseResult<YieldStmt>(new YieldStmt(yieldValue!), context);
        }

        async ValueTask<ParseResult<StringExp>> ParseSingleCommandAsync(ParserContext context, bool bStopRBrace)
        {
            var elemsBuilder = ImmutableArray.CreateBuilder<StringExpElement>();

            // 새 줄이거나 끝에 다다르면 종료
            while (!context.LexerContext.Pos.IsReachEnd())
            {
                if (bStopRBrace && Peek<RBraceToken>(await lexer.LexCommandModeAsync(context.LexerContext)))
                    break;

                if (Accept<NewLineToken>(await lexer.LexCommandModeAsync(context.LexerContext), ref context))
                    break;

                // ${ 이 나오면 
                if (Accept<DollarLBraceToken>(await lexer.LexCommandModeAsync(context.LexerContext), ref context))
                {
                    // TODO: EndInnerExpToken 일때 빠져나와야 한다는 표시를 해줘야 한다
                    if (!Parse(await parser.ParseExpAsync(context), ref context, out var exp))
                        return ParseResult<StringExp>.Invalid;

                    if (!Accept<RBraceToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                        return ParseResult<StringExp>.Invalid;

                    elemsBuilder.Add(new ExpStringExpElement(exp!));
                    continue;
                }

                // aa$b => $b 이야기
                if (Accept<IdentifierToken>(await lexer.LexCommandModeAsync(context.LexerContext), ref context, out var idToken))
                {
                    elemsBuilder.Add(new ExpStringExpElement(new IdentifierExp(idToken!.Value, default)));
                    continue;
                }

                if (Accept<TextToken>(await lexer.LexCommandModeAsync(context.LexerContext), ref context, out var textToken))
                {
                    elemsBuilder.Add(new TextStringExpElement(textToken!.Text));
                    continue;
                }

                return ParseResult<StringExp>.Invalid;
            }
            
            return new ParseResult<StringExp>(new StringExp(elemsBuilder.ToImmutable()), context);
        }

        internal async ValueTask<ParseResult<ForeachStmt>> ParseForeachStmtAsync(ParserContext context)
        {
            // foreach
            if (!Accept<ForeachToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return ParseResult<ForeachStmt>.Invalid;
            
            // (
            if (!Accept<LParenToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return ParseResult<ForeachStmt>.Invalid;

            bool bRef = Accept<RefToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context);

            // var 
            if (!Parse(await parser.ParseTypeExpAsync(context), ref context, out var typeExp))
                return ParseResult<ForeachStmt>.Invalid;

            // x
            if (!Accept<IdentifierToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context, out var varNameToken))
                return ParseResult<ForeachStmt>.Invalid;

            // in
            if (!Accept<InToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return ParseResult<ForeachStmt>.Invalid;

            // obj
            if (!Parse(await parser.ParseExpAsync(context), ref context, out var obj))
                return ParseResult<ForeachStmt>.Invalid;

            // )
            if (!Accept<RParenToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return ParseResult<ForeachStmt>.Invalid;

            // stmt
            if (!Parse(await parser.ParseEmbeddableStmtAsync(context), ref context, out var stmt))
                return ParseResult<ForeachStmt>.Invalid;

            return new ParseResult<ForeachStmt>(new ForeachStmt(bRef, typeExp!, varNameToken!.Value, obj!, stmt), context);
        }        

        // 
        internal async ValueTask<ParseResult<CommandStmt>> ParseCommandStmtAsync(ParserContext context)
        {
            // exec, @로 시작한다
            if (!Accept<ExecToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return ParseResult<CommandStmt>.Invalid;

            // TODO: optional ()

            // {로 시작한다면 MultiCommand, } 가 나오면 끝난다
            if (Accept<LBraceToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
            {
                // 새줄이거나 끝에 다다르거나 }가 나오면 종료, 
                var cmdsBuilder = ImmutableArray.CreateBuilder<StringExp>();
                while (true)
                {
                    if (Accept<RBraceToken>(await lexer.LexCommandModeAsync(context.LexerContext), ref context))
                        break;

                    if (Parse(await ParseSingleCommandAsync(context, true), ref context, out var singleCommand))                    
                    {
                        // singleCommand Skip 조건
                        if (singleCommand!.Elements.Length == 0)
                            continue;

                        if (singleCommand.Elements.Length == 1 &&
                            singleCommand.Elements[0] is TextStringExpElement textElem &&
                            string.IsNullOrWhiteSpace(textElem.Text))
                            continue;

                        cmdsBuilder.Add(singleCommand);
                        continue;
                    }

                    return ParseResult<CommandStmt>.Invalid;
                }

                return new ParseResult<CommandStmt>(new CommandStmt(cmdsBuilder.ToImmutable()), context);
            }
            else // 싱글 커맨드, 엔터가 나오면 끝난다
            {
                if (Parse(await ParseSingleCommandAsync(context, false), ref context, out var singleCommand) && 0 < singleCommand!.Elements.Length)
                    return new ParseResult<CommandStmt>(new CommandStmt(ImmutableArray.Create(singleCommand)), context);

                return ParseResult<CommandStmt>.Invalid;
            }
        }

        async ValueTask<ParseResult<DirectiveStmt>> ParseDirectiveStmtAsync(ParserContext context)
        {
            // ` <id> ( exp... );
            if (!Accept<BacktickToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return ParseResult<DirectiveStmt>.Invalid;

            if (!Accept<IdentifierToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context, out var idToken))
                return ParseResult<DirectiveStmt>.Invalid;

            if (!Accept<LParenToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return ParseResult<DirectiveStmt>.Invalid;

            var argsBuilder = ImmutableArray.CreateBuilder<Exp>();
            while(!Accept<RParenToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
            {
                if (0 < argsBuilder.Count)
                    if (!Accept<CommaToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                        return ParseResult<DirectiveStmt>.Invalid;

                if (!Parse(await parser.ParseExpAsync(context), ref context, out var arg))
                    return ParseResult<DirectiveStmt>.Invalid;

                argsBuilder.Add(arg);
            }

            if (!Accept<SemiColonToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return ParseResult<DirectiveStmt>.Invalid;

            return new ParseResult<DirectiveStmt>(new DirectiveStmt(idToken.Value, argsBuilder.ToImmutable()), context);
        }

        // if (...) 'x;' // 단일이냐
        // if (...) '{ }' // 묶음이냐
        public async ValueTask<ParseResult<EmbeddableStmt>> ParseEmbeddableStmtAsync(ParserContext context)
        {
            // { 가 없다면, Embeddable.Single
            if (!Accept<LBraceToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
            {
                if (Parse(await ParseStmtAsync(context), ref context, out var stmt))
                {
                    // block stmt는 제외되서 들어올 것이다
                    Debug.Assert(stmt is not BlockStmt);
                    return new ParseResult<EmbeddableStmt>(new EmbeddableStmt.Single(stmt), context);
                }

                return ParseResult<EmbeddableStmt>.Invalid;
            }
            else // 있다면 Embeddable.Multiple
            {
                var stmtsBuilder = ImmutableArray.CreateBuilder<Stmt>();

                // } 가 나올때까지
                while(!Accept<RBraceToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                {
                    if (Parse(await ParseStmtAsync(context), ref context, out var stmt))
                        stmtsBuilder.Add(stmt);                    
                    else
                        return ParseResult<EmbeddableStmt>.Invalid;
                }

                return new ParseResult<EmbeddableStmt>(new EmbeddableStmt.Multiple(stmtsBuilder.ToImmutable()), context);
            }
        }

        public async ValueTask<ParseResult<ImmutableArray<Stmt>>> ParseBodyAsync(ParserContext context)
        {
            if (!Accept<LBraceToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
                return ParseResult<ImmutableArray<Stmt>>.Invalid;

            var stmtsBuilder = ImmutableArray.CreateBuilder<Stmt>();
            while (!Accept<RBraceToken>(await lexer.LexNormalModeAsync(context.LexerContext, true), ref context))
            {
                if (Parse(await ParseStmtAsync(context), ref context, out var stmt))
                {
                    stmtsBuilder.Add(stmt!);
                    continue;
                }

                return ParseResult<ImmutableArray<Stmt>>.Invalid;
            }

            return new ParseResult<ImmutableArray<Stmt>>(stmtsBuilder.ToImmutable(), context);
        }

        public async ValueTask<ParseResult<Stmt>> ParseStmtAsync(ParserContext context)
        {
            if (Parse(await ParseDirectiveStmtAsync(context), ref context, out var directiveStmt))
                return new ParseResult<Stmt>(directiveStmt, context);

            if (Parse(await ParseBlankStmtAsync(context), ref context, out var blankStmt))
                return new ParseResult<Stmt>(blankStmt,context);

            if (Parse(await ParseBlockStmtAsync(context), ref context, out var blockStmt))
                return new ParseResult<Stmt>(blockStmt,context);

            if (Parse(await ParseContinueStmtAsync(context), ref context, out var continueStmt))
                return new ParseResult<Stmt>(continueStmt,context);

            if (Parse(await ParseBreakStmtAsync(context), ref context, out var breakStmt))
                return new ParseResult<Stmt>(breakStmt,context);

            if (Parse(await ParseReturnStmtAsync(context), ref context, out var returnStmt))
                return new ParseResult<Stmt>(returnStmt,context);

            if (Parse(await ParseVarDeclStmtAsync(context), ref context, out var varDeclStmt))
                return new ParseResult<Stmt>(varDeclStmt,context);

            if (Parse(await ParseIfStmtAsync(context), ref context, out var ifStmt))
                return new ParseResult<Stmt>(ifStmt,context);

            if (Parse(await ParseForStmtAsync(context), ref context, out var forStmt))
                return new ParseResult<Stmt>(forStmt,context);

            if (Parse(await ParseExpStmtAsync(context), ref context, out var expStmt))
                return new ParseResult<Stmt>(expStmt,context);

            if (Parse(await ParseTaskStmtAsync(context), ref context, out var taskStmt))
                return new ParseResult<Stmt>(taskStmt,context);

            if (Parse(await ParseAwaitStmtAsync(context), ref context, out var awaitStmt))
                return new ParseResult<Stmt>(awaitStmt,context);

            if (Parse(await ParseAsyncStmtAsync(context), ref context, out var asyncStmt))
                return new ParseResult<Stmt>(asyncStmt,context);

            if (Parse(await ParseForeachStmtAsync(context), ref context, out var foreachStmt))
                return new ParseResult<Stmt>(foreachStmt,context);

            if (Parse(await ParseYieldStmtAsync(context), ref context, out var yieldStmt))
                return new ParseResult<Stmt>(yieldStmt,context);

            if (Parse(await ParseCommandStmtAsync(context), ref context, out var cmdStmt))
                return new ParseResult<Stmt>(cmdStmt,context);

            throw new NotImplementedException();
        }
    }
}