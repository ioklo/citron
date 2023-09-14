using Citron.Collections;
using Citron.Syntax;
using System.Diagnostics.CodeAnalysis;

namespace Citron;

partial struct StmtParser
{
    bool ParseIfTestFragment([NotNullWhen(returnValue: true)] out Stmt? outStmt)
    {
        var prevContext = context;

        if (!InternalParseIfTestFragment(out outStmt))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    bool ParseIfStmt([NotNullWhen(returnValue: true)] out Stmt? outStmt)
    {
        var prevContext = context;

        if (!InternalParseIfStmt(out outStmt))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    bool ParseVarDecl([NotNullWhen(returnValue: true)] out VarDecl? outVarDecl)
    {
        var prevContext = context;

        if (!InternalParseVarDecl(out outVarDecl))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    // int x = 0;
    bool ParseVarDeclStmt([NotNullWhen(returnValue: true)] out VarDeclStmt? outStmt)
    {
        var prevContext = context;

        if (!InternalParseVarDeclStmt(out outStmt))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    bool ParseForStmtInitializer([NotNullWhen(returnValue: true)] out ForStmtInitializer? outInit)
    {
        var prevContext = context;

        if (!InternalParseForStmtInitializer(out outInit))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    bool ParseForStmt([NotNullWhen(returnValue: true)] out ForStmt? outStmt)
    {
        var prevContext = context;

        if (!InternalParseForStmt(out outStmt))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    bool ParseContinueStmt([NotNullWhen(returnValue: true)] out ContinueStmt? outStmt)
    {
        var prevContext = context;

        if (!InternalParseContinueStmt(out outStmt))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    bool ParseBreakStmt([NotNullWhen(returnValue: true)] out BreakStmt? outStmt)
    {
        var prevContext = context;

        if (!InternalParseBreakStmt(out outStmt))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    bool ParseReturnStmt([NotNullWhen(returnValue: true)] out ReturnStmt? outStmt)
    {
        var prevContext = context;

        if (!InternalParseReturnStmt(out outStmt))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    bool ParseBlockStmt([NotNullWhen(returnValue: true)] out BlockStmt? outStmt)
    {
        var prevContext = context;

        if (!InternalParseBlockStmt(out outStmt))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    bool ParseBlankStmt([NotNullWhen(returnValue: true)] out BlankStmt? outStmt)
    {
        var prevContext = context;

        if (!InternalParseBlankStmt(out outStmt))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    bool ParseExpStmt([NotNullWhen(returnValue: true)] out ExpStmt? outStmt)
    {
        var prevContext = context;

        if (!InternalParseExpStmt(out outStmt))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    bool ParseTaskStmt([NotNullWhen(returnValue: true)] out TaskStmt? outStmt)
    {
        var prevContext = context;

        if (!InternalParseTaskStmt(out outStmt))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    bool ParseAwaitStmt([NotNullWhen(returnValue: true)] out AwaitStmt? outStmt)
    {
        var prevContext = context;

        if (!InternalParseAwaitStmt(out outStmt))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    bool ParseAsyncStmt([NotNullWhen(returnValue: true)] out AsyncStmt? outStmt)
    {
        var prevContext = context;

        if (!InternalParseAsyncStmt(out outStmt))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    bool ParseYieldStmt([NotNullWhen(returnValue: true)] out YieldStmt? outStmt)
    {
        var prevContext = context;

        if (!InternalParseYieldStmt(out outStmt))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    bool ParseSingleCommand(bool bStopRBrace, [NotNullWhen(returnValue: true)] out StringExp? outExp)
    {
        var prevContext = context;

        if (!InternalParseSingleCommand(bStopRBrace, out outExp))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    bool ParseForeachStmt([NotNullWhen(returnValue: true)] out ForeachStmt? outStmt)
    {
        var prevContext = context;

        if (!InternalParseForeachStmt(out outStmt))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    bool ParseCommandStmt([NotNullWhen(returnValue: true)] out CommandStmt? outStmt)
    {
        var prevContext = context;

        if (!InternalParseCommandStmt(out outStmt))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    bool ParseDirectiveStmt([NotNullWhen(returnValue: true)] out DirectiveStmt? outStmt)
    {
        var prevContext = context;

        if (!InternalParseDirectiveStmt(out outStmt))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    // if (...) 'x;' // 단일이냐
    // if (...) '{ }' // 묶음이냐
    bool ParseEmbeddableStmt([NotNullWhen(returnValue: true)] out EmbeddableStmt? outStmt)
    {
        var prevContext = context;

        if (!InternalParseEmbeddableStmt(out outStmt))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    bool ParseBody([NotNullWhen(returnValue: true)] out ImmutableArray<Stmt>? outBody)
    {
        var prevContext = context;

        if (!InternalParseBody(out outBody))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    bool ParseStmt([NotNullWhen(returnValue: true)] out Stmt? outStmt)
    {
        var prevContext = context;

        if (!InternalParseStmt(out outStmt))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }
}
