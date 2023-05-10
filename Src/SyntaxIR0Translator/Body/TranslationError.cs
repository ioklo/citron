using Citron.Syntax;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Citron.Analysis;

enum TranslationResultKind
{   
    Valid,
    Error,
}

static class TranslationResult
{
    public static TranslationResult<TResult> Valid<TResult>(TResult result)
    {
        return new TranslationResult<TResult>(TranslationResultKind.Valid, result);
    }

    public static TranslationResult<TResult> Error<TResult>()
    {
        return new TranslationResult<TResult>(TranslationResultKind.Error, default);
    }
}

record struct TranslationResult<TResult>(TranslationResultKind Kind, TResult? Result)
{   
    public bool IsValid([NotNullWhen(returnValue: true)] out TResult? result)
    {
        if (Kind == TranslationResultKind.Valid)
        {
            Debug.Assert(Result != null);
            result = Result;
            return true;
        }
        else
        {
            result = default;
            return false;
        }
    }
}
