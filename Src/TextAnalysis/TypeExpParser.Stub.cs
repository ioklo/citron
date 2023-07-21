using Citron.Collections;
using Citron.Syntax;
using System.Diagnostics.CodeAnalysis;

namespace Citron;

partial struct TypeExpParser 
{
    bool ParseTypeArgs([NotNullWhen(returnValue: true)] out ImmutableArray<TypeExp>? outTypeArgs)
    {
        var prevContext = context;

        if (!InternalParseTypeArgs(out outTypeArgs))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    bool ParseTypeIdExp([NotNullWhen(returnValue: true)] out TypeExp? outTypeExp)
    {
        var prevContext = context;

        if (!InternalParseTypeIdExp(out outTypeExp))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    // 최상위
    bool ParseTypeExp0([NotNullWhen(returnValue: true)] out TypeExp? outTypeExp)
    {
        var prevContext = context;

        if (!InternalParseTypeExp0(out outTypeExp))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    // 첫번째, box와 *****
    bool ParseTypeExp1([NotNullWhen(returnValue: true)] out TypeExp? outTypeExp)
    {
        var prevContext = context;

        if (!InternalParseTypeExp1(out outTypeExp))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    bool ParseTypeExp2([NotNullWhen(returnValue: true)] out TypeExp? outTypeExp)
    {
        var prevContext = context;

        if (!InternalParseTypeExp2(out outTypeExp))
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
