﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Citron.LexicalAnalysis;
using Citron.Syntax;
using System.Collections.Generic;
using Citron.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Citron;

partial struct StmtParser
{
    Lexer lexer;
    ParserContext context;

    public static bool Parse(Lexer lexer, ref ParserContext context, [NotNullWhen(returnValue: true)] out Stmt? outStmt)
    {
        var parser = new StmtParser { lexer = lexer, context = context };
        if (!parser.ParseStmt(out outStmt))
            return false;

        context = parser.context;
        return true;
    }

    public static bool ParseBody(Lexer lexer, ref ParserContext context, [NotNullWhen(returnValue: true)] out ImmutableArray<Stmt>? outBody)
    {
        var parser = new StmtParser { lexer = lexer, context = context };
        if (!parser.ParseBody(out outBody))
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

    bool Accept<TToken>(LexResult lexResult, [NotNullWhen(true)] out TToken? token) where TToken : Token
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

    bool Peek(SingleToken token, LexResult lexResult)
    {
        return lexResult.HasValue && lexResult.Token == token;
    }

    // typeExp id = exp)
    bool InternalParseIfTestFragment([NotNullWhen(returnValue: true)] out Stmt? outStmt)
    {
        if (!TypeExpParser.Parse(lexer, ref context, out var testTypeExp) ||
            !Accept<IdentifierToken>(out var varName) ||
            !Accept(Tokens.Equal) ||
            !ExpParser.Parse(lexer, ref context, out var exp) ||
            !Accept(Tokens.RParen))
        {
            outStmt = null;
            return false;
        }

        // right assoc, conflict는 별다른 처리를 하지 않고 지나가면 될 것 같다
        if (!ParseEmbeddableStmt(out var body))
        {
            outStmt = null;
            return false;
        }

        EmbeddableStmt? elseBody = null;
        if (Accept(Tokens.Else))
        {
            if (!ParseEmbeddableStmt(out elseBody))
            {
                outStmt = null;
                return false;
            }
        }

        outStmt = new IfTestStmt(testTypeExp, varName.Value, exp, body, elseBody);
        return true;
    }

    bool InternalParseIfStmt([NotNullWhen(returnValue: true)] out Stmt? outStmt)
    {
        // if (exp) stmt => If(exp, stmt, null)
        // if (exp) stmt0 else stmt1 => If(exp, stmt0, stmt1)
        // if (exp0) if (exp1) stmt1 else stmt2 => If(exp0, If(exp1, stmt1, stmt2))
        // if (typeExp name = exp) => IfTestStmt(TypeExp, name, exp)

        if (!Accept(Tokens.If))
        {
            outStmt = null;
            return false;
        }

        if (!Accept(Tokens.LParen))
        {
            outStmt = null;
            return false;
        }

        // typeExp varName = exp꼴인지 먼저 확인
        if (ParseIfTestFragment(out outStmt))        
            return true;

        // 아니라면
        if (!ExpParser.Parse(lexer, ref context, out var cond))
        {
            outStmt = null;
            return false;
        }
        
        if (!Accept(Tokens.RParen))
        {
            outStmt = null;
            return false;
        }

        // right assoc, conflict는 별다른 처리를 하지 않고 지나가면 될 것 같다
        if (!ParseEmbeddableStmt(out var body))
        {
            outStmt = null;
            return false;
        }

        EmbeddableStmt? elseBody = null;
        if (Accept(Tokens.Else))
        {
            if (!ParseEmbeddableStmt(out elseBody))
            { 
                outStmt = null; 
                return false;
            }
        }

        outStmt = new IfStmt(cond!, body, elseBody);
        return true;
    }

    bool InternalParseVarDecl([NotNullWhen(returnValue: true)] out VarDecl? outVarDecl)
    {
        if (!TypeExpParser.Parse(lexer, ref context, out var varType))
        {
            outVarDecl = null;
            return false;
        }

        var elemsBuilder = ImmutableArray.CreateBuilder<VarDeclElement>();
        do
        {
            if (!Accept<IdentifierToken>(out var varIdResult))
            {
                outVarDecl = null;
                return false;
            }

            Exp? initExp = null;
            if (Accept(Tokens.Equal))
            {
                // TODO: ;나 ,가 나올때까지라는걸 명시해주면 좋겠다
                if (!ExpParser.Parse(lexer, ref context, out initExp))
                {
                    outVarDecl = null;
                    return false;
                }
            }

            elemsBuilder.Add(new VarDeclElement(varIdResult!.Value, initExp));

        } while (Accept(Tokens.Comma)); // ,가 나오면 계속한다

        outVarDecl = new VarDecl(varType!, elemsBuilder.ToImmutable());
        return true;
    }

    // int x = 0;
    bool InternalParseVarDeclStmt([NotNullWhen(returnValue: true)] out VarDeclStmt? outStmt)
    {
        if (!ParseVarDecl(out var varDecl))
        {
            outStmt = null;
            return false;
        }

        if (!context.LexerContext.Pos.IsReachEnd() &&
            !Accept(Tokens.SemiColon)) // ;으로 마무리
        {
            outStmt = null;
            return false;
        }

        outStmt = new VarDeclStmt(varDecl!);
        return true;
    }

    bool InternalParseForStmtInitializer([NotNullWhen(returnValue: true)] out ForStmtInitializer? outInit)
    {
        if (ParseVarDecl(out var varDecl))
        {
            outInit = new VarDeclForStmtInitializer(varDecl!);
            return true;
        }


        if (ExpParser.Parse(lexer, ref context, out var exp))
        {
            outInit = new ExpForStmtInitializer(exp!);
            return true;
        }

        outInit = null;
        return false;
    }

    bool InternalParseForStmt([NotNullWhen(returnValue: true)] out ForStmt? outStmt)
    {
        static bool Fatal() => throw new ParseFatalException();

        if (!Accept(Tokens.For))
        {
            outStmt = null;
            return false;
        }

        if (!Accept(Tokens.LParen))
            return Fatal();

        // TODO: 이 Initializer의 끝은 ';' 이다
        ParseForStmtInitializer(out var initializer);

        if (!Accept(Tokens.SemiColon))
            return Fatal();

        // TODO: 이 CondExp의 끝은 ';' 이다            
        ExpParser.Parse(lexer, ref context, out var cond);

        if (!Accept(Tokens.SemiColon))
            return Fatal();

        // TODO: 이 CondExp의 끝은 ')' 이다            
        ExpParser.Parse(lexer, ref context, out var cont);
        
        if (!Accept(Tokens.RParen))
            return Fatal();

        if (!ParseEmbeddableStmt(out var bodyStmt))
            return Fatal();

        outStmt = new ForStmt(initializer, cond, cont, bodyStmt!);
        return true;
    }

    bool InternalParseContinueStmt([NotNullWhen(returnValue: true)] out ContinueStmt? outStmt)
    {
        if (!Accept(Tokens.Continue))
        {
            outStmt = null;
            return false;
        }

        if (!Accept(Tokens.SemiColon))
        {
            outStmt = null;
            return false;
        }

        outStmt = new ContinueStmt();
        return true;
    }

    bool InternalParseBreakStmt([NotNullWhen(returnValue: true)] out BreakStmt? outStmt)
    {
        if (!Accept(Tokens.Break))
        {
            outStmt = null;
            return false;
        }

        if (!Accept(Tokens.SemiColon))
        {
            outStmt = null;
            return false;
        }

        outStmt = new BreakStmt();
        return true;
    }

    bool InternalParseReturnStmt([NotNullWhen(returnValue: true)] out ReturnStmt? outStmt)
    {
        if (!Accept(Tokens.Return))
        {
            outStmt = null;
            return false;
        }

        ReturnValueInfo? returnValue = null;            
        if (ExpParser.Parse(lexer, ref context, out var returnExp))
        {
            returnValue = new ReturnValueInfo(returnExp);
        }
        else
        {
            returnValue = null;
        }

        if (!Accept(Tokens.SemiColon))
        {
            outStmt = null;
            return false;
        }

        outStmt = new ReturnStmt(returnValue);
        return true;
    }

    bool InternalParseBlockStmt([NotNullWhen(returnValue: true)] out BlockStmt? outStmt)
    {
        if (!Accept(Tokens.LBrace))
        {
            outStmt = null;
            return false;
        }

        var stmtsBuilder = ImmutableArray.CreateBuilder<Stmt>();
        while (!Accept(Tokens.RBrace))
        {
            if (ParseStmt(out var stmt))
            {
                stmtsBuilder.Add(stmt!);
                continue;
            }

            outStmt = null;
            return false;
        }

        outStmt = new BlockStmt(stmtsBuilder.ToImmutable());
        return true;
    }

    bool InternalParseBlankStmt([NotNullWhen(returnValue: true)] out BlankStmt? outStmt)
    {
        if (!Accept(Tokens.SemiColon))
        {
            outStmt = null;
            return false;
        }

        outStmt = new BlankStmt();
        return true;
    }

    // TODO: Assign, Call만 가능하게 해야 한다
    bool InternalParseExpStmt([NotNullWhen(returnValue: true)] out ExpStmt? outStmt)
    {
        if (!ExpParser.Parse(lexer, ref context, out var exp))
        {
            outStmt = null;
            return false;
        }

        if (!Accept(Tokens.SemiColon))
        {
            outStmt = null;
            return false;
        }

        outStmt = new ExpStmt(exp);
        return true;
    }

    bool InternalParseTaskStmt([NotNullWhen(returnValue: true)] out TaskStmt? outStmt)
    {
        if (!Accept(Tokens.Task))
        {
            outStmt = null;
            return false;
        }

        if (!ParseBody(out var body))
        {
            outStmt = null;
            return false;
        }

        outStmt = new TaskStmt(body.Value);
        return true;
    }

    bool InternalParseAwaitStmt([NotNullWhen(returnValue: true)] out AwaitStmt? outStmt)
    {
        if (!Accept(Tokens.Await))
        {
            outStmt = null;
            return false;
        }

        if (!ParseBody(out var body))
        {
            outStmt = null;
            return false;
        }

        outStmt = new AwaitStmt(body.Value);
        return true;
    }

    bool InternalParseAsyncStmt([NotNullWhen(returnValue: true)] out AsyncStmt? outStmt)
    {
        if (!Accept(Tokens.Async))
        {
            outStmt = null;
            return false;
        }

        if (!ParseBody(out var body))
        {
            outStmt = null;
            return false;
        }

        outStmt = new AsyncStmt(body.Value);
        return true;
    }

    bool InternalParseYieldStmt([NotNullWhen(returnValue: true)] out YieldStmt? outStmt)
    {
        if (!Accept(Tokens.Yield))
        {
            outStmt = null;
            return false;
        }

        if (!ExpParser.Parse(lexer, ref context, out var yieldValue))
        {
            outStmt = null;
            return false;
        }

        if (!Accept(Tokens.SemiColon))
        {
            outStmt = null;
            return false;
        }

        outStmt = new YieldStmt(yieldValue!);
        return true;
    }

    bool InternalParseSingleCommand(bool bStopRBrace, [NotNullWhen(returnValue: true)] out StringExp? outExp)
    {
        var elemsBuilder = ImmutableArray.CreateBuilder<StringExpElement>();

        // 새 줄이거나 끝에 다다르면 종료
        while (!context.LexerContext.Pos.IsReachEnd())
        {
            if (bStopRBrace && Peek(Tokens.RBrace, lexer.LexCommandMode(context.LexerContext)))
                break;

            if (Accept(Tokens.NewLine, lexer.LexCommandMode(context.LexerContext)))
                break;

            // ${ 이 나오면 
            if (Accept(Tokens.DollarLBrace, lexer.LexCommandMode(context.LexerContext)))
            {
                // TODO: EndInnerExpToken 일때 빠져나와야 한다는 표시를 해줘야 한다
                if (!ExpParser.Parse(lexer, ref context, out var exp))
                {
                    outExp = null;
                    return false;
                }

                if (!Accept(Tokens.RBrace))
                {
                    outExp = null;
                    return false;
                }

                elemsBuilder.Add(new ExpStringExpElement(exp!));
                continue;
            }

            // aa$b => $b 이야기
            if (Accept<IdentifierToken>(lexer.LexCommandMode(context.LexerContext), out var idToken))
            {
                elemsBuilder.Add(new ExpStringExpElement(new IdentifierExp(idToken!.Value, default)));
                continue;
            }

            if (Accept<TextToken>(lexer.LexCommandMode(context.LexerContext), out var textToken))
            {
                elemsBuilder.Add(new TextStringExpElement(textToken!.Text));
                continue;
            }

            outExp = null;
            return false;
        }


        outExp = new StringExp(elemsBuilder.ToImmutable());
        return true;
    }

    bool InternalParseForeachStmt([NotNullWhen(returnValue: true)] out ForeachStmt? outStmt)
    {
        // foreach
        if (!Accept(Tokens.Foreach))
        {
            outStmt = null;
            return false;
        }

        // (
        if (!Accept(Tokens.LParen))
        {
            outStmt = null;
            return false;
        }

        // var 
        if (!TypeExpParser.Parse(lexer, ref context, out var typeExp))
        {
            outStmt = null;
            return false;
        }

        // x
        if (!Accept<IdentifierToken>(out var varNameToken))
        {
            outStmt = null;
            return false;
        }

        // in
        if (!Accept(Tokens.In))
        {
            outStmt = null;
            return false;
        }

        // obj
        if (!ExpParser.Parse(lexer, ref context, out var obj))
        {
            outStmt = null;
            return false;
        }

        // )
        if (!Accept(Tokens.RParen))
        {
            outStmt = null;
            return false;
        }

        // stmt
        if (!ParseEmbeddableStmt(out var stmt))
        {
            outStmt = null;
            return false;
        }

        outStmt = new ForeachStmt(typeExp!, varNameToken!.Value, obj!, stmt);
        return true;
    }        

    // 
    bool InternalParseCommandStmt([NotNullWhen(returnValue: true)] out CommandStmt? outStmt)
    {
        // @로 시작한다
        if (!Accept(Tokens.At))
        {
            outStmt = null;
            return false;
        }

        // TODO: optional ()

        // {로 시작한다면 MultiCommand, } 가 나오면 끝난다
        if (Accept(Tokens.LBrace))
        {
            // 새줄이거나 끝에 다다르거나 }가 나오면 종료, 
            var cmdsBuilder = ImmutableArray.CreateBuilder<StringExp>();
            while (true)
            {
                if (Accept(Tokens.RBrace, lexer.LexCommandMode(context.LexerContext)))
                    break;

                if (ParseSingleCommand(true, out var singleCommand))
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

                outStmt = null;
                return false;
            }

            outStmt = new CommandStmt(cmdsBuilder.ToImmutable());
            return true;
        }
        else // 싱글 커맨드, 엔터가 나오면 끝난다
        {
            if (ParseSingleCommand(false, out var singleCommand) && 0 < singleCommand!.Elements.Length)
            {
                outStmt = new CommandStmt(ImmutableArray.Create(singleCommand));
                return true;
            }

            outStmt = null;
            return false;
        }
    }

    bool InternalParseDirectiveStmt([NotNullWhen(returnValue: true)] out DirectiveStmt? outStmt)
    {
        // ` <id> ( exp... );
        if (!Accept(Tokens.Backtick))
        {
            outStmt = null;
            return false;
        }

        if (!Accept<IdentifierToken>(out var idToken))
        {
            outStmt = null;
            return false;
        }

        if (!Accept(Tokens.LParen))
        {
            outStmt = null;
            return false;
        }

        var argsBuilder = ImmutableArray.CreateBuilder<Exp>();
        while(!Accept(Tokens.RParen))
        {
            if (0 < argsBuilder.Count)
                if (!Accept(Tokens.Comma))
                {
                    outStmt = null;
                    return false;
                }

            if (!ExpParser.Parse(lexer, ref context, out var arg))
            {
                outStmt = null;
                return false;
            }

            argsBuilder.Add(arg);
        }

        if (!Accept(Tokens.SemiColon))
        {
            outStmt = null;
            return false;
        }

        outStmt = new DirectiveStmt(idToken.Value, argsBuilder.ToImmutable());
        return true;
    }

    // if (...) 'x;' // 단일이냐
    // if (...) '{ }' // 묶음이냐
    bool InternalParseEmbeddableStmt([NotNullWhen(returnValue: true)] out EmbeddableStmt? outStmt)
    {
        // { 가 없다면, Embeddable.Single
        if (!Accept(Tokens.LBrace))
        {
            if (ParseStmt(out var stmt))
            {
                // block stmt는 제외되서 들어올 것이다
                Debug.Assert(stmt is not BlockStmt);
                outStmt = new EmbeddableStmt.Single(stmt);
                return true;
            }

            outStmt = null;
            return false;
        }
        else // 있다면 Embeddable.Multiple
        {
            var stmtsBuilder = ImmutableArray.CreateBuilder<Stmt>();

            // } 가 나올때까지
            while(!Accept(Tokens.RBrace))
            {
                if (ParseStmt(out var stmt))
                    stmtsBuilder.Add(stmt);
                else
                {
                    outStmt = null;
                    return false;
                }
            }

            outStmt = new EmbeddableStmt.Multiple(stmtsBuilder.ToImmutable());
            return true;
        }
    }

    bool InternalParseBody([NotNullWhen(returnValue: true)] out ImmutableArray<Stmt>? outBody)
    {
        if (!Accept(Tokens.LBrace))
        {
            outBody = null;
            return false;
        }

        var stmtsBuilder = ImmutableArray.CreateBuilder<Stmt>();
        while (!Accept(Tokens.RBrace))
        {
            if (ParseStmt(out var stmt))
            {
                stmtsBuilder.Add(stmt!);
                continue;
            }

            outBody = null;
            return false;
        }

        outBody = stmtsBuilder.ToImmutable();
        return true;
    }

    bool InternalParseStmt([NotNullWhen(returnValue: true)] out Stmt? outStmt)
    {
        if (ParseDirectiveStmt(out var directiveStmt))
        {
            outStmt = directiveStmt;
            return true;
        }

        if (ParseBlankStmt(out var blankStmt))
        {
            outStmt = blankStmt;
            return true;
        }

        if (ParseBlockStmt(out var blockStmt))
        {
            outStmt = blockStmt;
            return true;
        }

        if (ParseContinueStmt(out var continueStmt))
        {
            outStmt = continueStmt;
            return true;
        }

        if (ParseBreakStmt(out var breakStmt))
        {
            outStmt = breakStmt;
            return true;
        }

        if (ParseReturnStmt(out var returnStmt))
        {
            outStmt = returnStmt;
            return true;
        }

        if (ParseVarDeclStmt(out var varDeclStmt))
        {
            outStmt = varDeclStmt;
            return true;
        }

        if (ParseIfStmt(out var ifStmt))
        {
            outStmt = ifStmt;
            return true;
        }

        if (ParseForStmt(out var forStmt))
        {
            outStmt = forStmt;
            return true;
        }

        if (ParseExpStmt(out var expStmt))
        {
            outStmt = expStmt;
            return true;
        }

        if (ParseTaskStmt(out var taskStmt))
        {
            outStmt = taskStmt;
            return true;
        }

        if (ParseAwaitStmt(out var awaitStmt))
        {
            outStmt = awaitStmt;
            return true;
        }

        if (ParseAsyncStmt(out var asyncStmt))
        {
            outStmt = asyncStmt;
            return true;
        }

        if (ParseForeachStmt(out var foreachStmt))
        {
            outStmt = foreachStmt;
            return true;
        }

        if (ParseYieldStmt(out var yieldStmt))
        {
            outStmt = yieldStmt;
            return true;
        }

        if (ParseCommandStmt(out var cmdStmt))
        {
            outStmt = cmdStmt;
            return true;
        }

        outStmt = null;
        return false;
    }
}