﻿namespace Citron; 

using Citron.Collections;
using Citron.Syntax;
using System.Diagnostics.CodeAnalysis;

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

    bool ParseIdTypeExp([NotNullWhen(returnValue: true)] out TypeExp? outTypeExp)
    {
        var prevContext = context;

        if (!InternalParseIdTypeExp(out outTypeExp))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    bool ParseNullableTypeExp([NotNullWhen(returnValue: true)] out TypeExp? outTypeExp)
    {
        var prevContext = context;

        if (!InternalParseNullableTypeExp(out outTypeExp))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }


    // box T*
    bool ParseBoxPtrTypeExp([NotNullWhen(returnValue: true)] out TypeExp? outTypeExp)
    {
        var prevContext = context;

        if (!InternalParseBoxPtrTypeExp(out outTypeExp))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    // T*
    bool ParseLocalPtrTypeExp([NotNullWhen(returnValue: true)] out TypeExp? outTypeExp)
    {
        var prevContext = context;

        if (!InternalParseLocalPtrTypeExp(out outTypeExp))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    // (T)
    bool ParseParenTypeExp([NotNullWhen(returnValue: true)] out TypeExp? outTypeExp)
    {
        var prevContext = context;

        if (!InternalParseParenTypeExp(out outTypeExp))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    // ID...
    bool ParseIdChainTypeExp([NotNullWhen(returnValue: true)] out TypeExp? outTypeExp)
    {
        var prevContext = context;

        if (!InternalParseIdChainTypeExp(out outTypeExp))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    // func<>
    // bool ParseFuncTypeExp([NotNullWhen(returnValue: true)] out TypeExp? outTypeExp);

    // tuple
    // bool ParseTupleTypeExp([NotNullWhen(returnValue: true)] out TypeExp? outTypeExp);

    // local modifier
    bool ParseLocalTypeExp([NotNullWhen(returnValue: true)] out TypeExp? outTypeExp)
    {
        var prevContext = context;

        if (!InternalParseLocalTypeExp(out outTypeExp))
        {
            context = prevContext;
            return false;
        }
        else
        {
            return true;
        }
    }

    // 
    bool ParseTypeExp([NotNullWhen(returnValue: true)] out TypeExp? outTypeExp)
    {
        var prevContext = context;

        if (!InternalParseTypeExp(out outTypeExp))
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
